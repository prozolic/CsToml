using CsToml.Utility;
using CsToml.Values;
using System.Diagnostics;

namespace CsToml.Debugger;

[DebuggerDisplay("Key={Key}, Value={Value}")]
internal sealed class CsTomlTableNodeKeyValuePairDebugView(KeyValuePair<CsTomlString, CsTomlTableNode> pair)
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly KeyValuePair<CsTomlString, CsTomlTableNode> rawPair = pair;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public CsTomlString Key => rawPair.Key;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public CsTomlTableNode Value => rawPair.Value;
}