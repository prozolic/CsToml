using CsToml.Error;
using CsToml.Extension;
using CsToml.Formatter;
using CsToml.Utility;
using System.Buffers.Text;
using System.Runtime.CompilerServices;

namespace CsToml.Values;

internal partial class CsTomlString
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool CanGetValue(CsTomlValueFeature feature)
        => ((CsTomlValueFeature.String | CsTomlValueFeature.Int64 | CsTomlValueFeature.Double | CsTomlValueFeature.Bool | 
            CsTomlValueFeature.DateTime | CsTomlValueFeature.DateTimeOffset | CsTomlValueFeature.DateOnly | CsTomlValueFeature.TimeOnly) & feature) == feature;

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
        if (BooleanExtensions.TryParse(Value, out var value))
        {
            return value;
        }

        return base.GetBool();
    }

    public override DateTime GetDateTime()
    {
        var trimBytes = Value.TrimWhiteSpace();
        if (Utf8Parser.TryParse(trimBytes, out DateTime value, out int bytesConsumed) && trimBytes.Length == bytesConsumed)
        {
            return value;
        }
        return base.GetDateTime();
    }

    public override DateTimeOffset GetDateTimeOffset()
    {
        var trimBytes = Value.TrimWhiteSpace();
        if (Utf8Parser.TryParse(trimBytes, out DateTimeOffset value, out int bytesConsumed) && trimBytes.Length == bytesConsumed)
        {
            return value;
        }
        return base.GetDateTimeOffset();
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

