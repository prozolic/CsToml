using CsToml.Values;
using System.Diagnostics;

namespace CsToml.Debugger;

internal sealed class CsTomlTableNodeDebugView(CsTomlTableNode csTomlTableNode)
{
    private readonly CsTomlTableNode node = csTomlTableNode;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public CsTomlTableNodeKeyValuePairDebugView[] ChildrenNode => node.Nodes.Select(k => new CsTomlTableNodeKeyValuePairDebugView(k)).ToArray();

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public CsTomlValue Value => node.Value!;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public CsTomlString[] Comments => node.Comments.ToArray();

}