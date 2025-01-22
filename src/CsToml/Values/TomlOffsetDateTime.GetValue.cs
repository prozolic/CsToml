
namespace CsToml.Values;

internal partial class TomlOffsetDateTime
{
    public override bool CanGetValue(TomlValueFeature feature)
        => ((TomlValueFeature.String | TomlValueFeature.DateTime | TomlValueFeature.DateTimeOffset | TomlValueFeature.DateOnly | TomlValueFeature.TimeOnly | TomlValueFeature.Object) & feature) == feature;

    public override string GetString() => ToString();

    public override DateTime GetDateTime() => Value.DateTime;

    public override DateTimeOffset GetDateTimeOffset() => Value;

    public override DateOnly GetDateOnly() => DateOnly.FromDateTime(Value.DateTime);

    public override TimeOnly GetTimeOnly() => TimeOnly.FromDateTime(Value.DateTime);

    public override object GetObject() => Value;
}

