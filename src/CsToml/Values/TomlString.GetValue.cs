
using System.Reflection;

namespace CsToml.Values;

internal partial class TomlString
{
    public override bool CanGetValue(TomlValueFeature feature)
        => ((TomlValueFeature.String | TomlValueFeature.Int64 | TomlValueFeature.Double | TomlValueFeature.Bool | 
            TomlValueFeature.DateTime | TomlValueFeature.DateTimeOffset | TomlValueFeature.DateOnly | TomlValueFeature.TimeOnly | TomlValueFeature.Object) & feature) == feature;

    public override string GetString() => Utf16String;

    public override long GetInt64()
    {
        if (long.TryParse(Utf16String.AsSpan(), out var value))
        {
            return value;
        }
        return base.GetInt64();
    }

    public override double GetDouble()
    {
        if (double.TryParse(Utf16String.AsSpan(), out var value))
        {
            return value;
        }
        return base.GetDouble();
    }

    public override bool GetBool()
    {
        if (bool.TryParse(Utf16String.AsSpan(), out var value))
        {
            return value;
        }
        return base.GetBool();
    }

    public override DateTime GetDateTime()
    {
        if (DateTime.TryParse(Utf16String.AsSpan(), out var value))
        {
            return value;
        }
        return base.GetDateTime();
    }

    public override DateTimeOffset GetDateTimeOffset()
    {
        if (DateTimeOffset.TryParse(Utf16String.AsSpan(), out var value))
        {
            return value;
        }
        return base.GetDateTime();
    }

    public override DateOnly GetDateOnly()
    {
        if (TryGetDateTime(out DateTime value))
        {
            return DateOnly.FromDateTime(value);
        }
        return base.GetDateOnly();
    }

    public override TimeOnly GetTimeOnly()
    {
        if (TryGetDateTime(out DateTime value))
        {
            return TimeOnly.FromDateTime(value);
        }
        return base.GetTimeOnly();
    }

    public override object GetObject() => value;

}

