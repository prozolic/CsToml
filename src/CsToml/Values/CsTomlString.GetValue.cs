using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text.Unicode;

namespace CsToml.Values;

internal partial class CsTomlString : ISpanFormattable
{
    public override string GetString() => Utf16String;

    public override long GetInt64()
    {
        if (Utf8Parser.TryParse(Value, out long value, out int bytesConsumed))
        {
            return value;
        }
        return base.GetInt64();
    }

    public override double GetDouble()
    {
        if (Utf8Parser.TryParse(Value, out double value, out int bytesConsumed))
        {
            return value;
        }
        return base.GetDouble();
    }

    public override bool GetBool()
    {
        if (Utf8Parser.TryParse(Value, out bool value, out int bytesConsumed))
        {
            return value;
        }
        return base.GetBool();
    }

    public override DateTime GetDateTime()
    {
        if (Utf8Parser.TryParse(Value, out DateTime value, out int bytesConsumed))
        {
            return value;
        }
        return base.GetDateTime();
    }

    public override DateTimeOffset GetDateTimeOffset()
    {
        if (Utf8Parser.TryParse(Value, out DateTimeOffset value, out int bytesConsumed))
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

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        var status = Utf8.ToUtf16(Value, destination, out var bytesRead, out charsWritten, replaceInvalidSequences: false);
        return status != OperationStatus.Done;
    }

    public string ToString(string? format, IFormatProvider? formatProvider) => GetString();
}

