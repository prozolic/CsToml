using CsToml.Utility;
using System.Buffers;

namespace CsToml;

public partial class CsTomlSerializer
{
    private static readonly string TomlExtension = ".toml";

    public static TPackage? ReadAndDeserialize<TPackage>(string? path, CsTomlSerializerOptions? options = null)
        where TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>
    {
        if (Path.GetExtension(path) != TomlExtension)
            throw new FormatException($"TOML files should use the extension .toml");

        using var handle = File.OpenHandle(path!, FileMode.Open, FileAccess.Read, options: FileOptions.SequentialScan);
        var length = RandomAccess.GetLength(handle);

        if (length == 0)
        {
            return TPackage.CreatePackage();
        }
        else if (length <= Array.MaxLength)
        {
            using var bytes = new ArrayPoolBufferWriter<byte>((int)length);
            var bytesSpan = bytes.GetFullSpan(); 
            var readCount = RandomAccess.Read(handle, bytesSpan, 0);

            var startIndex = Utf8Helper.ContainBOM(bytesSpan) ? 3 : 0;
            var tomlText = bytesSpan.Slice(startIndex, readCount - startIndex);
            return Deserialize<TPackage>(tomlText, options);
        }
        else
        {
            using var bytes = new NativeByteMemoryArray(length);
            var tomlMemories = bytes.AsMemoryList(0);
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
            return Deserialize<TPackage>(sequence, options);
        }
    }

    public static async ValueTask<TPackage?> ReadAndDeserializeAsync<TPackage>(string? path, CsTomlSerializerOptions? options = null, bool configureAwait = false, CancellationToken cancellationToken = default)
        where TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>
    {
        if (Path.GetExtension(path) != TomlExtension)
            throw new FormatException($"TOML files should use the extension .toml");

        cancellationToken.ThrowIfCancellationRequested();
        using var handle = File.OpenHandle(path!, FileMode.Open, FileAccess.Read, options: FileOptions.SequentialScan | FileOptions.Asynchronous);
        var length = RandomAccess.GetLength(handle);

        if (length == 0)
        {
            return TPackage.CreatePackage();
        }
        else if (length <= Array.MaxLength)
        {
            using var bytes = new ArrayPoolBufferWriter<byte>((int)length);
            var bytesMemory = bytes.GetFullMemory();
            var readCount = await RandomAccess.ReadAsync(handle, bytesMemory, 0, cancellationToken).ConfigureAwait(configureAwait);

            var startIndex = Utf8Helper.ContainBOM(bytesMemory.Span) ? 3 : 0;
            return Deserialize<TPackage>(bytesMemory.Span.Slice(startIndex, readCount - startIndex), options);
        }
        else
        {
            using var bytes = new NativeByteMemoryArray(length);
            var tomlMemories = bytes.AsMemoryList(0);
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
            return Deserialize<TPackage>(sequence, options);
        }
    }

    public static TPackage? Deserialize<TPackage>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options = null)
        where TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>
    {
        var package = TPackage.CreatePackage();
        DeserializeCore(tomlText, package, options);
        return package;
    }

    public static TPackage? Deserialize<TPackage>(in ReadOnlySequence<byte> tomlTextSequence, CsTomlSerializerOptions? options = null)
        where TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>
    {
        var package = TPackage.CreatePackage();
        DeserializeCore(tomlTextSequence, package, options);
        return package;
    }

    private static void DeserializeCore<TPackage>(ReadOnlySpan<byte> tomlText, TPackage? package, CsTomlSerializerOptions? options)
        where TPackage : CsTomlPackage
    {
        var reader = new Utf8SequenceReader(tomlText);
        package?.Deserialize(ref reader, options);
    }

    private static void DeserializeCore<TPackage>(in ReadOnlySequence<byte> tomlText, TPackage? package, CsTomlSerializerOptions? options)
    where TPackage : CsTomlPackage
    {
        var reader = new Utf8SequenceReader(tomlText);
        package?.Deserialize(ref reader, options);
    }

}


