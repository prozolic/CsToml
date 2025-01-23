
namespace CsToml.Values;

internal partial class TomlLocalDateTime
{
    public override bool CanGetValue(TomlValueFeature feature)
        => ((TomlValueFeature.String | TomlValueFeature.DateTime | TomlValueFeature.DateTimeOffset | TomlValueFeature.DateOnly | TomlValueFeature.TimeOnly | TomlValueFeature.Object) & feature) == feature;

    public override string GetString() => ToString();

    public override DateTime GetDateTime() => Value;

    public override DateTimeOffset GetDateTimeOffset() => Value;

    public override DateOnly GetDateOnly() => DateOnly.FromDateTime(Value);

    public override TimeOnly GetTimeOnly() => TimeOnly.FromDateTime(Value);

    public override object GetObject() => Value;
}

