using CsToml.Values;
using System.Diagnostics;

namespace CsToml.Debugger;

internal sealed class CsTomlTableNodeDebugView(CsTomlTableNode csTomlTableNode)
{
    private readonly CsTomlTableNode node = csTomlTableNode;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public CsTomlTableNodeKeyValuePairDebugView[] ChildrenNode
    {
        get
        {
            var keyValues = new List<CsTomlTableNodeKeyValuePairDebugView>();
            foreach (var keyValue in node.KeyValuePairs)
            {
                keyValues.Add(new CsTomlTableNodeKeyValuePairDebugView(keyValue));
            }
            return keyValues.ToArray();
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public CsTomlValue Value => node.Value!;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public CsTomlString[] Comments => node.Comments?.ToArray() ?? [];

    public bool IsGroupingProperty => node.IsGroupingProperty;

    public bool IsTableHeader => node.IsTableHeader;

    public bool IsTableHeaderDefinitionPosition => node.IsTableHeaderDefinitionPosition;

    public bool IsArrayOfTablesHeader => node.IsArrayOfTablesHeader;

    public bool IsArrayOfTablesHeaderDefinitionPosition => node.IsArrayOfTablesHeaderDefinitionPosition;

    public int NodeCount => node.NodeCount;


}