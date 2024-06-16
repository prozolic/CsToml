
using System.Runtime.CompilerServices;

namespace CsToml.Values;

internal partial class CsTomlBool
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool CanGetValue(CsTomlValueFeature feature) 
        => ((CsTomlValueFeature.String | CsTomlValueFeature.Int64 |  CsTomlValueFeature.Double | CsTomlValueFeature.Bool | CsTomlValueFeature.Number) & feature) == feature;

    public override string GetString() => Value ? bool.TrueString : bool.FalseString;

    public override long GetInt64() => Value ? 1 : 0;

    public override double GetDouble() => Value ? 1d : 0d;

    public override bool GetBool() => Value;

}

