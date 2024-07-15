
namespace CsToml.Values;

internal partial class CsTomlFloat
{
    public override bool CanGetValue(CsTomlValueFeature feature)
        => ((CsTomlValueFeature.String | CsTomlValueFeature.Int64 | CsTomlValueFeature.Double | CsTomlValueFeature.Bool | CsTomlValueFeature.Number | CsTomlValueFeature.Object) & feature) == feature;

    public override string GetString()
    {
        Span<char> destination = stackalloc char[32];
        TryFormat(destination, out var charsWritten, null, null);
        return destination[..charsWritten].ToString();
    }

    public override long GetInt64() => (long)Value;

    public override double GetDouble() => Value;

    public override bool GetBool() => Value != 0d;

    public override object GetObject() => Value;
}

