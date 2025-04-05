using CsToml.Error;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml.Values;

[DebuggerDisplay("{Value}")]
internal sealed partial class TomlOffsetDateTime(DateTimeOffset value) : TomlValue
{
    public DateTimeOffset Value { get; private set; } = value;

    public override bool HasValue => true;

    public override TomlValueType Type => TomlValueType.OffsetDateTime;

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        writer.WriteDateTimeOffset(Value);
    }

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (format.Length == 0 && provider == null)
        {
            return Value.TryFormat(destination, out charsWritten, "o", provider);
        }
        return Value.TryFormat(destination, out charsWritten, format, provider);
    }

    public override string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format) && formatProvider == null)
        {
            return ToString();
        }
        return Value.ToString(format, formatProvider);
    }

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (format.Length == 0 && provider == null)
        {
            return Value.TryFormat(utf8Destination, out bytesWritten, "o", provider);
        }
        return Value.TryFormat(utf8Destination, out bytesWritten, format, provider);
    }

    public override string ToString() => Value.ToString("o");

    public static TomlOffsetDateTime Parse(ReadOnlySpan<byte> bytes)
    {
        if (bytes[bytes.Length - 1] == TomlCodes.Alphabet.Z || bytes[bytes.Length - 1] == TomlCodes.Alphabet.z)
        {
            return new TomlOffsetDateTime(ParseDateTimeOffset(bytes[..^1], bytes.Slice(bytes.Length - 1, 1)));
        }

        return new TomlOffsetDateTime(ParseDateTimeOffset(bytes[..^6], bytes.Slice(bytes.Length - 6, 6)));
    }

    public static TomlOffsetDateTime ParseToOmitSeconds(ReadOnlySpan<byte> bytes)
    {
        if (bytes[bytes.Length - 1] == TomlCodes.Alphabet.Z || bytes[bytes.Length - 1] == TomlCodes.Alphabet.z)
        {
            return new TomlOffsetDateTime(ParseDateTimeOffset2(bytes[..^1], bytes.Slice(bytes.Length - 1, 1)));
        }

        return new TomlOffsetDateTime(ParseDateTimeOffset2(bytes[..^6], bytes.Slice(bytes.Length - 6, 6)));
    }

    private static DateTimeOffset ParseDateTimeOffset(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> offsetBytes)
    {
        var year = TomlCodes.Number.ParseDecimal(bytes[0]) * 1000;
        year += TomlCodes.Number.ParseDecimal(bytes[1]) * 100;
        year += TomlCodes.Number.ParseDecimal(bytes[2]) * 10;
        year += TomlCodes.Number.ParseDecimal(bytes[3]);

        var month = TomlCodes.Number.ParseDecimal(bytes[5]) * 10;
        month += TomlCodes.Number.ParseDecimal(bytes[6]);

        var day = TomlCodes.Number.ParseDecimal(bytes[8]) * 10;
        day += TomlCodes.Number.ParseDecimal(bytes[9]);

        var hour = TomlCodes.Number.ParseDecimal(bytes[11]) * 10 + TomlCodes.Number.ParseDecimal(bytes[12]);
        var minute = TomlCodes.Number.ParseDecimal(bytes[14]) * 10 + TomlCodes.Number.ParseDecimal(bytes[15]);
        var second = TomlCodes.Number.ParseDecimal(bytes[17]) * 10 + TomlCodes.Number.ParseDecimal(bytes[18]);

        // millisecond and microsecond is 0 ~ 999
        // https://learn.microsoft.com/en-us/dotnet/api/system.datetime.-ctor?view=net-8.0#system-datetime-ctor(system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32)
        var millisecond = 0;
        var microsecond = 0;

        if (bytes.Length == 21)
        {
            millisecond = TomlCodes.Number.ParseDecimal(bytes[20]) * 100;
        }
        else if (bytes.Length == 22)
        {
            millisecond += TomlCodes.Number.ParseDecimal(bytes[20]) * 100;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[21]) * 10;
        }
        else if (bytes.Length == 23)
        {
            millisecond += TomlCodes.Number.ParseDecimal(bytes[20]) * 100;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[21]) * 10;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[22]);
        }
        else if (bytes.Length == 24)
        {
            millisecond += TomlCodes.Number.ParseDecimal(bytes[20]) * 100;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[21]) * 10;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[22]);
            microsecond += TomlCodes.Number.ParseDecimal(bytes[23]) * 100;
        }
        else if (bytes.Length == 25)
        {
            millisecond += TomlCodes.Number.ParseDecimal(bytes[20]) * 100;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[21]) * 10;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[22]);
            microsecond += TomlCodes.Number.ParseDecimal(bytes[23]) * 100;
            microsecond += TomlCodes.Number.ParseDecimal(bytes[24]) * 10;
        }
        else if (bytes.Length >= 26)
        {
            millisecond += TomlCodes.Number.ParseDecimal(bytes[20]) * 100;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[21]) * 10;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[22]);
            microsecond += TomlCodes.Number.ParseDecimal(bytes[23]) * 100;
            microsecond += TomlCodes.Number.ParseDecimal(bytes[24]) * 10;
            microsecond += TomlCodes.Number.ParseDecimal(bytes[25]);
        }

        if (offsetBytes.Length == 1 && offsetBytes[0] == TomlCodes.Alphabet.Z || offsetBytes[0] == TomlCodes.Alphabet.z)
        {
            try
            {
                return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, microsecond, TimeSpan.Zero);
            }
            catch (ArgumentOutOfRangeException e)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<DateTimeOffset>(e);
                return default;
            }
        }

        if (offsetBytes.Length == 6)
        {
            var offset = ParseOffset(offsetBytes);
            try
            {
                return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, microsecond, offset);
            }
            catch (ArgumentOutOfRangeException e)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<DateTimeOffset>(e);
                return default;
            }
        }
        try
        {
            return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, microsecond, TimeSpan.Zero);
        }
        catch (ArgumentOutOfRangeException e)
        {
            ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<DateTimeOffset>(e);
            return default;
        }
    }

    private static DateTimeOffset ParseDateTimeOffset2(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> offsetBytes)
    {
        var year = TomlCodes.Number.ParseDecimal(bytes[0]) * 1000;
        year += TomlCodes.Number.ParseDecimal(bytes[1]) * 100;
        year += TomlCodes.Number.ParseDecimal(bytes[2]) * 10;
        year += TomlCodes.Number.ParseDecimal(bytes[3]);

        var month = TomlCodes.Number.ParseDecimal(bytes[5]) * 10;
        month += TomlCodes.Number.ParseDecimal(bytes[6]);

        var day = TomlCodes.Number.ParseDecimal(bytes[8]) * 10;
        day += TomlCodes.Number.ParseDecimal(bytes[9]);

        var hour = TomlCodes.Number.ParseDecimal(bytes[11]) * 10 + TomlCodes.Number.ParseDecimal(bytes[12]);
        var minute = TomlCodes.Number.ParseDecimal(bytes[14]) * 10 + TomlCodes.Number.ParseDecimal(bytes[15]);
        var second = 0;

        // millisecond and microsecond is 0 ~ 999
        // https://learn.microsoft.com/en-us/dotnet/api/system.datetime.-ctor?view=net-8.0#system-datetime-ctor(system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32)
        var millisecond = 0;
        var microsecond = 0;

        if (bytes.Length == 18)
        {
            millisecond = TomlCodes.Number.ParseDecimal(bytes[17]) * 100;
        }
        else if (bytes.Length == 19)
        {
            millisecond += TomlCodes.Number.ParseDecimal(bytes[17]) * 100;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[18]) * 10;
        }
        else if (bytes.Length == 20)
        {
            millisecond += TomlCodes.Number.ParseDecimal(bytes[17]) * 100;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[18]) * 10;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[19]);
        }
        else if (bytes.Length == 21)
        {
            millisecond += TomlCodes.Number.ParseDecimal(bytes[17]) * 100;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[18]) * 10;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[19]);
            microsecond += TomlCodes.Number.ParseDecimal(bytes[20]) * 100;
        }
        else if (bytes.Length == 22)
        {
            millisecond += TomlCodes.Number.ParseDecimal(bytes[17]) * 100;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[18]) * 10;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[19]);
            microsecond += TomlCodes.Number.ParseDecimal(bytes[20]) * 100;
            microsecond += TomlCodes.Number.ParseDecimal(bytes[21]) * 10;
        }
        else if (bytes.Length >= 23)
        {
            millisecond += TomlCodes.Number.ParseDecimal(bytes[17]) * 100;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[18]) * 10;
            millisecond += TomlCodes.Number.ParseDecimal(bytes[19]);
            microsecond += TomlCodes.Number.ParseDecimal(bytes[20]) * 100;
            microsecond += TomlCodes.Number.ParseDecimal(bytes[21]) * 10;
            microsecond += TomlCodes.Number.ParseDecimal(bytes[22]);
        }

        if (offsetBytes.Length == 1 && offsetBytes[0] == TomlCodes.Alphabet.Z || offsetBytes[0] == TomlCodes.Alphabet.z)
        {
            try
            {
                return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, microsecond, TimeSpan.Zero);
            }
            catch (ArgumentOutOfRangeException e)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<DateTimeOffset>(e);
                return default;
            }
        }

        if (offsetBytes.Length == 6)
        {
            var offset = ParseOffset(offsetBytes);

            try
            {
                return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, microsecond, offset);
            }
            catch (ArgumentOutOfRangeException e)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<DateTimeOffset>(e);
                return default;
            }
        }
        try
        {
            return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, microsecond, TimeSpan.Zero);
        }
        catch (ArgumentOutOfRangeException e)
        {
            ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<DateTimeOffset>(e);
            return default;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TimeSpan ParseOffset(ReadOnlySpan<byte> offsetBytes)
    {
        var plusOrMinus = TomlCodes.IsPlusSign(offsetBytes[0]) ? 1 : -1;
        var offsetHour = TomlCodes.Number.ParseDecimal(offsetBytes[1]) * 10 + TomlCodes.Number.ParseDecimal(offsetBytes[2]);
        var offsetMinute = TomlCodes.Number.ParseDecimal(offsetBytes[4]) * 10 + TomlCodes.Number.ParseDecimal(offsetBytes[5]);

        if (offsetHour < 0 || 23 < offsetHour)
        {
            ExceptionHelper.ThrowException($"Offset Date-Time time-numoffset(time-hour) is in an invalid format. time-hour:{offsetHour}");
        }
        else if (offsetMinute < 0 || 59 < offsetMinute)
        {
            ExceptionHelper.ThrowException($"Offset Date-Time time-numoffset(time-minute) is in an invalid format. time-minute:{offsetMinute}");
        }

        return new TimeSpan(offsetHour * plusOrMinus, offsetMinute * plusOrMinus, 0);
    }

}
