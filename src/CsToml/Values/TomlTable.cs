using CsToml.Debugger;
using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml.Values;

[DebuggerTypeProxy(typeof(TomlTableDebugView))]
[DebuggerDisplay("Table = {Count}")]
internal sealed partial class TomlTable : TomlValue
{
    public override bool HasValue => true;

    public override TomlValueType Type => TomlValueType.Table;

    internal TomlTable() { }

    private TomlTable(TomlTableState state)
    {
        this.state = state;
    }

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        InlineArray4<TomlDottedKey> initialKeys = default;
        ref InlineArray4<TomlDottedKey> initialKeysRef = ref Unsafe.AsRef(in initialKeys);
        TempList<TomlDottedKey> keyList = new(initialKeysRef);

        InlineArray4<TomlDottedKey> initiaTableHeaderKeys = default;
        ref InlineArray4<TomlDottedKey> initiaTableHeaderKeysRef = ref Unsafe.AsRef(in initiaTableHeaderKeys);
        TempList<TomlDottedKey> tableHeaderKeyList = new(initiaTableHeaderKeysRef);

        ToTomlStringCore(ref writer, ref keyList, ref tableHeaderKeyList);
    }

    private void ToTomlStringCore<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ref TempList<TomlDottedKey> keyList, ref TempList<TomlDottedKey> tableHeaderKeyList)
        where TBufferWriter : IBufferWriter<byte>
    {
        var firstArrayOfTablesIndex = -1;
        for (var i = 0; i < count; i++)
        {
            if (IsArrayOfTables(entries[i].value))
            {
                firstArrayOfTablesIndex = i;
                break;
            }
        }

        var headerWritten = false;

        if (firstArrayOfTablesIndex >= 0)
        {
            for (var i = 0; i < count; i++)
            {
                if (i < firstArrayOfTablesIndex || !IsArrayOfTables(entries[i].value))
                {
                    WriteEntry(ref writer, i, ref keyList, ref tableHeaderKeyList, ref headerWritten);
                }
            }
            for (var i = firstArrayOfTablesIndex; i < count; i++)
            {
                if (IsArrayOfTables(entries[i].value))
                {
                    WriteEntry(ref writer, i, ref keyList, ref tableHeaderKeyList, ref headerWritten);
                }
            }
        }
        else
        {
            for (var i = 0; i < count; i++)
            {
                WriteEntry(ref writer, i, ref keyList, ref tableHeaderKeyList, ref headerWritten);
            }
        }

        if (!headerWritten && IsTableHeaderDefinitionPosition && keyList.Count > 0)
        {
            if (writer.WrittenSize > 0)
            {
                writer.WriteNewLine();
            }
            if (CommentCount > 0)
            {
                WriteComments(ref writer, CommentSpan);
            }
            WriteTableHeader(ref writer, tableHeaderKeyList.Items);
        }

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsArrayOfTables(TomlValue value)
        => value is TomlTableArray || (value is TomlTable table && table.IsViaArrayOfTables);

    private void WriteArrayOfTables<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TomlTableArray tableArray, ref TempList<TomlDottedKey> tableHeaderKeyList)
        where TBufferWriter : IBufferWriter<byte>
    {
        var skipNewLine = false;
        if (tableArray.CommentCount > 0)
        {
            if (writer.WrittenSize > 0)
            {
                writer.WriteNewLine();
            }

            WriteComments(ref writer, tableArray.CommentSpan);
            skipNewLine = true;
        }
        var keysSpan = tableHeaderKeyList.Items;
        foreach (var v in tableArray)
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
                table.ToTomlStringCore(ref writer, ref keyList2, ref tableHeaderKeyList);
            }
            skipNewLine = false;
        }
    }

    private void WriteEntry<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, int index, ref TempList<TomlDottedKey> keyList, ref TempList<TomlDottedKey> tableHeaderKeyList, ref bool headerWritten)
        where TBufferWriter : IBufferWriter<byte>
    {
        var key = entries[index].key;
        var value = entries[index].value;
        var childTable = value as TomlTable;

        tableHeaderKeyList.Add(key);
        if (childTable != null || value is TomlTableArray)
        {
            if (!(childTable?.IsTableHeader ?? false) && IsTableHeader && keyList.Count > 0)
            {
                var skipNewLine = false;
                if (CommentCount > 0)
                {
                    WriteComments(ref writer, CommentSpan);
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

            var savedKeyCount = keyList.Count;
            keyList.Add(key);

            if (childTable != null)
            {
                childTable.ToTomlStringCore(ref writer, ref keyList, ref tableHeaderKeyList);
            }
            else
            {
                WriteArrayOfTables(ref writer, (TomlTableArray)value, ref tableHeaderKeyList);
            }
            keyList.TruncateTo(savedKeyCount);
        }
        else
        {
            tableHeaderKeyList.RemoveLast();
            if (IsTableHeader && keyList.Count > 0)
            {
                var skipNewLine = false;
                if (CommentCount > 0)
                {
                    if (writer.WrittenSize > 0)
                    {
                        writer.WriteNewLine();
                    }
                    WriteComments(ref writer, CommentSpan);
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
                WriteComments(ref writer, GetKeyValueComments(index));
                WriteKeyValueAndNewLine(ref writer, key, value);
            }
            else if (IsTableHeaderDefinitionPosition && !headerWritten)
            {
                if (writer.WrittenSize > 0)
                {
                    writer.WriteNewLine();
                }
                if (CommentCount > 0)
                {
                    WriteComments(ref writer, CommentSpan);
                }
                var keysSpan = tableHeaderKeyList.Items;
                WriteTableHeader(ref writer, keysSpan);

                headerWritten = true;
                WriteComments(ref writer, GetKeyValueComments(index));
                WriteKeyValueAndNewLine(ref writer, key, value);
            }
            else
            {
                WriteComments(ref writer, GetKeyValueComments(index));

                var keysSpan = keyList.Items;
                if (keysSpan.Length > 0)
                {
                    for (var i = 0; i < keysSpan.Length; i++)
                    {
                        WriterKey(ref writer, keysSpan[i], true);
                    }
                }
                WriteKeyValueAndNewLine(ref writer, key, value);
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
        var tableFormat = $"TOML Table[{count}]";
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

        if (!count.TryFormat(utf8Destination.Slice(written), out var byteWritten2, format, provider))
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
