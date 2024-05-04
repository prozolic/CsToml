using CsToml.Values;
using CsToml.Error;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CsToml;

[DebuggerDisplay("{table}")]
public partial class CsTomlPackage : 
    ICsTomlPackageCreator<CsTomlPackage>
{
    private readonly CsTomlTable table;
    private readonly List<CsTomlException> exceptions;

    public CsTomlNode this[ReadOnlySpan<byte> key]
        => new(table[key]);

    public long LineNumber { get; internal set; }

    public ReadOnlyCollection<CsTomlException> Exceptions => exceptions.AsReadOnly();

    public static CsTomlPackage CreatePackage() => new();

    public CsTomlPackage()
    {
        table = new();
        exceptions = [];
        LineNumber = 0;
    }
}

[DebuggerDisplay("{Value}")]
public readonly struct CsTomlNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly CsTomlNode Empty = new (CsTomlTableNode.Empty);
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly CsTomlTableNode node;

    public readonly CsTomlValue Value => node.Value!;

    public CsTomlNode this[ReadOnlySpan<byte> key]
    {
        get
        {
            if (node?.TryGetChildNode(key, out var value) ?? false)
            {
                return new CsTomlNode(value!);
            }
            return Empty;
        }
    }

    public CsTomlNode this[int index]
    {
        get
        {
            if (Value is CsTomlArray arrayValue)
            {
                var t = arrayValue[index] as CsTomlTable;
                if (t == null) return Empty;

                return new CsTomlNode(t!.RootNode);
            }
            return Empty;
        }
    }

    internal CsTomlNode(CsTomlTableNode node)
    {
        this.node = node;
    }
}
