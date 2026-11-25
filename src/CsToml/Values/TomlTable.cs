using CsToml.Debugger;
using CsToml.Utility;
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

    internal override void ToTomlString(ref Utf8TomlDocumentWriter writer)
    {
        InlineArray4<TomlDottedKey> initialKeys = default;
        ref InlineArray4<TomlDottedKey> initialKeysRef = ref Unsafe.AsRef(in initialKeys);
        TempList<TomlDottedKey> keyList = new(initialKeysRef);

        InlineArray4<TomlDottedKey> initiaTableHeaderKeys = default;
        ref InlineArray4<TomlDottedKey> initiaTableHeaderKeysRef = ref Unsafe.AsRef(in initiaTableHeaderKeys);
        TempList<TomlDottedKey> tableHeaderKeyList = new(initiaTableHeaderKeysRef);

        ToTomlStringCore(ref writer, ref keyList, ref tableHeaderKeyList, WriteTarget.All);
    }

    private bool ToTomlStringCore(ref Utf8TomlDocumentWriter writer, ref TempList<TomlDottedKey> keyList, ref TempList<TomlDottedKey> tableHeaderKeyList, WriteTarget target)
    {
        var headerWritten = false;
        var first = 0;
        var hasTable = (target & WriteTarget.KeyValue) == 0;
        var hasArrayOfTables = hasTable;

        if (IsTableHeader)
        {
            for (; first < count; first++)
            {
                var value = entries[first].value;
                if (IsArrayOfTables(value))
                {
                    hasArrayOfTables = true;
                    continue;
                }
                if (value is not TomlTable { IsTableHeader: true })
                {
                    break;
                }
                WriteChildTable(ref writer, first, ref keyList, ref tableHeaderKeyList, ref headerWritten, WriteTarget.All);
            }
        }

        if ((target & WriteTarget.KeyValue) != 0)
        {
            for (var i = first; i < count; i++)
            {
                var value = entries[i].value;
                if (value is TomlTable table)
                {
                    if (!table.IsTableHeader)
                    {
                        hasTable |= WriteChildTable(ref writer, i, ref keyList, ref tableHeaderKeyList, ref headerWritten, WriteTarget.KeyValue);
                    }
                    else if (table.IsViaArrayOfTables)
                    {
                        hasArrayOfTables = true;
                    }
                    else
                    {
                        hasTable = true;
                    }
                }
                else if (value is TomlTableArray)
                {
                    hasArrayOfTables = true;
                }
                else
                {
                    WriteKeyValue(ref writer, i, ref keyList, ref tableHeaderKeyList, ref headerWritten);
                }
            }
        }

        if ((target & WriteTarget.Table) != 0)
        {
            if (hasTable)
            {
                for (var i = first; i < count; i++)
                {
                    if (entries[i].value is TomlTable { IsViaArrayOfTables: false })
                    {
                        WriteChildTable(ref writer, i, ref keyList, ref tableHeaderKeyList, ref headerWritten, WriteTarget.Table);
                    }
                }
            }
            if (hasArrayOfTables)
            {
                for (var i = 0; i < count; i++)
                {
                    if (IsArrayOfTables(entries[i].value))
                    {
                        WriteChildTable(ref writer, i, ref keyList, ref tableHeaderKeyList, ref headerWritten, WriteTarget.Table);
                    }
                }
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

        return hasTable || hasArrayOfTables;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsArrayOfTables(TomlValue value)
        => value is TomlTableArray || (value is TomlTable table && table.IsViaArrayOfTables);

    private void WriteArrayOfTables(ref Utf8TomlDocumentWriter writer, TomlTableArray tableArray, ref TempList<TomlDottedKey> tableHeaderKeyList)
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
                table.ToTomlStringCore(ref writer, ref keyList2, ref tableHeaderKeyList, WriteTarget.All);
            }
            skipNewLine = false;
        }
    }

    private bool WriteChildTable(ref Utf8TomlDocumentWriter writer, int index, ref TempList<TomlDottedKey> keyList, ref TempList<TomlDottedKey> tableHeaderKeyList, ref bool headerWritten, WriteTarget target)
    {
        var key = entries[index].key;
        var value = entries[index].value;
        var childTable = value as TomlTable;
        var hasTable = false;

        tableHeaderKeyList.Add(key);
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
            hasTable = childTable.ToTomlStringCore(ref writer, ref keyList, ref tableHeaderKeyList, childTable.IsTableHeader ? WriteTarget.All : target);
        }
        else
        {
            WriteArrayOfTables(ref writer, (TomlTableArray)value, ref tableHeaderKeyList);
        }
        keyList.TruncateTo(savedKeyCount);
        tableHeaderKeyList.RemoveLast();
        return hasTable;
    }

    private void WriteKeyValue(ref Utf8TomlDocumentWriter writer, int index, ref TempList<TomlDottedKey> keyList, ref TempList<TomlDottedKey> tableHeaderKeyList, ref bool headerWritten)
    {
        var key = entries[index].key;
        var value = entries[index].value;

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

    private void WriterKey(ref Utf8TomlDocumentWriter writer, TomlDottedKey key, bool isGroupingProperty)
    {
        key.ToTomlString(ref writer);
        if (isGroupingProperty)
        {
            writer.Write(TomlCodes.Symbol.DOT);
        }
    }

    private void WriteKeyValueAndNewLine(ref Utf8TomlDocumentWriter writer, TomlDottedKey key, TomlValue value)
    {
        WriterKey(ref writer, key, false);
        writer.WriteEqual();
        value.ToTomlString(ref writer);
        writer.WriteNewLine();
    }

    private void WriteTableHeader(ref Utf8TomlDocumentWriter writer, ReadOnlySpan<TomlDottedKey> keysSpan)
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

    private void WriteArrayOfTablesHeader(ref Utf8TomlDocumentWriter writer, ReadOnlySpan<TomlDottedKey> keysSpan)
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

    private void WriteComments(ref Utf8TomlDocumentWriter writer, ReadOnlySpan<TomlString> comments)
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

    [Flags]
    private enum WriteTarget : byte
    {
        KeyValue = 1 << 0,
        Table = 1 << 1,
        All = KeyValue | Table,
    }
}
