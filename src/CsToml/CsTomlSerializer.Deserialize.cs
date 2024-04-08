using CsToml.Utility;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace CsToml;

public partial class CsTomlSerializer
{
    public static TPackage? ReadAndDeserialize<TPackage>(string? path, CsTomlSerializerOptions? options = null)
        where TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>
    {
        ExistsTomlFile(path);

        using var handle = File.OpenHandle(path!, FileMode.Open, FileAccess.Read, options: FileOptions.Asynchronous);
        var length = RandomAccess.GetLength(handle);

        if (length == 0)
        {
            return TPackage.CreatePackage();
        }
        else if (length <= Array.MaxLength)
        {
            var bytes = new byte[length];
            RandomAccess.Read(handle, bytes.AsSpan(), 0);
            var startIndex = Utf8Helper.ContainBOM(bytes) ? 3 : 0;
            var tomlText = bytes.AsSpan(startIndex);

            var package = TPackage.CreatePackage();
            DeserializeCore(tomlText, package, options);
            return package;
        }
        else
        {
            using var bytes = new NativeByteMemoryArray(length);
            var tomlMemories = bytes.AsMemoryList(0);
            RandomAccess.Read(handle, tomlMemories, 0);

            // check BOM
            var memory = tomlMemories[0];
            var startIndex = Utf8Helper.ContainBOM(memory.Span[..3]) ? 3 : 0;

            var startSegment = new ByteSequenceSegment(memory[startIndex..]);
            startSegment.SetRunningIndex(0);
            startSegment.SetNext(null);

            var endSegment = startSegment;
            for (int i = 1; i < tomlMemories.Count; i++)
            {
                endSegment = endSegment.AddNext(tomlMemories[i]);
            }

            var package = TPackage.CreatePackage();
            var sequence = new ReadOnlySequence<byte>(startSegment, 0, endSegment, endSegment.Length);
            DeserializeCore(sequence, package, options);
            return package;
        }
    }

    public static async ValueTask<TPackage?> ReadAndDeserializeAsync<TPackage>(string? path, CsTomlSerializerOptions? options = null, CancellationToken cancellationToken = default, bool configureAwait = false)
        where TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>
    {
        cancellationToken.ThrowIfCancellationRequested();
        ExistsTomlFile(path);

        using var handle = File.OpenHandle(path!, FileMode.Open, FileAccess.Read, options: FileOptions.Asynchronous);
        var length = RandomAccess.GetLength(handle);

        if (length == 0)
        {
            return TPackage.CreatePackage();
        }
        else if (length <= Array.MaxLength)
        {
            var bytes = new byte[length];
            await RandomAccess.ReadAsync(handle, bytes.AsMemory(), 0, cancellationToken).ConfigureAwait(configureAwait);

            var startIndex = Utf8Helper.ContainBOM(bytes) ? 3 : 0;
            var package = TPackage.CreatePackage();
            DeserializeCore(bytes.AsSpan(startIndex), package, options);
            return package;
        }
        else
        {
            using var bytes = new NativeByteMemoryArray(length);
            var tomlMemories = bytes.AsMemoryList(0);
            await RandomAccess.ReadAsync(handle, tomlMemories, 0, cancellationToken).ConfigureAwait(configureAwait);

            // check BOM
            var memory = tomlMemories[0];
            var startIndex = Utf8Helper.ContainBOM(memory.Span[..3]) ? 3 : 0;

            var startSegment = new ByteSequenceSegment(memory[startIndex..]);
            startSegment.SetRunningIndex(0);
            startSegment.SetNext(null);

            var endSegment = startSegment;
            for (int i = 1; i < tomlMemories.Count; i++)
            {
                endSegment = endSegment.AddNext(tomlMemories[i]);
            }

            var package = TPackage.CreatePackage();
            var sequence = new ReadOnlySequence<byte>(startSegment, 0, endSegment, endSegment.Length);
            DeserializeCore(sequence, package, options);
            return package;
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

    private static readonly string TomlExtension = ".toml";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ExistsTomlFile(string? path)
    {
        if (Path.GetExtension(path) != TomlExtension) 
            throw new FormatException($"TOML files should use the extension .toml");
        if (!File.Exists(path)) 
            throw new FileNotFoundException(nameof(path));
    }
}


