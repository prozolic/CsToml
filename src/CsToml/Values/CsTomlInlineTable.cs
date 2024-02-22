using CsToml.Utility;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlInlineTable : {RootNode}")]
internal class CsTomlInlineTable : CsTomlValue
{
    private readonly CsTomlTable inlineTable = new CsTomlTable();

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    internal CsTomlTableNode RootNode => inlineTable.RootNode;

    public CsTomlInlineTable() : base(CsTomlType.InlineTable) 
    {}

    public void AddKeyValue(CsTomlKey csTomlKey, CsTomlValue value, CsTomlTableNode? searchRootNode)
        => inlineTable.AddKeyValue(csTomlKey, value, searchRootNode, []);

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        writer.Write(CsTomlSyntax.Symbol.LEFTBRACES);
        var csTomlWriter = new CsTomlWriter(ref writer);
        csTomlWriter.WriteSpace();

        var keys = new List<CsTomlString>();
        ToTomlStringCore(ref csTomlWriter, RootNode, keys);

        csTomlWriter.WriteSpace();
        writer.Write(CsTomlSyntax.Symbol.RIGHTBRACES);
        return false;
    }

    private void ToTomlStringCore(ref CsTomlWriter writer, CsTomlTableNode parentNode, List<CsTomlString> keys)
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
                        writer.WriterKey(in keysSpan[i], true);
                    }
                }
                writer.WriteKeyValue(in key, childNode.Value!);
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

