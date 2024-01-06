using CsToml.Utility;
using CsToml.Values;
using System.Diagnostics;

namespace CsToml.Debugger;

[DebuggerDisplay("Key={Key}, Value={Value}")]
internal sealed class CsTomlTableNodeKeyValuePairDebugView(KeyValuePair<Utf8FixString, CsTomlTableNode> pair)
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly KeyValuePair<Utf8FixString, CsTomlTableNode> rawPair = pair;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public Utf8FixString Key => rawPair.Key;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public CsTomlTableNode Value => rawPair.Value;
}