using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlInlineTable : {RootNode}")]
internal class CsTomlInlineTable : CsTomlValue
{
    private readonly CsTomlTable inlineTable = new CsTomlTable();

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    internal CsTomlTableNode RootNode => inlineTable.RootNode;

    public CsTomlInlineTable() : base() 
    {}

    public void AddKeyValue(ArrayPoolList<CsTomlDotKey> csTomlKey, CsTomlValue value, CsTomlTableNode? searchRootNode)
        => inlineTable.AddKeyValue(csTomlKey, value, searchRootNode, []);

    internal override bool ToTomlString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer)
    {
        writer.Write(CsTomlSyntax.Symbol.LEFTBRACES);
        var csTomlWriter = new CsTomlWriter<TBufferWriter>(ref writer);
        csTomlWriter.WriteSpace();

        var keys = new List<CsTomlDotKey>();
        ToTomlStringCore(ref csTomlWriter, RootNode, keys);

        csTomlWriter.WriteSpace();
        writer.Write(CsTomlSyntax.Symbol.RIGHTBRACES);
        return false;
    }

    private void ToTomlStringCore<TBufferWriter>(ref CsTomlWriter<TBufferWriter> writer, CsTomlTableNode parentNode, List<CsTomlDotKey> keys)
        where TBufferWriter : IBufferWriter<byte>
    {
        var count = 0;

        foreach (var (key, childNode) in parentNode.KeyValuePairs)
        {
            if (childNode.IsGroupingProperty)
            {
                keys.Add(key);
                ToTomlStringCore(ref writer, childNode, keys);
                continue;
            }
            else
            {
                var keysSpan = CollectionsMarshal.AsSpan(keys);
                if (keysSpan.Length > 0)
                {
                    for (var i = 0; i < keysSpan.Length; i++)
                    {
                        writer.WriterKey(keysSpan[i], true);
                    }
                }
                writer.WriteKeyValue(key, childNode.Value!);
                count++;

                if (count != parentNode.NodeCount)
                {
                    writer.WriteComma();
                    writer.WriteSpace();
                }
            }
        }

        keys.Clear(); // clear subkey
    }

}

