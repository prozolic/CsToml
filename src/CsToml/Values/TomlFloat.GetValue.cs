
namespace CsToml.Values;

internal partial class TomlFloat
{
    public override bool CanGetValue(TomlValueFeature feature)
        => ((TomlValueFeature.String | TomlValueFeature.Int64 | TomlValueFeature.Double | TomlValueFeature.Boolean | TomlValueFeature.Number | TomlValueFeature.Object) & feature) == feature;

    public override string GetString() => ToString();

    public override long GetInt64() => (long)Value;

    public override double GetDouble() => Value;

    public override bool GetBool() => Value != 0d;

    public override object GetObject() => Value;
}

