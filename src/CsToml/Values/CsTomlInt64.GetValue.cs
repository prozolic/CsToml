
using System.Runtime.CompilerServices;

namespace CsToml.Values;

internal partial class CsTomlInt64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool CanGetValue(CsTomlValueFeature feature)
        => ((CsTomlValueFeature.String | CsTomlValueFeature.Int64 | CsTomlValueFeature.Double | CsTomlValueFeature.Bool | CsTomlValueFeature.Number) & feature) == feature;

    public override string GetString()
    {
        Span<char> destination = stackalloc char[32];
        TryFormat(destination, out var charsWritten, null, null);
        return destination[..charsWritten].ToString();
    }

    public override long GetInt64() => Value;

    public override double GetDouble() => Value;

    public override bool GetBool() => Value != 0;
}

