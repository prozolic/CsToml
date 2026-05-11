
using CsToml.Extensions.Utility;
using Cysharp.Collections;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace CsToml.Extensions;

public partial class CsTomlFileSerializer
{
    private static readonly string TomlExtension = ".toml";

    public static T Deserialize<T>(string tomlFilePath, CsTomlSerializerOptions? options = null)
        => Deserialize<T>(tomlFilePath, options, TomlFileExtensionPolicy.Strict);

    public static T Deserialize<T>(string tomlFilePath, CsTomlSerializerOptions? options, TomlFileExtensionPolicy extensionPolicy)
    {
        ValidateExtension(tomlFilePath, extensionPolicy);

        using var handle = File.OpenHandle(tomlFilePath!, FileMode.Open, FileAccess.Read, options: FileOptions.SequentialScan);
        var length = RandomAccess.GetLength(handle);

        if (length == 0)
        {
            return CsTomlSerializer.Deserialize<T>(ReadOnlySpan<byte>.Empty, options);
        }
        else if (length < Array.MaxLength)
        {
            using var bytes = new NativeMemoryArray<byte>(length);
            var bytesSpan = bytes.AsSpan();
            var readCount = RandomAccess.Read(handle, bytesSpan, 0);

            // check BOM
            var tomlByteSpan = bytesSpan.Slice(0, readCount);
            return CsTomlSerializer.Deserialize<T>(tomlByteSpan, options);
        }
        else
        {
            using var bytes = new NativeMemoryArray<byte>(length);
            var tomlMemories = bytes.AsMemoryList();
            RandomAccess.Read(handle, tomlMemories, 0);

            // check BOM
            var memory = tomlMemories[0];
            var startSegment = new ByteSequenceSegment(memory);
            startSegment.SetRunningIndex(0);
            startSegment.SetNext(null);

            var endSegment = startSegment;
            for (int i = 1; i < tomlMemories.Count; i++)
            {
                endSegment = endSegment.AddNext(tomlMemories[i]);
            }

            var sequence = new ReadOnlySequence<byte>(startSegment, 0, endSegment, endSegment.Length);
            return CsTomlSerializer.Deserialize<T>(sequence, options);
        }
    }

    public static ValueTask<T> DeserializeAsync<T>(string tomlFilePath, CsTomlSerializerOptions? options = null, bool configureAwait = false, CancellationToken cancellationToken = default)
        => DeserializeAsyncCore<T>(tomlFilePath, options, TomlFileExtensionPolicy.Strict, configureAwait, cancellationToken);

    public static ValueTask<T> DeserializeAsync<T>(string tomlFilePath, CsTomlSerializerOptions? options, TomlFileExtensionPolicy extensionPolicy, bool configureAwait = false, CancellationToken cancellationToken = default)
        => DeserializeAsyncCore<T>(tomlFilePath, options, extensionPolicy, configureAwait, cancellationToken);

    private static async ValueTask<T> DeserializeAsyncCore<T>(string tomlFilePath, CsTomlSerializerOptions? options, TomlFileExtensionPolicy extensionPolicy, bool configureAwait, CancellationToken cancellationToken)
    {
        ValidateExtension(tomlFilePath, extensionPolicy);

        cancellationToken.ThrowIfCancellationRequested();
        using var handle = File.OpenHandle(tomlFilePath!, FileMode.Open, FileAccess.Read, options: FileOptions.SequentialScan | FileOptions.Asynchronous);
        var length = RandomAccess.GetLength(handle);

        if (length == 0)
        {
            return CsTomlSerializer.Deserialize<T>(ReadOnlySpan<byte>.Empty, options);
        }
        else if (length <= Array.MaxLength)
        {
            using var bytes = new NativeMemoryArray<byte>(length);
            var byteMemories = bytes.AsMemory();
            var readCount = await RandomAccess.ReadAsync(handle, byteMemories, 0, cancellationToken).ConfigureAwait(configureAwait);

            return CsTomlSerializer.Deserialize<T>(byteMemories.Span.Slice(0, readCount), options);
        }
        else
        {
            using var bytes = new NativeMemoryArray<byte>(length);
            var tomlMemories = bytes.AsMemoryList();
            await RandomAccess.ReadAsync(handle, tomlMemories, 0, cancellationToken).ConfigureAwait(configureAwait);

            // check BOM
            var memory = tomlMemories[0];
            var startSegment = new ByteSequenceSegment(memory);
            startSegment.SetRunningIndex(0);
            startSegment.SetNext(null);

            var endSegment = startSegment;
            for (int i = 1; i < tomlMemories.Count; i++)
            {
                endSegment = endSegment.AddNext(tomlMemories[i]);
            }

            var sequence = new ReadOnlySequence<byte>(startSegment, 0, endSegment, endSegment.Length);
            return CsTomlSerializer.Deserialize<T>(sequence, options);
        }
    }

    public static void Serialize<T>(string tomlFilePath, T value, CsTomlSerializerOptions? options = null)
        => Serialize<T>(tomlFilePath, value, options, TomlFileExtensionPolicy.Strict);

    public static void Serialize<T>(string tomlFilePath, T value, CsTomlSerializerOptions? options, TomlFileExtensionPolicy extensionPolicy)
    {
        ValidateExtension(tomlFilePath, extensionPolicy);

        var directory = new FileInfo(tomlFilePath).Directory;
        if (!directory!.Exists)
            directory.Create();

        using var bufferWriter = new ByteBufferSegmentWriter();
        var tempWriter = bufferWriter;
        CsTomlSerializer.Serialize<ByteBufferSegmentWriter, T>(ref tempWriter, value, options);

        using var tomlFileHandle = File.OpenHandle(tomlFilePath, FileMode.Create, FileAccess.ReadWrite, options: FileOptions.Asynchronous);
        var fileWriter = new RandomAccessFileWriter(tomlFileHandle);
        bufferWriter.WriteTo(fileWriter.ByteWriter);
    }

    public static ValueTask SerializeAsync<T>(string tomlFilePath, T value, CsTomlSerializerOptions? options = null, bool configureAwait = false, CancellationToken cancellationToken = default)
        => SerializeAsyncCore<T>(tomlFilePath, value, options, TomlFileExtensionPolicy.Strict, configureAwait, cancellationToken);

    public static ValueTask SerializeAsync<T>(string tomlFilePath, T value, CsTomlSerializerOptions? options, TomlFileExtensionPolicy extensionPolicy, bool configureAwait = false, CancellationToken cancellationToken = default)
        => SerializeAsyncCore<T>(tomlFilePath, value, options, extensionPolicy, configureAwait, cancellationToken);

    private static async ValueTask SerializeAsyncCore<T>(string tomlFilePath, T value, CsTomlSerializerOptions? options, TomlFileExtensionPolicy extensionPolicy, bool configureAwait, CancellationToken cancellationToken)
    {
        ValidateExtension(tomlFilePath, extensionPolicy);

        var directory = new FileInfo(tomlFilePath).Directory;
        if (!directory!.Exists)
            directory.Create();

        cancellationToken.ThrowIfCancellationRequested();

        using var bufferWriter = new ByteBufferSegmentWriter();
        var tempWriter = bufferWriter;
        CsTomlSerializer.Serialize<ByteBufferSegmentWriter, T>(ref tempWriter, value, options);

        using var tomlFileHandle = File.OpenHandle(tomlFilePath, FileMode.Create, FileAccess.ReadWrite, options: FileOptions.Asynchronous);
        var fileWriter = new RandomAccessFileWriter(tomlFileHandle);
        await bufferWriter.WriteToAsync(fileWriter.ByteWriter, configureAwait, cancellationToken);
    }

    private static void ValidateExtension(string tomlFilePath, TomlFileExtensionPolicy extensionPolicy)
    {
        switch (extensionPolicy)
        {
            case TomlFileExtensionPolicy.Strict:
                if (!string.Equals(Path.GetExtension(tomlFilePath), TomlExtension, StringComparison.Ordinal))
                {
                    ThrowExtensionFormatException();
                }
                break;
            case TomlFileExtensionPolicy.Relaxed:
                // No validation needed for relaxed policy
                break;
            default:
                ThrowExtensionOutOfRangeException(extensionPolicy);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowExtensionFormatException() 
    {
        throw new FormatException("TOML files should use the extension .toml");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowExtensionOutOfRangeException(TomlFileExtensionPolicy extensionPolicy)
    {
        throw new ArgumentOutOfRangeException(nameof(extensionPolicy));
    }
}
