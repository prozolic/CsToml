using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Runtime.InteropServices;

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

        SerializeValue(ref csTomlWriter, package.Node);
    }

    private static void SerializeValue(ref CsTomlWriter writer, CsTomlTableNode node)
    {
        var keys = new List<Utf8FixString>();
        SerializeValueCore(ref writer, node, keys);
    }

    private static void SerializeValueCore(ref CsTomlWriter writer, CsTomlTableNode node, List<Utf8FixString> keys)
    {
        foreach (var (key, childNode) in node.Nodes)
        {
            if (childNode.IsGroupingProperty)
            {
                if (node.IsTableHeader && !childNode.IsTableHeader && keys.Count > 0)
                {
                    var keysSpan = CollectionsMarshal.AsSpan(keys);
                    writer.WriteTableHeader(keysSpan);
                    keys.Clear();
                }
                keys.Add(key);
                SerializeValueCore(ref writer, childNode, keys);
                continue;
            }
            else
            {
                if (node.IsTableHeader && keys.Count > 0)
                {
                    var keysSpan = CollectionsMarshal.AsSpan(keys);
                    writer.WriteTableHeader(keysSpan);
                    keys.Clear();
                    writer.WriteKeyValue(in key, childNode.Value!);
                }
                else
                {
                    var keysSpan = CollectionsMarshal.AsSpan(keys);
                    if (keysSpan.Length > 0)
                    {
                        for (var i = 0; i < keysSpan.Length; i++)
                        {
                            writer.WriterKey(in keysSpan[i], true);
                        }
                    }
                    writer.WriteKeyValue(in key, childNode.Value!);
                }
            }
        }

        keys.Clear(); // clear subkey
    }


}