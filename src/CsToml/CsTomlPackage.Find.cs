using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Text.Unicode;

namespace CsToml;

public partial class CsTomlPackage
{
    public bool TryGetValue(ReadOnlySpan<byte> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
    {
        var findResult = Find(key, options);
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

    public bool TryGetValue(ReadOnlySpan<ByteArray> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
    {
        var findResult = Find(key, options);
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

    public bool TryGetValue(ReadOnlySpan<byte> tableHeaderKey, ReadOnlySpan<byte> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
    {
        var findResult = Find(tableHeaderKey, key, options);
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

    public bool TryGetValue(ReadOnlySpan<ByteArray> tableHeader, ReadOnlySpan<byte> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
    {
        var findResult = Find(tableHeader, key, options);
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

    public bool TryGetValue(ReadOnlySpan<char> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
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

            return TryGetValue(utf8Span[..bytesWritten], out value, options);
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

            return TryGetValue(writer.WrittenSpan, out value, options);
        }
    }

    public bool TryGetValue(ReadOnlySpan<char> tableHeader, ReadOnlySpan<char> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
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
            return TryGetValue(
                utf8Span.Slice(0, bytesWritten),
                utf8Span.Slice(bytesWritten, bytesWritten2),
                out value,
                options);
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
            return TryGetValue(
                    writer.WrittenSpan.Slice(0, tableHeaderWriter.WrittingCount),
                    writer.WrittenSpan.Slice(tableHeaderWriter.WrittingCount, keyrWriter.WrittingCount),
                    out value,
                    options);
        }
    }

    public bool TryGetValue(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        var findResult = Find(arrayOfTableHeader, arrayIndex, key, options);
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

    public bool TryGetValue(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        var findResult = Find(arrayOfTableHeader, arrayIndex, key, options);
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

    public bool TryGetValue(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        var findResult = Find(arrayOfTableHeader, arrayIndex, key, options);
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

    public bool TryGetValue(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        var findResult = Find(arrayOfTableHeader, arrayIndex, key, options);
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

    public CsTomlValue? Find(ReadOnlySpan<byte> keys, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;
        
        var value = options.IsDottedKeys ? table.FindAsDottedKey(keys) : table.FindAsKey(keys);
        return value.HasValue ? value : default;
    }

    public CsTomlValue? Find(ReadOnlySpan<char> keys, CsTomlPackageOptions? options = default)
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

            return Find(utf8Span[..bytesWritten], options);
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

            return Find(writer.WrittenSpan, options);
        }
    }

    public CsTomlValue? Find(ReadOnlySpan<ByteArray> keys, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        var value = table.FindAsDottedKey(keys);
        return value.HasValue ? value : default;
    }

    public CsTomlValue? Find(ReadOnlySpan<byte> tableHeader, ReadOnlySpan<byte> key, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        if (options.IsDottedKeys)
        {
            if (table.TryGetSubTable(tableHeader, key, out var value))
            {
                return value!;
            }
        }
        else
        {
            if (table.TryGetTable(tableHeader, key, out var value))
            {
                return value!;
            }
        }

        return default;
    }

    public CsTomlValue? Find(ReadOnlySpan<char> tableHeader, ReadOnlySpan<char> key, CsTomlPackageOptions? options = default)
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
                options);
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
                options);
        }
    }

    public CsTomlValue? Find(ReadOnlySpan<ByteArray> tableHeader, ReadOnlySpan<byte> key, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        if (table.TryGetSubTable(tableHeader, key, out var value))
        {
            return value!;
        }

        return default;
    }

    public CsTomlValue? Find(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        // find Array Of Tables
        var arrayOfTablesValue = options.IsDottedKeys ?
            table.FindArrayOfTableOrValueAsDotted(arrayOfTableHeader) :
            table.FindArrayOfTableOrValue(arrayOfTableHeader);

        if (arrayOfTablesValue is CsTomlArray csTomlArrayOfTables)
        {
            if (csTomlArrayOfTables!.Count == 0 || csTomlArrayOfTables!.Count <= arrayIndex)
                return default;

            // find Value
            var t = (csTomlArrayOfTables[arrayIndex]! as CsTomlTable)!;
            var value = options.IsDottedKeys ?
                t.FindArrayOfTableOrValueAsDotted(key) :
                t.FindArrayOfTableOrValue(key);
            return value.HasValue ? value : default;
        }

        return default;
    }

    public CsTomlValue? Find(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        // find Array Of Tables
        var arrayOfTablesValue = options.IsDottedKeys ?
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

    public CsTomlValue? Find(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        // find Array Of Tables
        var arrayOfTablesValue = table.FindArrayOfTablesOrValue(arrayOfTableHeader);
        if (arrayOfTablesValue is CsTomlArray csTomlArrayOfTables)
        {
            if (csTomlArrayOfTables!.Count == 0 || csTomlArrayOfTables!.Count <= arrayIndex)
                return default;

            // find Value
            var t = (csTomlArrayOfTables[arrayIndex]! as CsTomlTable)!;
            var value = options.IsDottedKeys ?
                t.FindArrayOfTableOrValueAsDotted(key) :
                t.FindArrayOfTableOrValue(key);
            return value.HasValue ? value : default;
        }

        return default;
    }

    public CsTomlValue? Find(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

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

    public CsTomlValue? Find(ReadOnlySpan<char> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<char> key, CsTomlPackageOptions? options = default)
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
                options);
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
                options);
        }
    }

}

