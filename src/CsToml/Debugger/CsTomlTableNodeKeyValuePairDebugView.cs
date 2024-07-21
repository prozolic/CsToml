using CsToml.Values;
using System.Diagnostics;

namespace CsToml.Debugger;

[DebuggerDisplay("Key='{Key}', Value='{Value}'")]
internal sealed class CsTomlTableNodeKeyValuePairDebugView(KeyValuePair<CsTomlDotKey, CsTomlTableNode> pair)
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly KeyValuePair<CsTomlDotKey, CsTomlTableNode> rawPair = pair;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public CsTomlDotKey Key => rawPair.Key;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public CsTomlTableNode Value => rawPair.Value;

    public bool IsGroupingProperty => Value.IsGroupingProperty!;

    public bool IsTableHeader => Value.IsTableHeader!;

    public bool IsTableHeaderDefinitionPosition => Value.IsTableHeaderDefinitionPosition!;

    public bool IsArrayOfTablesHeader => Value.IsArrayOfTablesHeader!;

    public bool IsArrayOfTablesHeaderDefinitionPosition => Value.IsArrayOfTablesHeaderDefinitionPosition!;
}