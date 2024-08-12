
namespace CsToml.Values;

internal partial class TomlBoolean
{
    public override bool CanGetValue(TomlValueFeature feature) 
        => ((TomlValueFeature.String | TomlValueFeature.Int64 |  TomlValueFeature.Double | TomlValueFeature.Bool | TomlValueFeature.Number | TomlValueFeature.Object) & feature) == feature;

    public override string GetString() => Value ? bool.TrueString : bool.FalseString;

    public override long GetInt64() => Value ? 1 : 0;

    public override double GetDouble() => Value ? 1d : 0d;

    public override bool GetBool() => Value;

    public override object GetObject() => Value;
}

