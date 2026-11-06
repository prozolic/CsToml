using CsToml.Values;
using System.Diagnostics;

namespace CsToml.Debugger;

internal sealed class TomlTableArrayDebugView(TomlTableArray csTomlTableArray)
{
    private readonly TomlTableArray array = csTomlTableArray;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public TomlValue[] Items => array.AsSpan().ToArray();

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public TomlString[] Comments => array.CommentSpan.ToArray();
}
