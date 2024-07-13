
namespace CsToml.Values;

internal partial class CsTomlLocalDateTime
{
    public override bool CanGetValue(CsTomlValueFeature feature)
        => ((CsTomlValueFeature.String | CsTomlValueFeature.DateTime | CsTomlValueFeature.DateTimeOffset | CsTomlValueFeature.DateOnly | CsTomlValueFeature.TimeOnly) & feature) == feature;

    public override string GetString()
    {
        Span<char> destination = stackalloc char[32];
        TryFormat(destination, out var charsWritten, null, null);
        return destination[..charsWritten].ToString();
    }

    public override DateTime GetDateTime() => Value;

    public override DateTimeOffset GetDateTimeOffset() => Value;

    public override DateOnly GetDateOnly() => DateOnly.FromDateTime(Value);

    public override TimeOnly GetTimeOnly() => TimeOnly.FromDateTime(Value);

}

