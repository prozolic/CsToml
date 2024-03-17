using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;
using CsToml.Values;

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
        using var writer = new ArrayPoolBufferWriter<byte>(128);
        var utf8Writer = new Utf8Writer(writer);
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

    public bool TryGetValue(ReadOnlySpan<char> tableHeader, ReadOnlySpan<char> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
    {
        using var writer = new ArrayPoolBufferWriter<byte>(128);
        var tableHeaderWriter = new Utf8Writer(writer);
        var keyrWriter = new Utf8Writer(writer);
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
        using var writer = new ArrayPoolBufferWriter<byte>(128);
        var utf8Writer = new Utf8Writer(writer);
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
        using var writer = new ArrayPoolBufferWriter<byte>(128);
        var tableHeaderWriter = new Utf8Writer(writer);
        var keyrWriter = new Utf8Writer(writer);
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
        if (arrayOfTablesValue.Type != CsTomlType.Array)
            return default;

        var csTomlArrayOfTables = arrayOfTablesValue as CsTomlArray;
        if (csTomlArrayOfTables!.Count == 0 || csTomlArrayOfTables!.Count <= arrayIndex)
            return default;

        // find Value
        var t = (csTomlArrayOfTables[arrayIndex]! as CsTomlTable)!;
        var value = options.IsDottedKeys ?
            t.FindArrayOfTableOrValueAsDotted(key) :
            t.FindArrayOfTableOrValue(key);
        return value.HasValue ? value : default;
    }

    public CsTomlValue? Find(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        // find Array Of Tables
        var arrayOfTablesValue = options.IsDottedKeys ?
            table.FindArrayOfTableOrValueAsDotted(arrayOfTableHeader) :
            table.FindArrayOfTableOrValue(arrayOfTableHeader);
        if (arrayOfTablesValue.Type != CsTomlType.Array)
            return default;

        var csTomlArrayOfTables = arrayOfTablesValue as CsTomlArray;
        if (csTomlArrayOfTables!.Count == 0 || csTomlArrayOfTables!.Count <= arrayIndex)
            return default;

        // find Value
        var value = (csTomlArrayOfTables[arrayIndex]! as CsTomlTable)!.FindArrayOfTablesOrValue(key);
        return value.HasValue ? value : default;
    }

    public CsTomlValue? Find(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        // find Array Of Tables
        var arrayOfTablesValue = table.FindArrayOfTablesOrValue(arrayOfTableHeader);
        if (arrayOfTablesValue.Type != CsTomlType.Array)
            return default;

        var csTomlArrayOfTables = arrayOfTablesValue as CsTomlArray;
        if (csTomlArrayOfTables!.Count == 0 || csTomlArrayOfTables!.Count <= arrayIndex)
            return default;

        // find Value
        var t = (csTomlArrayOfTables[arrayIndex]! as CsTomlTable)!;
        var value = options.IsDottedKeys ?
            t.FindArrayOfTableOrValueAsDotted(key) :
            t.FindArrayOfTableOrValue(key);
        return value.HasValue ? value : default;
    }

    public CsTomlValue? Find(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        // find Array Of Tables
        var arrayOfTablesValue = table.FindArrayOfTablesOrValue(arrayOfTableHeader);
        if (arrayOfTablesValue.Type != CsTomlType.Array)
            return default;

        var csTomlArrayOfTables = arrayOfTablesValue as CsTomlArray;
        if (csTomlArrayOfTables!.Count == 0 || csTomlArrayOfTables!.Count <= arrayIndex)
            return default;

        // find Value
        var value = (csTomlArrayOfTables[arrayIndex]! as CsTomlTable)!.FindArrayOfTablesOrValue(key);
        return value.HasValue ? value : default;
    }

    public CsTomlValue? Find(ReadOnlySpan<char> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<char> key, CsTomlPackageOptions? options = default)
    {
        using var writer = new ArrayPoolBufferWriter<byte>(128);
        var arrayOfTableHeaderWriter = new Utf8Writer(writer);
        var keyWriter = new Utf8Writer(writer);
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

