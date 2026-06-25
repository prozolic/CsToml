using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml.Values;

[DebuggerDisplay("Table = {RootNode.NodeCount}")]
internal sealed partial class TomlTable : TomlValue
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly TomlTableNode node = new() { IsGroupingProperty = true, Value = TomlValue.Empty };

    public override bool HasValue => true;

    public override TomlValueType Type => TomlValueType.Table;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    internal TomlTableNode RootNode => node;

    internal TomlTable() { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal IDictionary<object, object> GetDictionary()
        => node.GetDictionary();

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        InlineArray4<TomlDottedKey> initialKeys = default;
        ref InlineArray4<TomlDottedKey> initialKeysRef = ref Unsafe.AsRef(in initialKeys);
        TempList<TomlDottedKey> keyList = new(initialKeysRef);

        InlineArray4<TomlDottedKey> initiaTableHeaderKeys = default;
        ref InlineArray4<TomlDottedKey> initiaTableHeaderKeysRef = ref Unsafe.AsRef(in initiaTableHeaderKeys);
        TempList<TomlDottedKey> tableHeaderKeyList = new(initiaTableHeaderKeysRef);

        ToTomlStringCore(ref writer, RootNode, ref keyList, ref tableHeaderKeyList);
    }

    private void ToTomlStringCore<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TomlTableNode parentNode, ref TempList<TomlDottedKey> keyList, ref TempList<TomlDottedKey> tableHeaderKeyList)
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
                var keysSpan = tableHeaderKeyList.Items;
                foreach (var v in arrayValue)
                {
                    if (writer.WrittenSize > 0 && !skipNewLine)
                    {
                        writer.WriteNewLine();
                    }
                    WriteArrayOfTablesHeader(ref writer, keysSpan);
                    if (v is TomlTable table)
                    {
                        InlineArray4<TomlDottedKey> initialKeys = default;
                        ref InlineArray4<TomlDottedKey> initialKeysRef = ref Unsafe.AsRef(in initialKeys);
                        TempList<TomlDottedKey> keyList2 = new(initialKeysRef);
                        table.ToTomlStringCore(ref writer, table.RootNode, ref keyList2, ref tableHeaderKeyList);
                    }
                    skipNewLine = false;
                }
            }
        }

        var hasArrayOfTables = false;
        foreach (var pair in parentNode.KeyValuePairs)
        {
            if (pair.Value.IsArrayOfTablesHeader)
            {
                hasArrayOfTables = true;
                break;
            }
        }

        var headerWritten = false;

        if (hasArrayOfTables)
        {
            // If array of table exists in current node, we need to sort the key-value pairs to ensure that non-array-of-tables come before array-of-tables.
            var orderedValues = new ExtendableArray<KeyValuePair<TomlDottedKey, TomlTableNode>>(parentNode.NodeCount);
            var firstAoTIndex = -1;
            foreach (var pair in parentNode.KeyValuePairs)
            {
                if (firstAoTIndex < 0 && pair.Value.IsArrayOfTablesHeader)
                {
                    firstAoTIndex = orderedValues.Count;
                }
                orderedValues.Add(pair);
            }

            if (firstAoTIndex >= 0)
            {
                var span = orderedValues.AsWritableSpan(firstAoTIndex, orderedValues.Count - firstAoTIndex);
                var temp = ArrayPool<KeyValuePair<TomlDottedKey, TomlTableNode>>.Shared.Rent(span.Length);
                try
                {
                    var writeIndex = 0;
                    for (var i = 0; i < span.Length; i++)
                    {
                        if (!span[i].Value.IsArrayOfTablesHeader)
                        {
                            temp[writeIndex++] = span[i];
                        }
                    }
                    for (var i = 0; i < span.Length; i++)
                    {
                        if (span[i].Value.IsArrayOfTablesHeader)
                        {
                            temp[writeIndex++] = span[i];
                        }
                    }
                    temp.AsSpan(0, span.Length).CopyTo(span);
                }
                finally
                {
                    ArrayPool<KeyValuePair<TomlDottedKey, TomlTableNode>>.Shared.Return(temp, clearArray: true);
                }
            }

            foreach (var (key, childNode) in orderedValues.AsSpan())
            {
                ProcessChildNode(ref writer, parentNode, key, childNode, ref keyList, ref tableHeaderKeyList, ref headerWritten);
            }

            orderedValues.Return();
        }
        else
        {
            foreach (var (key, childNode) in parentNode.KeyValuePairs)
            {
                ProcessChildNode(ref writer, parentNode, key, childNode, ref keyList, ref tableHeaderKeyList, ref headerWritten);
            }
        }

        if (!headerWritten && parentNode.IsTableHeaderDefinitionPosition && keyList.Count > 0)
        {
            if (writer.WrittenSize > 0)
            {
                writer.WriteNewLine();
            }
            if (parentNode.CommentCount > 0)
            {
                WriteComments(ref writer, parentNode.CommentSpan);
            }
            WriteTableHeader(ref writer, tableHeaderKeyList.Items);
        }

        keyList.Clear();
    }

    private void ProcessChildNode<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TomlTableNode parentNode, TomlDottedKey key, TomlTableNode childNode, ref TempList<TomlDottedKey> keyList, ref TempList<TomlDottedKey> tableHeaderKeyList, ref bool headerWritten)
        where TBufferWriter : IBufferWriter<byte>
    {
        tableHeaderKeyList.Add(key);
        if (childNode.IsGroupingProperty)
        {
            if (!childNode.IsTableHeader && parentNode.IsTableHeader && keyList.Count > 0)
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
                var keysSpan = tableHeaderKeyList.Items.Slice(0, tableHeaderKeyList.Count - 1);
                WriteTableHeader(ref writer, keysSpan);
                keyList.Clear();
                headerWritten = true;
            }
            keyList.Add(key);
            ToTomlStringCore(ref writer, childNode, ref keyList, ref tableHeaderKeyList);
        }
        else
        {
            tableHeaderKeyList.RemoveLast();
            if (parentNode.IsTableHeader && keyList.Count > 0)
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
                var keysSpan = tableHeaderKeyList.Items;
                WriteTableHeader(ref writer, keysSpan);
                keyList.Clear();
                headerWritten = true;
                WriteKeyValueAndNewLine(ref writer, key, childNode.Value!);
            }
            else if (parentNode.IsTableHeaderDefinitionPosition && !headerWritten)
            {
                if (writer.WrittenSize > 0)
                {
                    writer.WriteNewLine();
                }
                if (parentNode.CommentCount > 0)
                {
                    WriteComments(ref writer, parentNode.CommentSpan);
                }
                var keysSpan = tableHeaderKeyList.Items;
                WriteTableHeader(ref writer, keysSpan);
                headerWritten = true;
                if (childNode.CommentCount > 0)
                {
                    WriteComments(ref writer, childNode.CommentSpan);
                }
                WriteKeyValueAndNewLine(ref writer, key, childNode.Value!);
            }
            else
            {
                if (childNode.CommentCount > 0)
                {
                    WriteComments(ref writer, childNode.CommentSpan);
                }
                var keysSpan = keyList.Items;
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
        tableHeaderKeyList.RemoveLastIfFound(key);
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
    {
        var length = 65536; // 64K;
        var bufferWriter = RecycleArrayPoolBufferWriter<char>.Rent();
        try
        {
            var conflictCount = 0;
            var charsWritten = 0;
            while (!TryFormat(bufferWriter.GetSpan(length), out charsWritten, format, formatProvider))
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

    public override string ToString() => ToString(null, null);
}
