using CsToml.Values;
using System.Diagnostics;

namespace CsToml.Debugger;

internal sealed class TomlTableNodeDebugView(TomlTableNode csTomlTableNode)
{
    private readonly TomlTableNode node = csTomlTableNode;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public TomlTableNodeKeyValuePairDebugView[] ChildrenNode
    {
        get
        {
            var keyValues = new List<TomlTableNodeKeyValuePairDebugView>();
            foreach (var keyValue in node.KeyValuePairs)
            {
                keyValues.Add(new TomlTableNodeKeyValuePairDebugView(keyValue));
            }
            return keyValues.ToArray();
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public TomlValue Value => node.Value!;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public TomlString[] Comments => node.Comments?.ToArray() ?? [];

    public bool IsGroupingProperty => node.IsGroupingProperty;

    public bool IsTableHeader => node.IsTableHeader;

    public bool IsTableHeaderDefinitionPosition => node.IsTableHeaderDefinitionPosition;

    public bool IsArrayOfTablesHeader => node.IsArrayOfTablesHeader;

    public bool IsArrayOfTablesHeaderDefinitionPosition => node.IsArrayOfTablesHeaderDefinitionPosition;

    public int NodeCount => node.NodeCount;


}