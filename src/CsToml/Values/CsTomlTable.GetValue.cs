
namespace CsToml.Values;

internal partial class CsTomlTable 
{
    public override bool CanGetValue(CsTomlValueFeature feature)
        => (CsTomlValueFeature.Table & feature) == feature;

    public override CsTomlValue? Find(ReadOnlySpan<byte> keys, bool isDotKey = false)
    {
        var value = isDotKey ? FindAsDottedKey(keys) : FindAsKey(keys);
        return value.HasValue ? value : base.Find(keys, isDotKey);
    }
}
