﻿using CsToml.Error;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("{Value}")]
internal sealed partial class TomlLocalDate(DateOnly value) : TomlValue
{
    public DateOnly Value { get; private set; } = value;

    public override bool HasValue => true;

    public override TomlValueType Type => TomlValueType.LocalDate;

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        writer.WriteDateOnly(Value);
    }

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (format.Length == 0 && provider == null)
        {
            return Value.TryFormat(destination, out charsWritten, "yyyy-MM-dd", provider);
        }
        return Value.TryFormat(destination, out charsWritten, format, provider);
    }

    public override string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format) && formatProvider == null)
        {
            return Value.ToString("yyyy-MM-dd");
        }
        return Value.ToString(format, formatProvider);
    }

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (format.Length == 0 && provider == null)
        {
            return Value.TryFormat(utf8Destination, out bytesWritten, "yyyy-MM-dd", provider);
        }
        return Value.TryFormat(utf8Destination, out bytesWritten, format, provider);
    }

    public override string ToString() => Value.ToString("yyyy-MM-dd");

    public static TomlLocalDate Parse(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != TomlCodes.DateTime.LocalDateFormatLength)
            ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();

        // local date
        if (!(TomlCodes.IsHyphen(bytes[4]) && TomlCodes.IsHyphen(bytes[7])))
        {
            ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        }

        return new TomlLocalDate(ParseDateOnly(bytes));
    }

    public static DateOnly ParseDateOnly(ReadOnlySpan<byte> bytes)
    {
        var year = TomlCodes.Number.ParseDecimal(bytes[0]) * 1000;
        year += TomlCodes.Number.ParseDecimal(bytes[1]) * 100;
        year += TomlCodes.Number.ParseDecimal(bytes[2]) * 10;
        year += TomlCodes.Number.ParseDecimal(bytes[3]);

        var month = TomlCodes.Number.ParseDecimal(bytes[5]) * 10;
        month += TomlCodes.Number.ParseDecimal(bytes[6]);

        var day = TomlCodes.Number.ParseDecimal(bytes[8]) * 10;
        day += TomlCodes.Number.ParseDecimal(bytes[9]);

        try
        {
            return new DateOnly(year, month, day);
        }
        catch (ArgumentOutOfRangeException e)
        {
            ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<DateOnly>(e);
            return default;
        }
    }

}
