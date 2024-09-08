

namespace CsToml.Values;

internal partial class TomlInlineTable
{
    public override bool CanGetValue(TomlValueFeature feature)
        => ((TomlValueFeature.String | TomlValueFeature.InlineTable | TomlValueFeature.Object) & feature) == feature;

    public override string GetString()
        => ToString();

    public override object GetObject()
        => GetDictionary();

}

