
namespace CsToml.Values;

internal partial class TomlInteger
{
    public override bool CanGetValue(TomlValueFeature feature)
        => ((TomlValueFeature.String | TomlValueFeature.Int64 | TomlValueFeature.Double | TomlValueFeature.Bool | TomlValueFeature.Number | TomlValueFeature.Object) & feature) == feature;

    public override string GetString() => ToString();

    public override long GetInt64() => Value;

    public override double GetDouble() => Value;

    public override bool GetBool() => Value != 0;

    public override object GetObject() => Value;
}

