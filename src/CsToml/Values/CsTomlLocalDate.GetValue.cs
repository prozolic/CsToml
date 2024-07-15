
using CsToml.Extension;

namespace CsToml.Values;

internal partial class CsTomlLocalDate
{
    public override bool CanGetValue(CsTomlValueFeature feature)
        => ((CsTomlValueFeature.String | CsTomlValueFeature.DateTime | CsTomlValueFeature.DateTimeOffset | CsTomlValueFeature.DateOnly | CsTomlValueFeature.Object) & feature) == feature;

    public override string GetString()
    {
        Span<char> destination = stackalloc char[32];
        TryFormat(destination, out var charsWritten, null, null);
        return destination[..charsWritten].ToString();
    }

    public override DateTime GetDateTime() => Value.ToLocalDateTime();

    public override DateTimeOffset GetDateTimeOffset() => Value.ToLocalDateTime();

    public override DateOnly GetDateOnly() => Value;

    public override object GetObject() => Value;
}

