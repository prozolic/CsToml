using CsToml.Values;
using System.Diagnostics;

namespace CsToml.Debugger;

internal sealed class CsTomlTableNodeDebugView(CsTomlTableNode csTomlTableNode)
{
    private readonly CsTomlTableNode node = csTomlTableNode;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public CsTomlTableNodeKeyValuePairDebugView[] ChildrenNode => node.Nodes.Select(k => new CsTomlTableNodeKeyValuePairDebugView(k)).ToArray();

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public CsTomlValue Value => node.Value!;

    public CsTomlString[] Comments => node.Comments.ToArray();

    //public bool IsGroupingProperty => node.IsGroupingProperty!;

    //public bool IsTableHeader => node.IsTableHeader!;

    //public bool IsTableArrayHeader => node.IsTableArrayHeader!;
}