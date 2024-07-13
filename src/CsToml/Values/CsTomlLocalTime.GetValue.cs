
namespace CsToml.Values;

internal partial class CsTomlLocalTime
{
    public override bool CanGetValue(CsTomlValueFeature feature)
        => ((CsTomlValueFeature.String | CsTomlValueFeature.TimeOnly) & feature) == feature;

    public override string GetString()
    {
        Span<char> destination = stackalloc char[32];
        TryFormat(destination, out var charsWritten, null, null);
        return destination[..charsWritten].ToString();
    }

    public override TimeOnly GetTimeOnly() => Value;

}
