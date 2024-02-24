using CsToml.Utility;
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
        var bytes = new byte[length];
        RandomAccess.Read(handle, bytes.AsSpan(), 0);

        var startIndex = Utf8Helper.ContainBOM(bytes) ? 3 : 0;
        var tomlText = bytes.AsSpan(startIndex);

        var package = TPackage.CreatePackage();
        DeserializeCore(tomlText, package, options);
        return package;
    }

    public static async ValueTask<TPackage?> ReadAndDeserializeAsync<TPackage>(string? path, CsTomlSerializerOptions? options = null, CancellationToken cancellationToken = default)
        where TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>
    {
        ExistsTomlFile(path);

        using var handle = File.OpenHandle(path!, FileMode.Open, FileAccess.Read, options: FileOptions.Asynchronous);
        var length = RandomAccess.GetLength(handle);
        var bytes = new byte[length];
        await RandomAccess.ReadAsync(handle, bytes.AsMemory(), 0, cancellationToken).ConfigureAwait(false);

        var startIndex = Utf8Helper.ContainBOM(bytes) ? 3 : 0;
        var package = TPackage.CreatePackage();
        DeserializeCore(bytes.AsSpan(startIndex), package, options);
        return package;
    }

    public static TPackage? Deserialize<TPackage>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options = null)
        where TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>
    {
        var package = TPackage.CreatePackage();
        DeserializeCore(tomlText, package, options);
        return package;
    }

    private static void DeserializeCore<TPackage>(ReadOnlySpan<byte> tomlText, TPackage? package, CsTomlSerializerOptions? options)
        where TPackage : CsTomlPackage
    {
        var reader = new Utf8Reader(tomlText);
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


