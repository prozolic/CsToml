using CsToml.Error;
using CsToml.Utility;

using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlInlineTable : {RootNode}")]
internal class CsTomlInlineTable : CsTomlValue
{
    private readonly CsTomlTable inlineTable;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public CsTomlTableNode RootNode => inlineTable.RootNode;

    public CsTomlInlineTable() : base(CsTomlType.InlineTable) 
    {
        inlineTable = new CsTomlTable();
    }

    public bool TryAddValue(CsTomlKey csTomlKey, CsTomlValue value, CsTomlTableNode? searchRootNode = null)
    {
        return inlineTable.TryAddValue(csTomlKey, value, searchRootNode);
    }

    public bool TryAddTableHeader(CsTomlKey csTomlKey, out CsTomlTableNode? newNode)
    {
        return inlineTable.TryAddTableHeader(csTomlKey, out newNode);
    }

    public bool TryGetValue(ReadOnlySpan<byte> key, out CsTomlValue? value, CsTomlTableNode? searchRootNode = null)
    {
        return inlineTable.TryGetValue(key, out value, searchRootNode);
    }
}

