
namespace CsToml.Values;

internal partial class CsTomlTable 
{
    public override bool CanGetValue(CsTomlValueFeature feature)
        => ((CsTomlValueFeature.String | CsTomlValueFeature.Table) & feature) == feature;

    public override string GetString()
        => ToString();

    public override CsTomlValue? Find(ReadOnlySpan<byte> keys, bool isDotKey = false)
    {
        var value = isDotKey ? FindAsDottedKey(keys) : FindAsKey(keys);
        return value.HasValue ? value : base.Find(keys, isDotKey);
    }

    public override CsTomlValue? Find(ReadOnlySpan<ByteArray> dottedKeys)
    {
        var value = FindNode(RootNode, dottedKeys).Value!;
        return value.HasValue ? value : base.Find(dottedKeys);
    }

}
