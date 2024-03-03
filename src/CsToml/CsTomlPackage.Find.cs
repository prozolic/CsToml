using CsToml.Error;
using CsToml.Extension;
using CsToml.Formatter;
using CsToml.Utility;
using CsToml.Values;
using static CsToml.Extension.StringExtensions;

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
            StringFormatter.Serialize(ref utf8Writer, key);
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
            StringFormatter.Serialize(ref tableHeaderWriter, tableHeader);
            StringFormatter.Serialize(ref keyrWriter, key);
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
        
        var value = options.IsDottedKeys ? FindAsDottedKey(keys) : FindAsKey(keys);
        return value.HasValue ? value : default;
    }

    public CsTomlValue? Find(ReadOnlySpan<char> keys, CsTomlPackageOptions? options = default)
    {
        using var writer = new ArrayPoolBufferWriter<byte>(128);
        var utf8Writer = new Utf8Writer(writer);
        try
        {
            StringFormatter.Serialize(ref utf8Writer, keys);
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

        var value = FindAsDottedKey(keys);
        return value.HasValue ? value : default;
    }

    public CsTomlValue? Find(ReadOnlySpan<byte> tableHeader, ReadOnlySpan<byte> key, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        if (options.IsDottedKeys)
        {
            if (TryGetSubTable(tableHeader, out var node) && node!.TryGetChildNode(key, out var valueNode))
            {
                var value = valueNode!.Value;
                return value!.HasValue ? value : default;
            }
        }
        else
        {
            if (TryGetTable(table.RootNode, tableHeader, out var node) && node!.TryGetChildNode(key, out var valueNode))
            {
                var value = valueNode!.Value;
                return value!.HasValue ? value : default;
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
            StringFormatter.Serialize(ref tableHeaderWriter, tableHeader);
            StringFormatter.Serialize(ref keyrWriter, key);
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

        if (TryGetSubTable(tableHeader, out var node) && node!.TryGetChildNode(key, out var valueNode))
        {
            var value = valueNode!.Value;
            return value!.HasValue ? value : default;
        }

        return default;
    }

    public CsTomlValue? Find(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        // find Array Of Tables
        var arrayOfTablesValue = options.IsDottedKeys ? 
            FindArrayOfTableOrValueAsDotted(table, arrayOfTableHeader) : 
            FindArrayOfTableOrValue(table, arrayOfTableHeader);
        if (arrayOfTablesValue.Type != CsTomlType.Array)
            return default;

        var csTomlArrayOfTables = arrayOfTablesValue as CsTomlArray;
        if (csTomlArrayOfTables!.Count == 0 || csTomlArrayOfTables!.Count <= arrayIndex)
            return default;

        // find Value
        var t = (csTomlArrayOfTables[arrayIndex]! as CsTomlTable)!;
        var value = options.IsDottedKeys ?
            FindArrayOfTableOrValueAsDotted(t, key) :
            FindArrayOfTableOrValue(t, key);
        return value.HasValue ? value : default;
    }

    public CsTomlValue? Find(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        // find Array Of Tables
        var arrayOfTablesValue = options.IsDottedKeys ?
            FindArrayOfTableOrValueAsDotted(table, arrayOfTableHeader) :
            FindArrayOfTableOrValue(table, arrayOfTableHeader);
        if (arrayOfTablesValue.Type != CsTomlType.Array)
            return default;

        var csTomlArrayOfTables = arrayOfTablesValue as CsTomlArray;
        if (csTomlArrayOfTables!.Count == 0 || csTomlArrayOfTables!.Count <= arrayIndex)
            return default;

        // find Value
        var value = FindArrayOfTablesOrValue((csTomlArrayOfTables[arrayIndex]! as CsTomlTable)!, key);
        return value.HasValue ? value : default;
    }

    public CsTomlValue? Find(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        // find Array Of Tables
        var arrayOfTablesValue = FindArrayOfTablesOrValue(table, arrayOfTableHeader);
        if (arrayOfTablesValue.Type != CsTomlType.Array)
            return default;

        var csTomlArrayOfTables = arrayOfTablesValue as CsTomlArray;
        if (csTomlArrayOfTables!.Count == 0 || csTomlArrayOfTables!.Count <= arrayIndex)
            return default;

        // find Value
        var t = (csTomlArrayOfTables[arrayIndex]! as CsTomlTable)!;
        var value = options.IsDottedKeys ?
            FindArrayOfTableOrValueAsDotted(t, key) :
            FindArrayOfTableOrValue(t, key);
        return value.HasValue ? value : default;
    }

    public CsTomlValue? Find(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key, CsTomlPackageOptions? options = default)
    {
        options ??= CsTomlPackageOptions.Default;

        // find Array Of Tables
        var arrayOfTablesValue = FindArrayOfTablesOrValue(table, arrayOfTableHeader);
        if (arrayOfTablesValue.Type != CsTomlType.Array)
            return default;

        var csTomlArrayOfTables = arrayOfTablesValue as CsTomlArray;
        if (csTomlArrayOfTables!.Count == 0 || csTomlArrayOfTables!.Count <= arrayIndex)
            return default;

        // find Value
        var value = FindArrayOfTablesOrValue((csTomlArrayOfTables[arrayIndex]! as CsTomlTable)!, key);
        return value.HasValue ? value : default;
    }

    public CsTomlValue? Find(ReadOnlySpan<char> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<char> key, CsTomlPackageOptions? options = default)
    {
        using var writer = new ArrayPoolBufferWriter<byte>(128);
        var arrayOfTableHeaderWriter = new Utf8Writer(writer);
        var keyrWriter = new Utf8Writer(writer);
        try
        {
            StringFormatter.Serialize(ref arrayOfTableHeaderWriter, arrayOfTableHeader);
            StringFormatter.Serialize(ref keyrWriter, key);
        }
        catch (CsTomlException)
        {
            return default;
        }
        return Find(
            writer.WrittenSpan.Slice(0, arrayOfTableHeaderWriter.WrittingCount),
            arrayIndex,
            writer.WrittenSpan.Slice(arrayOfTableHeaderWriter.WrittingCount, keyrWriter.WrittingCount),
            options);
    }

    private bool TryGetTable(CsTomlTableNode rootNode, ReadOnlySpan<byte> tableHeader, out CsTomlTableNode? node)
    {
        var currentNode = rootNode;
        if (currentNode!.TryGetChildNode(tableHeader, out var childNode) && childNode!.IsGroupingProperty)
        {
            node = childNode;
            return true;
        }
        node = default;
        return false;
    }

    private bool TryGetSubTable(ReadOnlySpan<byte> tableHeader, out CsTomlTableNode? node)
    {
        var hit = false;
        CsTomlTableNode? currentNode = table.RootNode;
        foreach (var dottedKey in tableHeader.SplitSpan("."u8))
        {
            if (TryGetTable(currentNode!, dottedKey.Value, out currentNode))
            {
                hit = true;
            }
            else
            {
                node = default;
                return false;
            }
        }

        if (hit && currentNode!.IsGroupingProperty)
        {
            node = currentNode;
            return true;
        }

        node = default;
        return false;
    }

    private bool TryGetSubTable(ReadOnlySpan<ByteArray> tableHeaderSpan, out CsTomlTableNode? node)
    {
        var hit = false;
        CsTomlTableNode? currentNode = table.RootNode;

        for (int i = 0; i < tableHeaderSpan.Length; i++)
        {
            if (TryGetTable(currentNode!, tableHeaderSpan[i].value, out currentNode))
            {
                hit = true;
            }
            else
            {
                node = default;
                return false;
            }
        }

        if (hit && currentNode!.IsGroupingProperty)
        {
            node = currentNode;
            return true;
        }

        node = default;
        return false;
    }

    private CsTomlValue FindAsDottedKey(ReadOnlySpan<byte> dottedKeySpan)
    {
        var hit = false;
        var currentNode = table.RootNode;
        foreach (var dottedKey in dottedKeySpan.SplitSpan("."u8))
        {
            if (currentNode!.TryGetChildNode(dottedKey.Value, out var childNode))
            {
                hit = true;
                currentNode = childNode;
            }
        }

        if (hit && !currentNode!.IsGroupingProperty)
        {
            return currentNode.Value!;
        }

        return CsTomlValue.Empty;
    }

    private CsTomlValue FindAsDottedKey(ReadOnlySpan<ByteArray> dottedKeys)
    {
        var hit = false;
        var currentNode = table.RootNode;

        for (int i = 0; i < dottedKeys.Length; i++)
        {
            if (currentNode!.TryGetChildNode(dottedKeys[i].value, out var childNode))
            {
                hit = true;
                currentNode = childNode;
            }
        }

        if (hit && !currentNode!.IsGroupingProperty)
        {
            return currentNode.Value!;
        }

        return CsTomlValue.Empty;
    }

    private CsTomlValue FindAsKey(ReadOnlySpan<byte> keySpan)
    {
        var currentNode = table.RootNode;
        if (currentNode!.TryGetChildNode(keySpan, out var childNode) && !childNode!.IsGroupingProperty)
        {
            return childNode!.Value!;
        }
        return CsTomlValue.Empty;
    }

    private CsTomlValue FindArrayOfTableOrValueAsDotted(CsTomlTable innerTable, ReadOnlySpan<byte> keys)
    {
        var hit = false;
        var currentNode = innerTable.RootNode;
        foreach (var key in keys.SplitSpan("."u8))
        {
            if (currentNode!.TryGetChildNode(key.Value, out var childNode))
            {
                hit = true;
                currentNode = childNode;
            }
        }

        if (hit && (!currentNode!.IsGroupingProperty || currentNode!.IsArrayOfTablesHeader))
        {
            return currentNode.Value!;
        }

        return CsTomlValue.Empty;
    }

    private CsTomlValue FindArrayOfTableOrValue(CsTomlTable innerTable, ReadOnlySpan<byte> keySpan)
    {
        var currentNode = innerTable.RootNode;
        if (currentNode!.TryGetChildNode(keySpan, out var childNode) && 
            (!childNode!.IsGroupingProperty || childNode!.IsArrayOfTablesHeader))
        {
            return childNode!.Value!;
        }
        return CsTomlValue.Empty;
    }

    private CsTomlValue FindArrayOfTablesOrValue(CsTomlTable innerTable, ReadOnlySpan<ByteArray> keys)
    {
        var hit = false;
        var currentNode = innerTable.RootNode;
        foreach (var key in keys)
        {
            if (currentNode!.TryGetChildNode(key.value, out var childNode))
            {
                hit = true;
                currentNode = childNode;
            }
        }

        if (hit && (!currentNode!.IsGroupingProperty || currentNode!.IsArrayOfTablesHeader))
        {
            return currentNode.Value!;
        }

        return CsTomlValue.Empty;
    }
}

public readonly struct ByteArray
{
    public readonly byte[] value;

    public ByteArray(ReadOnlySpan<byte> byteSpan)
    {
        value = byteSpan.ToArray();
    }

    public ByteArray(byte[] byteSpan)
    {
        value = byteSpan;
    }

    public static implicit operator ByteArray(ReadOnlySpan<byte> bytes)
        => new(bytes);

    public static implicit operator ByteArray(byte[] bytes)
        => new(bytes);
}

