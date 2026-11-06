using CsToml.Debugger;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Values;

[DebuggerTypeProxy(typeof(TomlTableArrayDebugView))]
internal sealed class TomlTableArray : TomlArray
{
    private List<TomlString>? comments;

    internal int CommentCount => comments?.Count ?? 0;

    internal ReadOnlySpan<TomlString> CommentSpan => comments != null ? CollectionsMarshal.AsSpan(comments) : ReadOnlySpan<TomlString>.Empty;

    // An array of tables always holds at least one table.
    internal TomlTable LastTable => (TomlTable)LastValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(TomlTable table)
    {
        AddCore(table);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Span<TomlString> AddComments(int commentCount)
    {
        return TomlTable.AppendComments(ref comments, commentCount);
    }
}
