
using CsToml.Error;
using CsToml.Utility;
using System.Buffers;

namespace CsToml.Formatter;

internal class DateTimeOffsetFormatter : ICsTomlFormatter<DateTimeOffset>
{
    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, DateTimeOffset value)
        where TBufferWriter : IBufferWriter<byte>
    {
        var totalMicrosecond =  value.Millisecond * 1000 + value.Microsecond;
        var totalMinutes = value.Offset.TotalMinutes; // check timezone

        if (totalMicrosecond == 0 && totalMinutes == 0) 
        {
            SerializeCore(ref writer, value, "u");
        }
        else if (totalMicrosecond > 0 && totalMinutes != 0)
        {
            var length = TomlCodes.Number.DigitsDecimalUnroll4(totalMicrosecond);

            switch (length)
            {
                case 1:
                    SerializeCore(ref writer, value, "yyyy-MM-ddTHH:mm:ss.fzzz");
                    break;
                case 2:
                    SerializeCore(ref writer, value, "yyyy-MM-ddTHH:mm:ss.ffzzz");
                    break;
                case 3:
                    SerializeCore(ref writer, value, "yyyy-MM-ddTHH:mm:ss.fffzzz");
                    break;
                case 4:
                    SerializeCore(ref writer, value, "yyyy-MM-ddTHH:mm:ss.ffffzzz");
                    break;
                case 5:
                    SerializeCore(ref writer, value, "yyyy-MM-ddTHH:mm:ss.fffffzzz");
                    break;
                case 6:
                    SerializeCore(ref writer, value, "yyyy-MM-ddTHH:mm:ss.ffffffzzz");
                    break;
                default:
                    SerializeCore(ref writer, value, "yyyy-MM-ddTHH:mm:ss.ffffffzzz");
                    break;
            }
        }
        else if (totalMicrosecond > 0)
        {
            var length = TomlCodes.Number.DigitsDecimalUnroll4(totalMicrosecond);

            switch (length)
            {
                case 1:
                    SerializeCore(ref writer, value, "yyyy-MM-ddTHH:mm:ss.f");
                    break;
                case 2:
                    SerializeCore(ref writer, value, "yyyy-MM-ddTHH:mm:ss.ff");
                    break;
                case 3:
                    SerializeCore(ref writer, value, "yyyy-MM-ddTHH:mm:ss.fff");
                    break;
                case 4:
                    SerializeCore(ref writer, value, "yyyy-MM-ddTHH:mm:ss.ffff");
                    break;
                case 5:
                    SerializeCore(ref writer, value, "yyyy-MM-ddTHH:mm:ss.fffff");
                    break;
                case 6:
                    SerializeCore(ref writer, value, "yyyy-MM-ddTHH:mm:ss.ffffff");
                    break;
                default:
                    SerializeCore(ref writer, value, "yyyy-MM-ddTHH:mm:ss.ffffff");
                    break;
            }
        }
        else if (totalMinutes != 0)
        {
            SerializeCore(ref writer, value, "yyyy-MM-ddTHH:mm:sszzz");
        }
    }

    private static void SerializeCore<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, DateTimeOffset value, ReadOnlySpan<char> format)
        where TBufferWriter : IBufferWriter<byte>
    {
        var length = 32;
        int bytesWritten;
        Span<byte> buffer = writer.GetSpan(length);
        while (!value.TryFormat(buffer, out bytesWritten, format))
        {
            length *= 2;
            buffer = writer.GetSpan(length);
        }

        // ex 1979-05-27 07:32:00Z -> 1979-05-27T07:32:00Z
        if (value.Offset == TimeSpan.Zero)
        {
            buffer[10] = TomlCodes.Alphabet.T;
        }

        writer.Advance(bytesWritten);
    }

    public static DateTimeOffset Deserialize(ref Utf8Reader reader, int length)
    {
        var bytes = reader.ReadBytes(length);
        if (bytes.Length < TomlCodes.DateTime.OffsetDateTimeZFormatLength) throw new ArgumentException();

        if (TomlCodes.IsHyphen(bytes[4]) && TomlCodes.IsHyphen(bytes[7]) && (bytes[10] == TomlCodes.Alphabet.T || bytes[10] == TomlCodes.Alphabet.t || TomlCodes.IsTabOrWhiteSpace(bytes[10])))
        {
            if (bytes[bytes.Length - 1] ==  TomlCodes.Alphabet.Z || bytes[bytes.Length - 1] == TomlCodes.Alphabet.z)
                return DeserializeDateTimeOffset(bytes[..^1], bytes.Slice(bytes.Length - 1, 1));

            return DeserializeDateTimeOffset(bytes[..^6], bytes.Slice(bytes.Length - 6, 6));
        }

        throw new Exception();
    }

    private static DateTimeOffset DeserializeDateTimeOffset(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> offsetBytes)
    {
        var year = DeserializeDecimal(bytes[0]) * 1000;
        year += DeserializeDecimal(bytes[1]) * 100;
        year += DeserializeDecimal(bytes[2]) * 10;
        year += DeserializeDecimal(bytes[3]);

        var month = DeserializeDecimal(bytes[5]) * 10;
        month += DeserializeDecimal(bytes[6]);

        var day = DeserializeDecimal(bytes[8]) * 10;
        day += DeserializeDecimal(bytes[9]);

        var hour = DeserializeDecimal(bytes[11]) * 10 + DeserializeDecimal(bytes[12]);
        var minute = DeserializeDecimal(bytes[14]) * 10 + DeserializeDecimal(bytes[15]);
        var second = DeserializeDecimal(bytes[17]) * 10 + DeserializeDecimal(bytes[18]);

        // millisecond and microsecond is 0 ~ 999
        // https://learn.microsoft.com/en-us/dotnet/api/system.datetime.-ctor?view=net-8.0#system-datetime-ctor(system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32)
        var millisecond = 0;
        var microsecond = 0;

        if (bytes.Length == 21)
        {
            millisecond = DeserializeDecimal(bytes[20]) * 100;
        }
        else if (bytes.Length == 22)
        {
            millisecond += DeserializeDecimal(bytes[20]) * 100;
            millisecond += DeserializeDecimal(bytes[21]) * 10;
        }
        else if (bytes.Length == 23)
        {
            millisecond += DeserializeDecimal(bytes[20]) * 100;
            millisecond += DeserializeDecimal(bytes[21]) * 10;
            millisecond += DeserializeDecimal(bytes[22]);
        }
        else if (bytes.Length == 24)
        {
            millisecond += DeserializeDecimal(bytes[20]) * 100;
            millisecond += DeserializeDecimal(bytes[21]) * 10;
            millisecond += DeserializeDecimal(bytes[22]);
            microsecond += DeserializeDecimal(bytes[23]) * 100;
        }
        else if (bytes.Length == 25)
        {
            millisecond += DeserializeDecimal(bytes[20]) * 100;
            millisecond += DeserializeDecimal(bytes[21]) * 10;
            millisecond += DeserializeDecimal(bytes[22]);
            microsecond += DeserializeDecimal(bytes[23]) * 100;
            microsecond += DeserializeDecimal(bytes[24]) * 10;
        }
        else if (bytes.Length >= 26)
        {
            millisecond += DeserializeDecimal(bytes[20]) * 100;
            millisecond += DeserializeDecimal(bytes[21]) * 10;
            millisecond += DeserializeDecimal(bytes[22]);
            microsecond += DeserializeDecimal(bytes[23]) * 100;
            microsecond += DeserializeDecimal(bytes[24]) * 10;
            microsecond += DeserializeDecimal(bytes[25]);
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
            var offsetHour = DeserializeDecimal(offsetBytes[1]) * 10 + DeserializeDecimal(offsetBytes[2]);
            var offsetMinute = DeserializeDecimal(offsetBytes[4]) * 10 + DeserializeDecimal(offsetBytes[5]);
            try
            {
                return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, microsecond, new TimeSpan(offsetHour * plusOrMinus, offsetMinute * plusOrMinus, 0));
            }
            catch (ArgumentOutOfRangeException e)
            {
                return ExceptionHelper.NotReturnThrow<DateTimeOffset, ArgumentOutOfRangeException>(
                    ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<DateTimeOffset>, e);
            }
        }
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

    internal static int DeserializeDecimal(byte utf8Byte)
    {
        if (!TomlCodes.IsNumber(utf8Byte))
        {
            ExceptionHelper.ThrowNumericConversionFailed(utf8Byte);
        }
        return TomlCodes.Number.ParseDecimal(utf8Byte);
    }
}


