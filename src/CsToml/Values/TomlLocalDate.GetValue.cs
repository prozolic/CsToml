
using CsToml.Extension;

namespace CsToml.Values;

internal partial class TomlLocalDate
{
    public override bool CanGetValue(TomlValueFeature feature)
        => ((TomlValueFeature.String | TomlValueFeature.DateTime | TomlValueFeature.DateTimeOffset | TomlValueFeature.DateOnly | TomlValueFeature.Object) & feature) == feature;

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

