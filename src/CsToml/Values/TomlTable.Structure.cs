using CsToml.Error;
using CsToml.Utility;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Values;

internal sealed partial class TomlTable
{
    private TomlTableState state;
    private List<TomlString>? comments;
    private List<KeyValueComments>? keyValueComments;

    internal bool IsTableHeader => (state & TomlTableState.Header) != 0;

    internal bool IsTableHeaderDefinitionPosition => (state & TomlTableState.Defined) != 0;

    internal bool IsViaArrayOfTables => (state & TomlTableState.ViaArrayOfTables) != 0;

    internal int CommentCount => comments?.Count ?? 0;

    internal ReadOnlySpan<TomlString> CommentSpan => comments != null ? CollectionsMarshal.AsSpan(comments) : ReadOnlySpan<TomlString>.Empty;

    internal static Span<TomlString> AppendComments(ref List<TomlString>? comments, int commentCount)
    {
        var list = comments ??= new List<TomlString>(commentCount);
        var start = list.Count;
        CollectionsMarshal.SetCount(list, start + commentCount);
        return CollectionsMarshal.AsSpan(list).Slice(start, commentCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Span<TomlString> AddComments(int commentCount)
    {
        return AppendComments(ref comments, commentCount);
    }

    // Entries are only appended and a key/value receives its comments right after it is added,
    // so keyValueComments stays ordered by entry index.
    internal Span<TomlString> AddKeyValueComments(int entryIndex, int commentCount)
    {
        var values = new TomlString[commentCount];
        var list = keyValueComments ??= new List<KeyValueComments>();
        Debug.Assert(list.Count == 0 || list[^1].EntryIndex <= entryIndex);
        list.Add(new KeyValueComments(entryIndex, values));
        return values;
    }

    internal ReadOnlySpan<TomlString> GetKeyValueComments(int entryIndex)
    {
        if (keyValueComments == null)
        {
            return ReadOnlySpan<TomlString>.Empty;
        }

        var span = CollectionsMarshal.AsSpan(keyValueComments);
        var low = 0;
        var high = span.Length - 1;
        while (low <= high)
        {
            var middle = (int)(((uint)low + (uint)high) >> 1);
            var index = span[middle].EntryIndex;
            if (index == entryIndex)
            {
                return span[middle].Comments;
            }
            if (index < entryIndex)
            {
                low = middle + 1;
            }
            else
            {
                high = middle - 1;
            }
        }
        return ReadOnlySpan<TomlString>.Empty;
    }

    internal bool TryGetKey(ReadOnlySpan<byte> key, out TomlDottedKey? dottedKey)
        => TryGetValueCore(key, ByteArrayHash.ToInt32(key), out dottedKey, out _);

    internal TomlTable? TryGetMirrorChild(TomlDottedKey key)
    {
        ref var entry = ref FindEntry(key, key.GetHashCodeFast());
        if (Unsafe.IsNullRef(ref entry))
        {
            return null;
        }

        var value = entry.value;
        if (value is TomlTableArray tableArray)
        {
            return tableArray.LastTable;
        }
        return value as TomlTable;
    }

    internal TomlTable GetOrAddArrayOfTableHeaderKeyTable(TomlDottedKey key)
    {
        var hashCode = key.GetHashCodeFast();
        ref var entry = ref FindEntry(key, hashCode);
        if (Unsafe.IsNullRef(ref entry))
        {
            return AddTable(key, hashCode, TomlTableState.Header | TomlTableState.ViaArrayOfTables);
        }

        var value = entry.value;
        if (value is TomlTable table)
        {
            return table;
        }
        if (value is TomlTableArray tableArray)
        {
            return tableArray.LastTable;
        }

        ExceptionHelper.ThrowIncorrectTomlFormat();
        return default;
    }

    internal TomlTable AddArrayOfTableHeaderKeyLastTable(TomlDottedKey key, out TomlTableArray tableArray, out TomlTable? previousTable)
    {
        var hashCode = key.GetHashCodeFast();
        ref var entry = ref FindEntry(key, hashCode);
        TomlTableArray? array;
        if (Unsafe.IsNullRef(ref entry))
        {
            array = new TomlTableArray();
            previousTable = null;
            AddNew(key, hashCode, array);
        }
        else
        {
            var value = entry.value;
            array = value as TomlTableArray;
            if (array == null)
            {
                if (value is TomlTable)
                {
                    ExceptionHelper.ThrowTheArrayOfTablesIsDefinedAsTable(key.ToString());
                }
                ExceptionHelper.ThrowIncorrectTomlFormat();
            }
            previousTable = array.LastTable;
        }

        var table = new TomlTable();
        array.Add(table);
        tableArray = array;
        return table;
    }

    internal TomlTable GetOrAddTableHeaderKeyTable(TomlDottedKey key, out bool newTable)
    {
        var hashCode = key.GetHashCodeFast();
        ref var entry = ref FindEntry(key, hashCode);
        if (Unsafe.IsNullRef(ref entry))
        {
            newTable = true;
            return AddTable(key, hashCode, TomlTableState.Header);
        }

        newTable = false;
        var value = entry.value;
        if (value is TomlTable table)
        {
            return table;
        }
        if (value is TomlTableArray tableArray)
        {
            return tableArray.LastTable;
        }

        // key is already defined.
        ExceptionHelper.ThrowKeyIsDefined(key);
        return default;
    }

    internal TomlTable AddTableHeaderKeyLastTable(TomlDottedKey key)
    {
        var table = GetOrAddTableHeaderKeyTable(key, out var newTable);

        if (!newTable)
        {
            if (table.IsTableHeaderDefinitionPosition || !table.IsTableHeader)
            {
                ExceptionHelper.ThrowTableHeaderIsDefined(key.ToString());
            }
        }

        table.state |= TomlTableState.Defined;
        return table;
    }

    internal TomlTable GetOrAddKeyTable(TomlDottedKey key)
    {
        var hashCode = key.GetHashCodeFast();
        ref var entry = ref FindEntry(key, hashCode);
        if (Unsafe.IsNullRef(ref entry))
        {
            return AddTable(key, hashCode, TomlTableState.None);
        }

        var value = entry.value;
        if (value is TomlTable table)
        {
            if (table.IsTableHeaderDefinitionPosition)
            {
                ExceptionHelper.ThrowTheKeyIsDefinedAsTable();
            }
            return table;
        }
        if (value is TomlTableArray)
        {
            ExceptionHelper.ThrowTheKeyIsDefinedAsArrayOfTables();
        }

        ExceptionHelper.ThrowKeyIsDefined(key);
        return default;
    }

    internal int AddKeyValue(TomlDottedKey key, TomlValue value)
    {
        var index = TryAdd(key, key.GetHashCodeFast(), value);
        if (index < 0)
        {
            ExceptionHelper.ThrowKeyIsDefined(key);
        }

        return index;
    }

    private TomlTable AddTable(TomlDottedKey key, int hashCode, TomlTableState state)
    {
        var table = new TomlTable(state);
        AddNew(key, hashCode, table);
        return table;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryGetChildValue(ReadOnlySpan<byte> key, out TomlValue? value)
    {
        if (TryGetValueCore(key, ByteArrayHash.ToInt32(key), out _, out value))
        {
            return true;
        }

        if (!key.Contains(TomlCodes.Symbol.BACKSLASH))
        {
            return false;
        }

        return TryGetChildValueSlow(key, out value);
    }

    private bool TryGetChildValueSlow(ReadOnlySpan<byte> key, out TomlValue? value)
    {
        var reader = new Utf8SequenceReader(key);
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            while (reader.TryPeek(out var ch))
            {
                if (TomlCodes.IsBackSlash(ch))
                {
                    reader.Advance(1);

                    if (TomlCodes.TryParseEscapeSequence(
                        ref reader,
                        bufferWriter,
                        multiLine: false,
                        supportsEscapeSequenceE: true,
                        supportsEscapeSequenceX: true,
                        throwError: false) == EscapeSequenceResult.Failure)
                    {
                        value = default;
                        return false;
                    }
                    continue;
                }

                bufferWriter.Write(ch);
                reader.Advance(1);
            }

            // search Quoted keys
            var unescapedKey = bufferWriter.WrittenSpan;
            return TryGetValueCore(unescapedKey, ByteArrayHash.ToInt32(unescapedKey), out _, out value);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }

    internal IDictionary<object, object> GetDictionary()
    {
        var dictionary = new Dictionary<object, object>(count);
        foreach (ref var entry in entries.AsSpan(0, count))
        {
            dictionary.Add(entry.key.Utf16String, entry.value.GetObject());
        }
        return dictionary;
    }

    private readonly struct KeyValueComments(int entryIndex, TomlString[] comments)
    {
        public readonly int EntryIndex = entryIndex;
        public readonly TomlString[] Comments = comments;
    }
}
