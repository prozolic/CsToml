using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Text.Unicode;

namespace CsToml;

public partial class CsTomlPackage
{
    public CsTomlValue? Find(ReadOnlySpan<byte> keys, bool isDottedKeys = false)
        => table.Find(keys, isDottedKeys);

    public CsTomlValue? Find(ReadOnlySpan<ByteArray> keys)
        => table.Find(keys);

    public CsTomlValue? Find(ReadOnlySpan<char> keys, bool isDottedKeys = false)
    {
        var maxBufferSize = (keys.Length + 1) * 3;
        if (maxBufferSize < 1024)
        {
            Span<byte> utf8Span = stackalloc byte[maxBufferSize];
            var status = Utf8.FromUtf16(keys, utf8Span, out int _, out int bytesWritten, replaceInvalidSequences: false);
            if (status != System.Buffers.OperationStatus.Done)
            {
                if (status == OperationStatus.InvalidData)
                    ExceptionHelper.ThrowInvalidByteIncluded();
                ExceptionHelper.ThrowBufferTooSmallFailed();
            }

            return Find(utf8Span[..bytesWritten], isDottedKeys);
        }
        else
        {
            var writer = new ArrayPoolBufferWriter<byte>(128);
            using var _ = writer;

            var utf8Writer = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref writer);
            try
            {
                ValueFormatter.Serialize(ref utf8Writer, keys);
            }
            catch (CsTomlException)
            {
                return default;
            }

            return Find(writer.WrittenSpan, isDottedKeys);
        }
    }

    public CsTomlValue? Find(ReadOnlySpan<byte> tableHeader, ReadOnlySpan<byte> key, bool isTableHeaderAsDottedKeys = false, bool isDottedKeys = false)
    {
        var tableNode = isTableHeaderAsDottedKeys ? table.FindNodeAsDottedKey(table.RootNode, tableHeader) : table.FindNode(table.RootNode, tableHeader);
        if (tableNode.NodeCount == 0 && !tableNode.IsTableHeaderDefinitionPosition) return default;

        var keyNode = isDottedKeys ? table.FindNodeAsDottedKey(tableNode, key) : table.FindNode(tableNode, key);
        if (keyNode.NodeCount == 0 && !keyNode.IsGroupingProperty) return keyNode.Value;

        return default;
    }

    public CsTomlValue? Find(ReadOnlySpan<byte> tableHeader, ReadOnlySpan<ByteArray> key, bool isTableHeaderAsDottedKeys = false)
    {
        var tableNode = isTableHeaderAsDottedKeys ? table.FindNodeAsDottedKey(table.RootNode, tableHeader) : table.FindNode(table.RootNode, tableHeader);
        if (tableNode.NodeCount == 0 && !tableNode.IsTableHeaderDefinitionPosition) return default;

        var keyNode = table.FindNode(tableNode, key);
        if (keyNode.NodeCount == 0 && !keyNode.IsGroupingProperty) return keyNode.Value;

        return default;
    }

    public CsTomlValue? Find(ReadOnlySpan<ByteArray> tableHeader, ReadOnlySpan<byte> key, bool isDottedKeys = false)
    {
        var tableNode = table.FindNode(table.RootNode, tableHeader);
        if (tableNode.NodeCount == 0 && !tableNode.IsTableHeaderDefinitionPosition) return default;

        var keyNode = isDottedKeys ? table.FindNodeAsDottedKey(tableNode, key) : table.FindNode(tableNode, key);
        if (keyNode.NodeCount == 0 && !keyNode.IsGroupingProperty) return keyNode.Value;

        return default;
    }

    public CsTomlValue? Find(ReadOnlySpan<ByteArray> tableHeader, ReadOnlySpan<ByteArray> key)
    {
        var tableNode = table.FindNode(table.RootNode, tableHeader);
        if (tableNode.NodeCount == 0 && !tableNode.IsTableHeaderDefinitionPosition) return default;

        var keyNode = table.FindNode(tableNode, key);
        if (keyNode.NodeCount == 0 && !keyNode.IsGroupingProperty) return keyNode.Value;

        return default;
    }

    public CsTomlValue? Find(ReadOnlySpan<char> tableHeader, ReadOnlySpan<char> key, bool isTableHeaderAsDottedKeys = false, bool isDottedKeys = false)
    {
        // buffer size to 3 times worst-case (UTF16 -> UTF8)
        var maxBufferSize = (tableHeader.Length + key.Length + 1) * 3;
        if (maxBufferSize < 1024)
        {
            Span<byte> utf8Span = stackalloc byte[maxBufferSize];
            var status = Utf8.FromUtf16(tableHeader, utf8Span, out int _, out int bytesWritten, replaceInvalidSequences: false);
            if (status != System.Buffers.OperationStatus.Done)
            {
                if (status == OperationStatus.InvalidData)
                    ExceptionHelper.ThrowInvalidByteIncluded();
                ExceptionHelper.ThrowBufferTooSmallFailed();
            }
            var status2 = Utf8.FromUtf16(key, utf8Span.Slice(bytesWritten), out int __, out int bytesWritten2, replaceInvalidSequences: false);
            if (status2 != System.Buffers.OperationStatus.Done)
            {
                if (status == OperationStatus.InvalidData)
                    ExceptionHelper.ThrowInvalidByteIncluded();
                ExceptionHelper.ThrowBufferTooSmallFailed();
            }

            return Find(
                utf8Span.Slice(0, bytesWritten),
                utf8Span.Slice(bytesWritten, bytesWritten2),
                isTableHeaderAsDottedKeys, isDottedKeys);
        }
        else
        {
            var writer = new ArrayPoolBufferWriter<byte>(128);
            using var _ = writer;
            var tableHeaderWriter = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref writer);
            var keyrWriter = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref writer);
            try
            {
                ValueFormatter.Serialize(ref tableHeaderWriter, tableHeader);
                ValueFormatter.Serialize(ref keyrWriter, key);
            }
            catch (CsTomlException)
            {
                return default;
            }

            return Find(
                writer.WrittenSpan.Slice(0, tableHeaderWriter.WrittingCount),
                writer.WrittenSpan.Slice(tableHeaderWriter.WrittingCount, keyrWriter.WrittingCount),
                isDottedKeys);
        }
    }

    public CsTomlValue? Find(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, bool isTableHeaderAsDottedKeys = false, bool isDottedKeys = false)
    {
        // find Array Of Tables
        var arrayOfTablesValue = isTableHeaderAsDottedKeys ?
            table.FindArrayOfTableOrValueAsDotted(arrayOfTableHeader) :
            table.FindArrayOfTableOrValue(arrayOfTableHeader);

        if (arrayOfTablesValue is CsTomlArray csTomlArrayOfTables)
        {
            if (csTomlArrayOfTables!.Count == 0 || csTomlArrayOfTables!.Count <= arrayIndex)
                return default;

            // find Value
            var t = (csTomlArrayOfTables[arrayIndex]! as CsTomlTable)!;
            var value = isDottedKeys ?
                t.FindArrayOfTableOrValueAsDotted(key) :
                t.FindArrayOfTableOrValue(key);
            return value.HasValue ? value : default;
        }

        return default;
    }

    public CsTomlValue? Find(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key, bool isTableHeaderAsDottedKeys = false)
    {
        // find Array Of Tables
        var arrayOfTablesValue = isTableHeaderAsDottedKeys ?
            table.FindArrayOfTableOrValueAsDotted(arrayOfTableHeader) :
            table.FindArrayOfTableOrValue(arrayOfTableHeader);

        if (arrayOfTablesValue is CsTomlArray csTomlArrayOfTables)
        {
            if (csTomlArrayOfTables!.Count == 0 || csTomlArrayOfTables!.Count <= arrayIndex)
                return default;

            // find Value
            var value = (csTomlArrayOfTables[arrayIndex]! as CsTomlTable)!.FindArrayOfTablesOrValue(key);
            return value.HasValue ? value : default;

        }

        return default;
    }

    public CsTomlValue? Find(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, bool isDottedKeys = false)
    {
        // find Array Of Tables
        var arrayOfTablesValue = table.FindArrayOfTablesOrValue(arrayOfTableHeader);
        if (arrayOfTablesValue is CsTomlArray csTomlArrayOfTables)
        {
            if (csTomlArrayOfTables!.Count == 0 || csTomlArrayOfTables!.Count <= arrayIndex)
                return default;

            // find Value
            var t = (csTomlArrayOfTables[arrayIndex]! as CsTomlTable)!;
            var value = isDottedKeys ?
                t.FindArrayOfTableOrValueAsDotted(key) :
                t.FindArrayOfTableOrValue(key);
            return value.HasValue ? value : default;
        }

        return default;
    }

    public CsTomlValue? Find(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key)
    {
        // find Array Of Tables
        var arrayOfTablesValue = table.FindArrayOfTablesOrValue(arrayOfTableHeader);
        if (arrayOfTablesValue is CsTomlArray csTomlArrayOfTables)
        {
            if (csTomlArrayOfTables!.Count == 0 || csTomlArrayOfTables!.Count <= arrayIndex)
                return default;

            // find Value
            var value = (csTomlArrayOfTables[arrayIndex]! as CsTomlTable)!.FindArrayOfTablesOrValue(key);
            return value.HasValue ? value : default;
        }

        return default;
    }

    public CsTomlValue? Find(ReadOnlySpan<char> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<char> key, bool isDottedKeys = false)
    {
        // buffer size to 3 times worst-case (UTF16 -> UTF8)
        var maxBufferSize = (arrayOfTableHeader.Length + key.Length + 1) * 3;
        if (maxBufferSize < 1024)
        {
            Span<byte> utf8Span = stackalloc byte[maxBufferSize];
            var status = Utf8.FromUtf16(arrayOfTableHeader, utf8Span, out int _, out int bytesWritten, replaceInvalidSequences: false);
            if (status != System.Buffers.OperationStatus.Done)
            {
                if (status == OperationStatus.InvalidData)
                    ExceptionHelper.ThrowInvalidByteIncluded();
                ExceptionHelper.ThrowBufferTooSmallFailed();
            }
            var status2 = Utf8.FromUtf16(key, utf8Span.Slice(bytesWritten), out int __, out int bytesWritten2, replaceInvalidSequences: false);
            if (status2 != System.Buffers.OperationStatus.Done)
            {
                if (status == OperationStatus.InvalidData)
                    ExceptionHelper.ThrowInvalidByteIncluded();
                ExceptionHelper.ThrowBufferTooSmallFailed();
            }

            return Find(
                utf8Span.Slice(0, bytesWritten),
                arrayIndex,
                utf8Span.Slice(bytesWritten, bytesWritten2),
                isDottedKeys);
        }
        else
        {
            var writer = new ArrayPoolBufferWriter<byte>(128);
            using var _ = writer;
            var arrayOfTableHeaderWriter = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref writer);
            var keyWriter = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref writer);
            try
            {
                ValueFormatter.Serialize(ref arrayOfTableHeaderWriter, arrayOfTableHeader);
                ValueFormatter.Serialize(ref keyWriter, key);
            }
            catch (CsTomlException)
            {
                return default;
            }
            return Find(
                writer.WrittenSpan.Slice(0, arrayOfTableHeaderWriter.WrittingCount),
                arrayIndex,
                writer.WrittenSpan.Slice(arrayOfTableHeaderWriter.WrittingCount, keyWriter.WrittingCount),
                isDottedKeys);
        }
    }

    public bool TryFind(ReadOnlySpan<byte> key, out CsTomlValue? value, bool isDottedKeys = false)
    {
        var findResult = Find(key, isDottedKeys);
        if (findResult?.HasValue ?? false)
        {
            value = findResult;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public bool TryFind(ReadOnlySpan<ByteArray> key, out CsTomlValue? value)
    {
        var findResult = Find(key);
        if (findResult?.HasValue ?? false)
        {
            value = findResult;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public bool TryFind(ReadOnlySpan<byte> tableHeaderKey, ReadOnlySpan<byte> key, out CsTomlValue? value, bool isTableHeaderAsDottedKeys = false, bool isDottedKeys = false)
    {
        value = Find(tableHeaderKey, key, isTableHeaderAsDottedKeys, isDottedKeys);
        return value?.HasValue ?? false;
    }

    public bool TryFind(ReadOnlySpan<byte> tableHeaderKey, ReadOnlySpan<ByteArray> key, out CsTomlValue? value, bool isTableHeaderAsDottedKeys = false)
    {
        var findResult = Find(tableHeaderKey, key, isTableHeaderAsDottedKeys);
        if (findResult?.HasValue ?? false)
        {
            value = findResult;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public bool TryFind(ReadOnlySpan<ByteArray> tableHeader, ReadOnlySpan<byte> key, out CsTomlValue? value, bool isDottedKeys = false)
    {
        var findResult = Find(tableHeader, key, isDottedKeys);
        if (findResult?.HasValue ?? false)
        {
            value = findResult;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public bool TryFind(ReadOnlySpan<ByteArray> tableHeader, ReadOnlySpan<ByteArray> key, out CsTomlValue? value)
    {
        var findResult = Find(tableHeader, key);
        if (findResult?.HasValue ?? false)
        {
            value = findResult;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public bool TryFind(ReadOnlySpan<char> key, out CsTomlValue? value, bool isDottedKeys = false)
    {
        // buffer size to 3 times worst-case (UTF16 -> UTF8)
        var maxBufferSize = (key.Length + 1) * 3;
        if (maxBufferSize < 1024)
        {
            Span<byte> utf8Span = stackalloc byte[maxBufferSize];
            var status = Utf8.FromUtf16(key, utf8Span, out int charsRead, out int bytesWritten, replaceInvalidSequences: false);
            if (status != System.Buffers.OperationStatus.Done)
            {
                if (status == OperationStatus.InvalidData)
                    ExceptionHelper.ThrowInvalidByteIncluded();
                ExceptionHelper.ThrowBufferTooSmallFailed();
            }

            return TryFind(utf8Span[..bytesWritten], out value, isDottedKeys);
        }
        else
        {
            var writer = new ArrayPoolBufferWriter<byte>(128);
            using var _ = writer;

            var utf8Writer = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref writer);
            try
            {
                ValueFormatter.Serialize(ref utf8Writer, key);
            }
            catch (CsTomlException)
            {
                value = default;
                return false;
            }

            return TryFind(writer.WrittenSpan, out value, isDottedKeys);
        }
    }

    public bool TryFind(ReadOnlySpan<char> tableHeader, ReadOnlySpan<char> key, out CsTomlValue? value, bool isTableHeaderAsDottedKeys = false, bool isDottedKeys = false)
    {
        // buffer size to 3 times worst-case (UTF16 -> UTF8)
        var maxBufferSize = (tableHeader.Length + key.Length + 1) * 3;
        if (maxBufferSize < 1024)
        {
            Span<byte> utf8Span = stackalloc byte[maxBufferSize];
            var status = Utf8.FromUtf16(tableHeader, utf8Span, out int _, out int bytesWritten, replaceInvalidSequences: false);
            if (status != System.Buffers.OperationStatus.Done)
            {
                if (status == OperationStatus.InvalidData)
                    ExceptionHelper.ThrowInvalidByteIncluded();
                ExceptionHelper.ThrowBufferTooSmallFailed();
            }
            var status2 = Utf8.FromUtf16(key, utf8Span.Slice(bytesWritten), out int __, out int bytesWritten2, replaceInvalidSequences: false);
            if (status2 != System.Buffers.OperationStatus.Done)
            {
                if (status == OperationStatus.InvalidData)
                    ExceptionHelper.ThrowInvalidByteIncluded();
                ExceptionHelper.ThrowBufferTooSmallFailed();
            }
            return TryFind(
                utf8Span.Slice(0, bytesWritten),
                utf8Span.Slice(bytesWritten, bytesWritten2),
                out value,
                isTableHeaderAsDottedKeys, isDottedKeys);
        }
        else
        {
            var writer = new ArrayPoolBufferWriter<byte>(128);
            using var _ = writer;

            var tableHeaderWriter = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref writer);
            var keyrWriter = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref writer);
            try
            {
                ValueFormatter.Serialize(ref tableHeaderWriter, tableHeader);
                ValueFormatter.Serialize(ref keyrWriter, key);
            }
            catch (CsTomlException)
            {
                value = default;
                return false;
            }
            return TryFind(
                    writer.WrittenSpan.Slice(0, tableHeaderWriter.WrittingCount),
                    writer.WrittenSpan.Slice(tableHeaderWriter.WrittingCount, keyrWriter.WrittingCount),
                    out value,
                    isTableHeaderAsDottedKeys, isDottedKeys);
        }
    }

    public bool TryFind(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, out CsTomlValue? value, bool isTableHeaderAsDottedKeys = false, bool isDottedKeys = false)
    {
        var findResult = Find(arrayOfTableHeader, arrayIndex, key, isTableHeaderAsDottedKeys, isDottedKeys);
        if (findResult?.HasValue ?? false)
        {
            value = findResult;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public bool TryFind(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key, out CsTomlValue? value, bool isTableHeaderAsDottedKeys = false)
    {
        var findResult = Find(arrayOfTableHeader, arrayIndex, key, isTableHeaderAsDottedKeys);
        if (findResult?.HasValue ?? false)
        {
            value = findResult;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public bool TryFind(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, out CsTomlValue? value, bool isDottedKeys = false)
    {
        var findResult = Find(arrayOfTableHeader, arrayIndex, key, isDottedKeys);
        if (findResult?.HasValue ?? false)
        {
            value = findResult;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public bool TryFind(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key, out CsTomlValue? value)
    {
        var findResult = Find(arrayOfTableHeader, arrayIndex, key);
        if (findResult?.HasValue ?? false)
        {
            value = findResult;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }
}

