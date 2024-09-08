
using CsToml.Error;

namespace CsToml.Values;

internal partial class TomlTable 
{
    public override bool CanGetValue(TomlValueFeature feature)
        => ((TomlValueFeature.String | TomlValueFeature.Table | TomlValueFeature.Object) & feature) == feature;

    public override string GetString()
        => ToString();

    public override object GetObject()
        => GetDictionary();
}
