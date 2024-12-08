using CsToml.Error;
using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Values;

[DebuggerDisplay("Table = {RootNode.NodeCount}")]
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

    internal TomlTable() { }

    internal void AddTableHeader(ReadOnlySpan<TomlDottedKey> dotKeys, IReadOnlyCollection<TomlString>? comments, out TomlTableNode? newNode)
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

            // dotKeys[i] is already defined.
            newNode = RootNode;
            ExceptionHelper.ThrowKeyIsDefined(dotKeys[i]);
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

    internal void AddArrayOfTablesHeader(ReadOnlySpan<TomlDottedKey> dotKeys, IReadOnlyCollection<TomlString>? comments, out TomlTableNode? newNode)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal IDictionary<string, object?> GetDictionary()
        => node.GetDictionary();

    internal override bool ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        var keys = new List<TomlDottedKey>();
        var tableNodes = new List<TomlDottedKey>();
        ToTomlStringCore(ref writer, RootNode, keys, tableNodes);

        return true;
    }

    private void ToTomlStringCore<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TomlTableNode parentNode, List<TomlDottedKey> keys, List<TomlDottedKey> tableHeaderKeys)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (parentNode.IsArrayOfTablesHeader)
        {
            if (parentNode.Value is TomlArray arrayValue)
            {
                var skipNewLine = false;
                if (parentNode.CommentCount > 0)
                {
                    if (writer.WrittenSize > 0)
                    {
                        writer.WriteNewLine();
                    }

                    WriteComments(ref writer, parentNode.CommentSpan);
                    skipNewLine = true;
                }
                var keysSpan = CollectionsMarshal.AsSpan(tableHeaderKeys);
                foreach (var v in arrayValue)
                {
                    if (writer.WrittenSize > 0 && !skipNewLine)
                    {
                        writer.WriteNewLine();
                    }
                    WriteArrayOfTablesHeader(ref writer, keysSpan);
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
                        WriteComments(ref writer, parentNode.CommentSpan);
                        skipNewLine = true;
                    }
                    if (writer.WrittenSize > 0 && !skipNewLine)
                    {
                        writer.WriteNewLine();
                    }
                    var keysSpan = CollectionsMarshal.AsSpan(keys);
                    WriteTableHeader(ref writer, keysSpan);
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
                        if (writer.WrittenSize > 0)
                        {
                            writer.WriteNewLine();
                        }
                        WriteComments(ref writer, parentNode.CommentSpan);
                        skipNewLine = true;
                    }
                    if (writer.WrittenSize > 0 && !skipNewLine)
                    {
                        writer.WriteNewLine();
                    }
                    var keysSpan = CollectionsMarshal.AsSpan(tableHeaderKeys);
                    WriteTableHeader(ref writer, keysSpan);
                    keys.Clear();
                    WriteKeyValueAndNewLine(ref writer, key, childNode.Value!);
                }
                else
                {
                    if (childNode.CommentCount > 0)
                    {
                        WriteComments(ref writer, childNode.CommentSpan);
                    }
                    var keysSpan = CollectionsMarshal.AsSpan(keys);
                    if (keysSpan.Length > 0)
                    {
                        for (var i = 0; i < keysSpan.Length; i++)
                        {
                            WriterKey(ref writer, keysSpan[i], true);
                        }
                    }
                    WriteKeyValueAndNewLine(ref writer, key, childNode.Value!);
                }
            }
            tableHeaderKeys.Remove(key);
        }

        keys.Clear(); // clear subkey
    }

    private void WriterKey<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TomlDottedKey key, bool isGroupingProperty)
        where TBufferWriter : IBufferWriter<byte>
    {
        key.ToTomlString(ref writer);
        if (isGroupingProperty)
        {
            writer.Write(TomlCodes.Symbol.DOT);
        }
    }

    private void WriteKeyValueAndNewLine<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TomlDottedKey key, TomlValue value)
        where TBufferWriter : IBufferWriter<byte>
    {
        WriterKey(ref writer, key, false);
        writer.WriteEqual();
        value.ToTomlString(ref writer);
        writer.WriteNewLine();
    }

    private void WriteTableHeader<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<TomlDottedKey> keysSpan)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.BeginTableHeader();
        if (keysSpan.Length > 0)
        {
            for (var i = 0; i < keysSpan.Length; i++)
            {
                WriterKey(ref writer, keysSpan[i], i < keysSpan.Length - 1);
            }
        }

        writer.EndTableHeader();
        writer.WriteNewLine();
    }

    private void WriteArrayOfTablesHeader<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<TomlDottedKey> keysSpan)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.BeginArrayOfTablesHeader();

        if (keysSpan.Length > 0)
        {
            for (var i = 0; i < keysSpan.Length; i++)
            {
                WriterKey(ref writer, keysSpan[i], i < keysSpan.Length - 1);
            }
        }

        writer.EndArrayOfTablesHeader();
        writer.WriteNewLine();
    }

    private void WriteComments<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<TomlString> comments)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (comments.Length == 0) return;

        for (var i = 0; i < comments.Length; i++)
        {
            writer.Write(TomlCodes.Symbol.NUMBERSIGN);
            comments[i].ToTomlString(ref writer);
            writer.WriteNewLine();
        }
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
