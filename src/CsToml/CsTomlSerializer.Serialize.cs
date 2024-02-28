using CsToml.Utility;
using System.Buffers;

namespace CsToml;

public partial class CsTomlSerializer
{
    public static byte[] Serialize<TPackage>(TPackage? package)
        where TPackage : CsTomlPackage
    {
        var writer = new ArrayBufferWriter<byte>();
        Serialize(writer, package);
        return writer.WrittenSpan.ToArray();
    }

    public static void Serialize<TPackage>(in IBufferWriter<byte> bufferWriter, TPackage? package)
        where TPackage : CsTomlPackage
    {
        var utf8Writer = new Utf8Writer(bufferWriter);
        package.Serialize(ref utf8Writer);
    }

}