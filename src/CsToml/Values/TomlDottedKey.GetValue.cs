using CsToml.Error;
using System.Buffers.Text;

namespace CsToml.Values;

internal partial class TomlDottedKey
{
    public override bool CanGetValue(TomlValueFeature feature)=> ((
        TomlValueFeature.String | 
        TomlValueFeature.Int64 | 
        TomlValueFeature.Double | 
        TomlValueFeature.Boolean |
        TomlValueFeature.DateTime | 
        TomlValueFeature.DateTimeOffset | 
        TomlValueFeature.DateOnly | 
        TomlValueFeature.TimeOnly | 
        TomlValueFeature.Object) & feature) == feature;

    public override string GetString() => Utf16String;

    public override long GetInt64()
    {
        if (long.TryParse(Value, out var value))
        {
            return value;
        }
        return base.GetInt64();
    }

    public override double GetDouble()
    {
        if (double.TryParse(Value, out var value))
        {
            return value;
        }
        return base.GetDouble();
    }

    public override bool GetBool()
    {
        try
        {
            // no allcation.
            return TomlBoolean.Parse(Value).GetBool();
        }
        catch (CsTomlException)
        {
            return base.GetBool();
        }
    }

    public override DateTime GetDateTime()
    {
        if (Utf8Parser.TryParse(Value, out DateTime value, out int bytesConsumed, 'O'))
        {
            return value;
        }
        return base.GetDateTime();
    }

    public override DateTimeOffset GetDateTimeOffset()
    {
        if (Utf8Parser.TryParse(Value, out DateTimeOffset value, out int bytesConsumed, 'O'))
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

    public override object GetObject() => bytes.ToArray();
}

