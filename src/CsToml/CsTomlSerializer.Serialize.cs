using CsToml.Utility;
using System.Buffers;

namespace CsToml;

public partial class CsTomlSerializer
{
    public static byte[] Serialize(ref CsTomlPackage package)
    {
        var writer = new ArrayBufferWriter<byte>();
        Serialize(writer, ref package);
        return writer.WrittenSpan.ToArray();
    }

    public static void Serialize(IBufferWriter<byte> bufferWriter, ref CsTomlPackage package)
    {
        var utf8Writer = new Utf8Writer(bufferWriter);
        var csTomlWriter = new CsTomlWriter(ref utf8Writer);

        var rootNode = package.Node;
        foreach(var (key, childNode) in rootNode.Nodes)
        {
            if (!childNode.IsGroupingProperty)
            {
                csTomlWriter.WriteKeyValue(key, childNode.Value!);
            }

            // TODO: currently being implemented
        }
    }



}