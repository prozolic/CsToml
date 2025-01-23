
namespace CsToml.Values;

internal partial class TomlLocalTime
{
    public override bool CanGetValue(TomlValueFeature feature)
        => ((TomlValueFeature.String | TomlValueFeature.TimeOnly | TomlValueFeature.Object) & feature) == feature;

    public override string GetString() => ToString();

    public override TimeOnly GetTimeOnly() => Value;

    public override object GetObject() => Value;
}
