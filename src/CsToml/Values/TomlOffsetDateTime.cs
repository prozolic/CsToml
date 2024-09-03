using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("{Value}")]
internal sealed partial class TomlOffsetDateTime(DateTimeOffset value) : TomlValue()
{
    public DateTimeOffset Value { get; private set; } = value;

    public override bool HasValue => true;

    internal override bool ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        writer.WriteDateTimeOffset(Value);
        return true;
    }

    public override string ToString()
        => GetString();

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        => Value.TryFormat(destination, out charsWritten, format, provider);

    public override string ToString(string? format, IFormatProvider? formatProvider)
        => Value.ToString(format, formatProvider);

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        => Value.TryFormat(utf8Destination, out bytesWritten, format, provider);

    public static TomlOffsetDateTime Parse(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < TomlCodes.DateTime.OffsetDateTimeZFormatLength) throw new ArgumentException();

        if (TomlCodes.IsHyphen(bytes[4]) && TomlCodes.IsHyphen(bytes[7]) && (bytes[10] == TomlCodes.Alphabet.T || bytes[10] == TomlCodes.Alphabet.t || TomlCodes.IsTabOrWhiteSpace(bytes[10])))
        {
            if (bytes[bytes.Length - 1] == TomlCodes.Alphabet.Z || bytes[bytes.Length - 1] == TomlCodes.Alphabet.z)
            {
                return new TomlOffsetDateTime(ParseDateTimeOffset(bytes[..^1], bytes.Slice(bytes.Length - 1, 1)));
            }

            return new TomlOffsetDateTime(ParseDateTimeOffset(bytes[..^6], bytes.Slice(bytes.Length - 6, 6)));
        }

        ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        return default;
    }

    private static DateTimeOffset ParseDateTimeOffset(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> offsetBytes)
    {
        var year = ParseDecimalByte(bytes[0]) * 1000;
        year += ParseDecimalByte(bytes[1]) * 100;
        year += ParseDecimalByte(bytes[2]) * 10;
        year += ParseDecimalByte(bytes[3]);

        var month = ParseDecimalByte(bytes[5]) * 10;
        month += ParseDecimalByte(bytes[6]);

        var day = ParseDecimalByte(bytes[8]) * 10;
        day += ParseDecimalByte(bytes[9]);

        var hour = ParseDecimalByte(bytes[11]) * 10 + ParseDecimalByte(bytes[12]);
        var minute = ParseDecimalByte(bytes[14]) * 10 + ParseDecimalByte(bytes[15]);
        var second = ParseDecimalByte(bytes[17]) * 10 + ParseDecimalByte(bytes[18]);

        // millisecond and microsecond is 0 ~ 999
        // https://learn.microsoft.com/en-us/dotnet/api/system.datetime.-ctor?view=net-8.0#system-datetime-ctor(system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32)
        var millisecond = 0;
        var microsecond = 0;

        if (bytes.Length == 21)
        {
            millisecond = ParseDecimalByte(bytes[20]) * 100;
        }
        else if (bytes.Length == 22)
        {
            millisecond += ParseDecimalByte(bytes[20]) * 100;
            millisecond += ParseDecimalByte(bytes[21]) * 10;
        }
        else if (bytes.Length == 23)
        {
            millisecond += ParseDecimalByte(bytes[20]) * 100;
            millisecond += ParseDecimalByte(bytes[21]) * 10;
            millisecond += ParseDecimalByte(bytes[22]);
        }
        else if (bytes.Length == 24)
        {
            millisecond += ParseDecimalByte(bytes[20]) * 100;
            millisecond += ParseDecimalByte(bytes[21]) * 10;
            millisecond += ParseDecimalByte(bytes[22]);
            microsecond += ParseDecimalByte(bytes[23]) * 100;
        }
        else if (bytes.Length == 25)
        {
            millisecond += ParseDecimalByte(bytes[20]) * 100;
            millisecond += ParseDecimalByte(bytes[21]) * 10;
            millisecond += ParseDecimalByte(bytes[22]);
            microsecond += ParseDecimalByte(bytes[23]) * 100;
            microsecond += ParseDecimalByte(bytes[24]) * 10;
        }
        else if (bytes.Length >= 26)
        {
            millisecond += ParseDecimalByte(bytes[20]) * 100;
            millisecond += ParseDecimalByte(bytes[21]) * 10;
            millisecond += ParseDecimalByte(bytes[22]);
            microsecond += ParseDecimalByte(bytes[23]) * 100;
            microsecond += ParseDecimalByte(bytes[24]) * 10;
            microsecond += ParseDecimalByte(bytes[25]);
        }

        if (offsetBytes.Length == 1 && offsetBytes[0] == TomlCodes.Alphabet.Z || offsetBytes[0] == TomlCodes.Alphabet.z)
        {
            try
            {
                return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, microsecond, TimeSpan.Zero);
            }
            catch (ArgumentOutOfRangeException e)
            {
                return ExceptionHelper.NotReturnThrow<DateTimeOffset, ArgumentOutOfRangeException>(
                    ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<DateTimeOffset>, e);
            }
        }

        if (offsetBytes.Length == 6)
        {
            var plusOrMinus = TomlCodes.IsPlusSign(offsetBytes[0]) ? 1 : -1;
            var offsetHour = ParseDecimalByte(offsetBytes[1]) * 10 + ParseDecimalByte(offsetBytes[2]);
            var offsetMinute = ParseDecimalByte(offsetBytes[4]) * 10 + ParseDecimalByte(offsetBytes[5]);
            
            if (offsetHour < 0 || 23 < offsetHour)
            {
                ExceptionHelper.ThrowException($"Offset Date-Time time-numoffset(time-hour) is in an invalid format. time-hour:{offsetHour}");
            }
            else if (offsetMinute < 0 || 59 < offsetMinute)
            {
                ExceptionHelper.ThrowException($"Offset Date-Time time-numoffset(time-minute) is in an invalid format. time-minute:{offsetMinute}");
            }

            try
            {
                return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, microsecond, new TimeSpan(offsetHour * plusOrMinus, offsetMinute * plusOrMinus, 0));
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

    private static int ParseDecimalByte(byte utf8Byte)
    {
        if (!TomlCodes.IsNumber(utf8Byte))
        {
            ExceptionHelper.ThrowNumericConversionFailed(utf8Byte);
        }
        return TomlCodes.Number.ParseDecimal(utf8Byte);
    }
}
