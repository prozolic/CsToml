using CsToml.Values;
using System.Diagnostics;

namespace CsToml.Debugger;

[DebuggerDisplay("Key='{Key}', Value='{Value}'")]
internal sealed class TomlTableKeyValuePairDebugView(KeyValuePair<TomlDottedKey, TomlValue> pair, TomlString[] comments)
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly KeyValuePair<TomlDottedKey, TomlValue> rawPair = pair;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public TomlDottedKey Key => rawPair.Key;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public TomlValue Value => rawPair.Value;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public TomlString[] Comments => comments;

    public bool IsTable => Value is TomlTable;

    public bool IsArrayOfTables => Value is TomlTableArray;
}
