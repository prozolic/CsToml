using CsToml.Error;
using CsToml.Extension;
using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CsToml.Values;

[DebuggerDisplay("Count = {RootNode.NodeCount}")]
internal sealed partial class TomlTable : TomlValue
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly TomlTableNode node = TomlTableNode.CreateGroupingPropertyNode();

    public override bool HasValue => true;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    internal TomlTableNode RootNode => node;

    internal TomlTableNode this[ReadOnlySpan<byte> key]
    {
        get
        {
            if (RootNode.TryGetChildNode(key, out var value))
            {
                return value!;
            }
            return TomlTableNode.Empty;
        }
    }

    internal TomlTable() : base() { }

    internal TomlTableNode AddKeyValue(ReadOnlySpan<TomlDotKey> dotKeys, TomlValue value, TomlTableNode? searchRootNode, IReadOnlyCollection<TomlString>? comments)
    {
        var currentNode = searchRootNode ?? RootNode;
        var lastKey = dotKeys[^1];

        for (var i = 0; i < dotKeys.Length - 1; i++)
        {
            var sectionKey = dotKeys[i];
            if (currentNode!.TryGetOrAddChildNode(sectionKey, out var childNode) == NodeStatus.NewAdd)
            {
                currentNode = childNode;
                continue;
            }
            if (childNode.IsTableHeaderDefinitionPosition)
            {
                ExceptionHelper.ThrowTheKeyIsDefinedAsTable();
            }
            if (childNode.IsArrayOfTablesHeaderDefinitionPosition)
            {
                ExceptionHelper.ThrowTheKeyIsDefinedAsArrayOfTables();
            }
            if (childNode.IsGroupingProperty)
            {
                currentNode = childNode;
                continue;
            }

            ExceptionHelper.ThrowNotTurnIntoTable(dotKeys.GetJoinName());
        }

        return currentNode.AddKeyValue(lastKey, value, comments);
    }

    internal void AddTableHeader(ReadOnlySpan<TomlDotKey> dotKeys, IReadOnlyCollection<TomlString>? comments, out TomlTableNode? newNode)
    {
        var node = RootNode;
        var addedNewNode = false;

        for (var i = 0; i < dotKeys.Length; i++)
        {
            var sectionKey = dotKeys[i];
            if (node!.TryGetOrAddChildNode(sectionKey, out var childNode) == NodeStatus.NewAdd)
            {
                addedNewNode = true;
                node = childNode!;
                node.IsTableHeader = true;
                continue;
            }
            
            if (childNode!.IsArrayOfTablesHeaderDefinitionPosition)
            {
                var tableHeaderArrayValue = (childNode!.Value as TomlArray)?.LastValue;
                node = (tableHeaderArrayValue as TomlTable)?.RootNode;
                continue;
            }

            if (childNode!.IsGroupingProperty)
            {
                node = childNode;
                continue;
            }

            newNode = null;
            ExceptionHelper.ThrowIncorrectTomlFormat();
        }

        if (!addedNewNode)
        {
            if (node!.IsTableHeaderDefinitionPosition)
            {
                ExceptionHelper.ThrowTableHeaderIsDefined(dotKeys.GetJoinName());
            }
            if (node!.IsArrayOfTablesHeaderDefinitionPosition)
            {
                ExceptionHelper.ThrowTableHeaderIsDefinedAsArrayOfTables(dotKeys.GetJoinName());
            }
            if (!node!.IsTableHeader)
            {
                ExceptionHelper.ThrowTableHeaderIsDefined(dotKeys.GetJoinName());
            }
        }

        if (comments?.Count > 0)
        {
            node!.AddComment(comments);
        }
        node!.IsTableHeaderDefinitionPosition = true;
        newNode = node;
    }

    internal void AddArrayOfTablesHeader(ReadOnlySpan<TomlDotKey> dotKeys, IReadOnlyCollection<TomlString>? comments, out TomlTableNode? newNode)
    {
        var currentNode = RootNode;
        var addedNewNode = false;

        for (var i = 0; i < dotKeys.Length; i++)
        {
            var sectionKey = dotKeys[i];
            if (currentNode!.TryGetOrAddChildNode(sectionKey, out var childNode) == NodeStatus.NewAdd)
            {
                addedNewNode = true;
                currentNode = childNode!;
                currentNode.IsArrayOfTablesHeader = true;
                currentNode.IsTableHeader = i < dotKeys.Length - 1;
                continue;
            }

            if (childNode!.IsArrayOfTablesHeaderDefinitionPosition)
            {
                if (i == dotKeys.Length - 1)
                {
                    currentNode = childNode;
                    continue;
                }
                else
                {
                    var tableHeaderArrayValue = (childNode!.Value as TomlArray)?.LastValue;
                    currentNode = (tableHeaderArrayValue as TomlTable)?.RootNode;
                    continue;
                }
            }
            if (childNode!.IsGroupingProperty)
            {
                currentNode = childNode;
                continue;
            }

            newNode = null;
            ExceptionHelper.ThrowIncorrectTomlFormat();
        }

        if (currentNode!.IsTableHeader)
        {
            ExceptionHelper.ThrowTheArrayOfTablesIsDefinedAsTable(dotKeys.GetJoinName());
        }

        if (addedNewNode)
        {
            currentNode.Value = new TomlArray();
            currentNode.IsArrayOfTablesHeader = true;
            currentNode.IsArrayOfTablesHeaderDefinitionPosition = true;
        }
        else
        {
            if (!currentNode!.IsArrayOfTablesHeaderDefinitionPosition)
            {
                ExceptionHelper.ThrowTheArrayOfTablesIsDefinedAsTable(dotKeys.GetJoinName());
            }
        }
        var table = new TomlTable();
        (currentNode.Value as TomlArray)?.Add(table);
        if (comments?.Count > 0)
        {
            currentNode.AddComment(comments);
        }
        newNode = table.RootNode;
    }

    internal TomlValue FindAsDottedKey(ReadOnlySpan<byte> dottedKeySpan)
        => FindNodeAsDottedKey(RootNode, dottedKeySpan).Value!;

    internal TomlValue FindAsKey(ReadOnlySpan<byte> keySpan)
    {
        var node = FindNode(RootNode, keySpan);
        if (!node!.IsGroupingProperty)
        {
            return node.Value!;
        }
        return Empty;
    }

    internal TomlValue FindArrayOfTableOrValueAsDotted(ReadOnlySpan<byte> keys)
    {
        var node = FindNodeAsDottedKey(RootNode, keys);
        if (!node!.IsGroupingProperty || node!.IsArrayOfTablesHeader)
        {
            return node.Value!;
        }
        return Empty;
    }

    internal TomlValue FindArrayOfTableOrValue(ReadOnlySpan<byte> keySpan)
    {
        var node = FindNode(RootNode, keySpan);
        if (!node!.IsGroupingProperty || node!.IsArrayOfTablesHeader)
        {
            return node.Value!;
        }
        return Empty;
    }

    internal TomlValue FindArrayOfTablesOrValue(ReadOnlySpan<ByteArray> keys)
    {
        var node = FindNode(RootNode, keys);
        if (!node!.IsGroupingProperty || node!.IsArrayOfTablesHeader)
        {
            return node.Value!;
        }
        return Empty;
    }

    internal TomlTableNode FindNodeAsDottedKey(TomlTableNode root, ReadOnlySpan<byte> dottedKeySpan)
    {
        var hit = false;
        var currentNode = root;
        foreach (var dottedKey in dottedKeySpan.SplitSpan("."u8))
        {
            if (currentNode!.TryGetChildNode(dottedKey.Value, out var childNode))
            {
                hit = true;
                currentNode = childNode;
                continue;
            }
            return TomlTableNode.Empty;
        }
        
        return hit ? currentNode! : TomlTableNode.Empty;
    }

    internal TomlTableNode FindNode(TomlTableNode root, ReadOnlySpan<byte> dottedKeySpan)
        => root.TryGetChildNode(dottedKeySpan, out var childNode) ? childNode! : TomlTableNode.Empty;

    internal TomlTableNode FindNode(TomlTableNode root, ReadOnlySpan<ByteArray> dottedKeys)
    {
        var hit = false;
        var currentNode = root;

        for (int i = 0; i < dottedKeys.Length; i++)
        {
            if (currentNode!.TryGetChildNode(dottedKeys[i].value, out var childNode))
            {
                hit = true;
                currentNode = childNode;
                continue;
            }
            return TomlTableNode.Empty;
        }

        return hit ? currentNode! : TomlTableNode.Empty;
    }

    internal override bool ToTomlString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer)
    {
        var csTomlWriter = new CsTomlWriter<TBufferWriter>(ref writer);
        var keys = new List<TomlDotKey>();
        var tableNodes = new List<TomlDotKey>();
        ToTomlStringCore(ref csTomlWriter, RootNode, keys, tableNodes);

        return true;
    }

    private void ToTomlStringCore<TBufferWriter>(ref CsTomlWriter<TBufferWriter> writer, TomlTableNode parentNode, List<TomlDotKey> keys, List<TomlDotKey> tableHeaderKeys)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (parentNode.IsArrayOfTablesHeader)
        {
            if (parentNode.Value is TomlArray arrayValue)
            {
                var skipNewLine = false;
                if (parentNode.CommentCount > 0)
                {
                    if (writer.WrittingCount > 0)
                    {
                        writer.WriteNewLine();
                    }
                    writer.WriteComments(parentNode.CommentSpan);
                    skipNewLine = true;
                }
                var keysSpan = CollectionsMarshal.AsSpan(tableHeaderKeys);
                foreach (var v in arrayValue)
                {
                    if (writer.WrittingCount > 0 && !skipNewLine)
                    {
                        writer.WriteNewLine();
                    }
                    writer.WriteArrayOfTablesHeader(keysSpan);
                    if (v is TomlTable table)
                    {
                        table.ToTomlStringCore(ref writer, table.RootNode, [], tableHeaderKeys);
                    }
                    skipNewLine = false;
                }
            }
        }

        foreach (var (key, childNode) in parentNode.KeyValuePairs)
        {
            tableHeaderKeys.Add(key);
            if (childNode.IsGroupingProperty)
            {
                if (!childNode.IsTableHeader && parentNode.IsTableHeader && keys.Count > 0)
                {
                    var skipNewLine = false;
                    if (parentNode.CommentCount > 0)
                    {
                        writer.WriteComments(parentNode.CommentSpan);
                        skipNewLine = true;
                    }
                    if (writer.WrittingCount > 0 && !skipNewLine)
                    {
                        writer.WriteNewLine();
                    }
                    var keysSpan = CollectionsMarshal.AsSpan(keys);
                    writer.WriteTableHeader(keysSpan);
                    keys.Clear();
                }
                keys.Add(key);
                ToTomlStringCore(ref writer, childNode, keys, tableHeaderKeys);
            }
            else
            {
                tableHeaderKeys.RemoveAt(tableHeaderKeys.Count - 1);
                if (parentNode.IsTableHeader && keys.Count > 0)
                {
                    var skipNewLine = false;
                    if (parentNode.CommentCount > 0)
                    {
                        if (writer.WrittingCount > 0)
                        {
                            writer.WriteNewLine();
                        }
                        writer.WriteComments(parentNode.CommentSpan);
                        skipNewLine = true;
                    }
                    if (writer.WrittingCount > 0 && !skipNewLine)
                    {
                        writer.WriteNewLine();
                    }
                    var keysSpan = CollectionsMarshal.AsSpan(tableHeaderKeys);
                    writer.WriteTableHeader(keysSpan);
                    keys.Clear();
                    writer.WriteKeyValueAndNewLine(key, childNode.Value!);
                }
                else
                {
                    if (childNode.CommentCount > 0)
                    {
                        writer.WriteComments(childNode.CommentSpan);
                    }
                    var keysSpan = CollectionsMarshal.AsSpan(keys);
                    if (keysSpan.Length > 0)
                    {
                        for (var i = 0; i < keysSpan.Length; i++)
                        {
                            writer.WriterKey(keysSpan[i], true);
                        }
                    }
                    writer.WriteKeyValueAndNewLine(key, childNode.Value!);
                }
            }
            tableHeaderKeys.Remove(key);
        }

        keys.Clear(); // clear subkey
    }

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        var tableFormat = $"TOML Table[{node.NodeCount}]";
        if (tableFormat.TryCopyTo(destination))
        {
            charsWritten = tableFormat.Length;
            return true;
        }
        charsWritten = 0;
        return false;
    }

    public override string ToString(string? format, IFormatProvider? formatProvider)
        => ToString();

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (!"TOML Table["u8.TryCopyTo(utf8Destination))
        {
            bytesWritten = 0;
            return false;
        }
        var written = 11;

        if (!node.NodeCount.TryFormat(utf8Destination.Slice(written), out var byteWritten2, format, provider))
        {
            bytesWritten = 0;
            return false;
        }
        written += byteWritten2;

        if (utf8Destination.Length - written <= 0)
        {
            bytesWritten = 0;
            return false;
        }

        utf8Destination[written++] = TomlCodes.Symbol.RIGHTSQUAREBRACKET;
        bytesWritten = written;
        return true;
    }

    public override string ToString()
    {
        var length = 65536; // 64K;
        var bufferWriter = RecycleArrayPoolBufferWriter<char>.Rent();
        try
        {
            var conflictCount = 0;
            var charsWritten = 0;
            while (!TryFormat(bufferWriter.GetSpan(length), out charsWritten))
            {
                if (++conflictCount >= 15)
                {
                    break;
                }
                length *= 2;
            }

            bufferWriter.Advance(charsWritten);
            return new string(bufferWriter.WrittenSpan);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<char>.Return(bufferWriter);
        }
    }

}
