using CsToml.Utility;
using System.Buffers;

namespace CsToml;

public partial class CsTomlSerializer
{
    public static TPackage Deserialize<TPackage>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options = null)
        where TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>
    {
        var package = TPackage.CreatePackage() ?? throw new NullReferenceException($"{typeof(TPackage)} was not created.");
        DeserializeCore(tomlText, package, options);
        return package;
    }

    public static TPackage Deserialize<TPackage>(in ReadOnlySequence<byte> tomlTextSequence, CsTomlSerializerOptions? options = null)
        where TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>
    {
        var package = TPackage.CreatePackage() ?? throw new NullReferenceException($"{typeof(TPackage)} was not created.");
        DeserializeCore(tomlTextSequence, package, options);
        return package;
    }

    private static void DeserializeCore<TPackage>(ReadOnlySpan<byte> tomlText, TPackage package, CsTomlSerializerOptions? options)
        where TPackage : CsTomlPackage
    {
        var reader = new Utf8SequenceReader(tomlText);
        package.Deserialize(ref reader, options);
    }

    private static void DeserializeCore<TPackage>(in ReadOnlySequence<byte> tomlText, TPackage package, CsTomlSerializerOptions? options)
    where TPackage : CsTomlPackage
    {
        var reader = new Utf8SequenceReader(tomlText);
        package.Deserialize(ref reader, options);
    }

}


