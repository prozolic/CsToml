
namespace CsToml.Values;

internal partial class CsTomlBool
{
    public override bool CanGetValue(CsTomlValueFeature feature) 
        => ((CsTomlValueFeature.String | CsTomlValueFeature.Int64 |  CsTomlValueFeature.Double | CsTomlValueFeature.Bool | CsTomlValueFeature.Number | CsTomlValueFeature.Object) & feature) == feature;

    public override string GetString() => Value ? bool.TrueString : bool.FalseString;

    public override long GetInt64() => Value ? 1 : 0;

    public override double GetDouble() => Value ? 1d : 0d;

    public override bool GetBool() => Value;

    public override object GetObject() => Value;
}

