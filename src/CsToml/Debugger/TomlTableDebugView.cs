using CsToml.Values;
using System.Diagnostics;

namespace CsToml.Debugger;

internal sealed class TomlTableDebugView(TomlTable csTomlTable)
{
    private readonly TomlTable table = csTomlTable;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public TomlTableKeyValuePairDebugView[] Children
    {
        get
        {
            var keyValues = new List<TomlTableKeyValuePairDebugView>();
            var index = 0;
            foreach (var keyValue in table.KeyValuePairs)
            {
                keyValues.Add(new TomlTableKeyValuePairDebugView(keyValue, table.GetKeyValueComments(index++).ToArray()));
            }
            return keyValues.ToArray();
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public TomlString[] Comments => table.CommentSpan.ToArray();

    public bool IsTableHeader => table.IsTableHeader;

    public bool IsTableHeaderDefinitionPosition => table.IsTableHeaderDefinitionPosition;

    public bool IsViaArrayOfTables => table.IsViaArrayOfTables;

    public int Count => table.Count;
}
