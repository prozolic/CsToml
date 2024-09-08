using CsToml.Values;
using System.Diagnostics;

namespace CsToml.Debugger;

[DebuggerDisplay("Key='{Key}', Value='{Value}'")]
internal sealed class TomlTableNodeKeyValuePairDebugView(KeyValuePair<TomlDottedKey, TomlTableNode> pair)
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly KeyValuePair<TomlDottedKey, TomlTableNode> rawPair = pair;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public TomlDottedKey Key => rawPair.Key;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public TomlTableNode Value => rawPair.Value;

    public bool IsGroupingProperty => Value.IsGroupingProperty!;

    public bool IsTableHeader => Value.IsTableHeader!;

    public bool IsTableHeaderDefinitionPosition => Value.IsTableHeaderDefinitionPosition!;

    public bool IsArrayOfTablesHeader => Value.IsArrayOfTablesHeader!;

    public bool IsArrayOfTablesHeaderDefinitionPosition => Value.IsArrayOfTablesHeaderDefinitionPosition!;
}