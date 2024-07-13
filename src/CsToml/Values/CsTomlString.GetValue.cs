
namespace CsToml.Values;

internal partial class CsTomlString
{
    public override bool CanGetValue(CsTomlValueFeature feature)
        => ((CsTomlValueFeature.String | CsTomlValueFeature.Int64 | CsTomlValueFeature.Double | CsTomlValueFeature.Bool | 
            CsTomlValueFeature.DateTime | CsTomlValueFeature.DateTimeOffset | CsTomlValueFeature.DateOnly | CsTomlValueFeature.TimeOnly) & feature) == feature;

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

}

