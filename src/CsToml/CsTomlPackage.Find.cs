
using CsToml.Error;
using CsToml.Extension;
using CsToml.Formatter;
using CsToml.Utility;
using CsToml.Values;

namespace CsToml;

public partial class CsTomlPackage
{
    public bool TryGetValue(ReadOnlySpan<byte> key, out CsTomlValue? value)
        => TryGetValueCore(key, out value);

    public bool TryGetValue(string key, out CsTomlValue? value)
    {
        using var writer = new ArrayPoolBufferWriter<byte>(128);
        var utf8Writer = new Utf8Writer(writer);
        try
        {
            StringFormatter.Serialize(ref utf8Writer, key);
        }
        catch (CsTomlException)
        {
            // TODO;
            value = null;
            return false;
        }
        return TryGetValueCore(writer.WrittenSpan, out value);
    }

    private bool TryGetValueCore(ReadOnlySpan<byte> key, out CsTomlValue? value)
    {
        var hit = false;
        var currentNode = table.RootNode;
        foreach (var separateKeyRange in key.SplitSpan("."u8))
        {
            var separateKey = separateKeyRange.Value;
            if (currentNode!.TryGetChildNode(separateKey, out var childNode))
            {
                hit = true;
                currentNode = childNode;
            }
        }

        if (hit && (!currentNode!.IsGroupingProperty || currentNode!.IsArrayOfTablesHeader))
        {
            value = currentNode.Value!;
            return true;
        }

        value = null;
        return false;
    }

    public CsTomlValue Find(ByteArray[] keys)
        => FindCore(table, keys);

    public CsTomlValue Find(ByteArray[] tableHeaderKeys, int arrayIndex, ByteArray[] keys)
    {
        // find Array Of Tables
        var value = Find(tableHeaderKeys);
        if (value.Type != CsTomlType.Array) 
            return CsTomlValue.Empty;

        var csTomlArrayOfTables = value as CsTomlArray;
        if (csTomlArrayOfTables!.Count == 0 || csTomlArrayOfTables!.Count <= arrayIndex) 
            return CsTomlValue.Empty;

        // find Value
        return FindCore((csTomlArrayOfTables[arrayIndex]! as CsTomlTable)!, keys);
    }

    private CsTomlValue FindCore(CsTomlTable innerTable, ByteArray[] keys)
    {
        var hit = false;
        var currentNode = innerTable.RootNode;
        foreach (var key in keys)
        {
            var separateKey = key.value;
            if (currentNode!.TryGetChildNode(separateKey, out var childNode))
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

