using CsToml.Utility;

namespace CsToml;

public partial class CsTomlSerializer
{
    public static void ReadAndDeserialize(ref CsTomlPackage package, string? path)
    {
        if (Path.GetExtension(path) != ".toml") throw new FormatException($"TOML files should use the extension .toml");
        if (!File.Exists(path)) throw new FileNotFoundException(nameof(path));

        using var handle = File.OpenHandle(path, FileMode.Open, FileAccess.Read, options: FileOptions.Asynchronous);
        var length = RandomAccess.GetLength(handle);
        var bytes = new byte[length];
        RandomAccess.Read(handle, bytes.AsSpan(), 0);

        var startIndex = Utf8Helper.ContainBOM(bytes) ? 3 : 0;
        var byteSpan = bytes.AsSpan(startIndex);
        Deserialize(ref package, byteSpan);
    }

    public static async ValueTask<CsTomlPackage> ReadAndDeserializeAsync<TFactory>(string? path, CancellationToken cancellationToken = default)
        where TFactory : ICsTomlPackageFactory
    {
        if (!File.Exists(path)) throw new FileNotFoundException(nameof(path));

        using var handle = File.OpenHandle(path, FileMode.Open, FileAccess.Read, options: FileOptions.Asynchronous);
        var length = RandomAccess.GetLength(handle);
        var bytes = new byte[length];
        await RandomAccess.ReadAsync(handle, bytes.AsMemory(), 0, cancellationToken).ConfigureAwait(false);

        var startIndex = Utf8Helper.ContainBOM(bytes) ? 3 : 0;
        var package = TFactory.GetPackage();
        Deserialize(ref package, bytes.AsSpan(startIndex));
        return package;
    }

    public static void Deserialize(ref CsTomlPackage package, ReadOnlySpan<byte> tomlText)
    {
        var reader = new Utf8Reader(tomlText);
        package.Deserialize(ref reader);
    }
}


