﻿
using System.Buffers;
using System.Runtime.CompilerServices;
using CsToml.Extensions.Extension;
using CsToml.Extensions.Utility;
using Cysharp.Collections;
using Microsoft.Win32.SafeHandles;

namespace CsToml.Extensions;

public partial class CsTomlFileSerializer
{
    private static readonly string TomlExtension = ".toml";

    public static TPackage Deserialize<TPackage>(string tomlFilePath, CsTomlSerializerOptions? options = null)
        where TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>
    {
        if (Path.GetExtension(tomlFilePath) != TomlExtension)
            throw new FormatException($"TOML files should use the extension .toml");

        using var handle = File.OpenHandle(tomlFilePath!, FileMode.Open, FileAccess.Read, options: FileOptions.SequentialScan);
        var length = RandomAccess.GetLength(handle);

        if (length == 0)
        {
            return TPackage.CreatePackage() ?? throw new NullReferenceException($"{typeof(TPackage)} was not created.");
        }

        if (length < Array.MaxLength)
        {
            using var bytes = new NativeMemoryArray<byte>(length);
            var bytesSpan = bytes.AsSpan();
            var readCount = RandomAccess.Read(handle, bytesSpan, 0);

            // check BOM
            var startIndex = Utf8Helper.ContainBOM(bytesSpan) ? 3 : 0;
            var tomlByteSpan = bytesSpan.Slice(startIndex, readCount - startIndex);
            return CsTomlSerializer.Deserialize<TPackage>(tomlByteSpan, options);
        }
        else
        {
            using var bytes = new NativeMemoryArray<byte>(length);
            var tomlMemories = bytes.AsMemoryList();
            RandomAccess.Read(handle, tomlMemories, 0);

            // check BOM
            var memory = tomlMemories[0];
            var startIndex = Utf8Helper.ContainBOM(memory.Span) ? 3 : 0;

            var startSegment = new ByteSequenceSegment(memory[startIndex..]);
            startSegment.SetRunningIndex(0);
            startSegment.SetNext(null);

            var endSegment = startSegment;
            for (int i = 1; i < tomlMemories.Count; i++)
            {
                endSegment = endSegment.AddNext(tomlMemories[i]);
            }

            var sequence = new ReadOnlySequence<byte>(startSegment, 0, endSegment, endSegment.Length);
            return CsTomlSerializer.Deserialize<TPackage>(sequence, options);
        }
    }

    public static ValueTask<TPackage> DeserializeAsync<TPackage>(string tomlFilePath, CsTomlSerializerOptions? options = null, bool configureAwait = false, CancellationToken cancellationToken = default)
        where TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>
    {
        if (Path.GetExtension(tomlFilePath) != TomlExtension)
            throw new FormatException($"TOML files should use the extension .toml");

        cancellationToken.ThrowIfCancellationRequested();
        using var handle = File.OpenHandle(tomlFilePath!, FileMode.Open, FileAccess.Read, options: FileOptions.SequentialScan | FileOptions.Asynchronous);
        var length = RandomAccess.GetLength(handle);

        if (length == 0)
        {
            return TPackage.CreatePackage()?.FromResult() ?? throw new NullReferenceException($"{typeof(TPackage)} was not created."); ;
        }
        else if (length <= Array.MaxLength)
        {
            return ReadAsync(handle, (int)length, options, configureAwait, cancellationToken);
        }
        else
        {
            return ReadSequenceAsync(handle, length, options, configureAwait, cancellationToken);
        }

        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
        async ValueTask<TPackage> ReadAsync(SafeFileHandle handle, int arrayLength, CsTomlSerializerOptions? options, bool configureAwait, CancellationToken cancellationToken)
        {
            using var bytes = new NativeMemoryArray<byte>(arrayLength);
            var byteMemories = bytes.AsMemory();
            var readCount = await RandomAccess.ReadAsync(handle, byteMemories, 0, cancellationToken).ConfigureAwait(configureAwait);

            var startIndex = Utf8Helper.ContainBOM(byteMemories.Span) ? 3 : 0;
            return CsTomlSerializer.Deserialize<TPackage>(byteMemories.Span.Slice(startIndex, readCount - startIndex), options);
        }

        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
        async ValueTask<TPackage> ReadSequenceAsync(SafeFileHandle handle, long sequenceLength, CsTomlSerializerOptions? options, bool configureAwait, CancellationToken cancellationToken)
        {
            using var bytes = new NativeMemoryArray<byte>(sequenceLength);
            var tomlMemories = bytes.AsMemoryList();
            await RandomAccess.ReadAsync(handle, tomlMemories, 0, cancellationToken).ConfigureAwait(configureAwait);

            // check BOM
            var memory = tomlMemories[0];
            var startIndex = Utf8Helper.ContainBOM(memory.Span) ? 3 : 0;

            var startSegment = new ByteSequenceSegment(memory[startIndex..]);
            startSegment.SetRunningIndex(0);
            startSegment.SetNext(null);

            var endSegment = startSegment;
            for (int i = 1; i < tomlMemories.Count; i++)
            {
                endSegment = endSegment.AddNext(tomlMemories[i]);
            }

            var sequence = new ReadOnlySequence<byte>(startSegment, 0, endSegment, endSegment.Length);
            return CsTomlSerializer.Deserialize<TPackage>(sequence, options);
        }
    }

    public static void Serialize<TPackage>(string tomlFilePath, TPackage? package, CsTomlSerializerOptions? options = null)
        where TPackage : CsTomlPackage
    {
        if (Path.GetExtension(tomlFilePath) != TomlExtension)
            throw new FormatException($"TOML files should use the extension .toml");

        var directory = new FileInfo(tomlFilePath).Directory;
        if (!directory!.Exists)
            directory.Create();

        using var bufferWriter = new ByteBufferSegmentWriter();
        var tempWriter = bufferWriter;
        CsTomlSerializer.Serialize(ref tempWriter, package);

        using var tomlFileHandle = File.OpenHandle(tomlFilePath, FileMode.Create, FileAccess.ReadWrite, options: FileOptions.Asynchronous);
        var fileWriter = new RandomAccessFileWriter(tomlFileHandle);
        bufferWriter.WriteTo(fileWriter.FileWriter);
    }

    public static async ValueTask SerializeAsync<TPackage>(string tomlFilePath, TPackage? package, CsTomlSerializerOptions? options = null, bool configureAwait = false, CancellationToken cancellationToken = default)
    where TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>
    {
        if (Path.GetExtension(tomlFilePath) != TomlExtension)
            throw new FormatException($"TOML files should use the extension .toml");

        var directory = new FileInfo(tomlFilePath).Directory;
        if (!directory!.Exists)
            directory.Create();

        cancellationToken.ThrowIfCancellationRequested();

        using var bufferWriter = new ByteBufferSegmentWriter();
        var tempWriter = bufferWriter;
        CsTomlSerializer.Serialize(ref tempWriter, package);

        using var tomlFileHandle = File.OpenHandle(tomlFilePath, FileMode.Create, FileAccess.ReadWrite, options: FileOptions.Asynchronous);
        var fileWriter = new RandomAccessFileWriter(tomlFileHandle);
        await bufferWriter.WriteToAsync(fileWriter.FileWriter, configureAwait, cancellationToken);
    }

}


