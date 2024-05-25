using CsToml.Values;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsToml;


[DebuggerDisplay("{Value}")]
public struct CsTomlPackageNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private CsTomlTableNode node;

    public readonly CsTomlValue Value => node.Value!;

    public CsTomlPackageNode this[ReadOnlySpan<byte> key]
    {
        get
        {
            if (node?.TryGetChildNode(key, out var value) ?? false)
            {
                node = value!;
                return this;
            }
            node = CsTomlTableNode.Empty;
            return this;
        }
    }

    public CsTomlPackageNode this[int index]
    {
        get
        {
            if (Value is CsTomlArray arrayValue)
            {
                var t = arrayValue[index] as CsTomlTable;
                if (t == null)
                {
                    node = CsTomlTableNode.Empty;
                    return this;
                }

                node = t!.RootNode;
                return this;
            }
            node = CsTomlTableNode.Empty;
            return this;
        }
    }

    internal CsTomlPackageNode(CsTomlTableNode node)
    {
        this.node = node;
    }
}


