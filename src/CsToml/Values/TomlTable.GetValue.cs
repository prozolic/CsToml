
namespace CsToml.Values;

internal partial class TomlTable 
{
    public override bool CanGetValue(TomlValueFeature feature)
        => ((TomlValueFeature.String | TomlValueFeature.Table) & feature) == feature;

    public override string GetString()
        => ToString();

    public override TomlValue? Find(ReadOnlySpan<byte> keys, bool isDotKey = false)
    {
        var value = isDotKey ? FindAsDottedKey(keys) : FindAsKey(keys);
        return value.HasValue ? value : base.Find(keys, isDotKey);
    }

    public override TomlValue? Find(ReadOnlySpan<ByteArray> dottedKeys)
    {
        var value = FindNode(RootNode, dottedKeys).Value!;
        return value.HasValue ? value : base.Find(dottedKeys);
    }

}
