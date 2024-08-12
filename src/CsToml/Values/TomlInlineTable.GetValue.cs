

namespace CsToml.Values;

internal partial class TomlInlineTable
{
    public override bool CanGetValue(TomlValueFeature feature)
        => ((TomlValueFeature.String | TomlValueFeature.InlineTable) & feature) == feature;

    public override string GetString()
        => ToString();

    public override TomlValue? Find(ReadOnlySpan<byte> keys, bool isDotKey = false)
        => inlineTable.Find(keys, isDotKey);

    public override TomlValue? Find(ReadOnlySpan<ByteArray> dottedKeys)
        => inlineTable.Find(dottedKeys);
}

