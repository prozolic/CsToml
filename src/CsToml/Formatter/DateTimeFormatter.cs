using CsToml.Error;
using CsToml.Utility;
using System.Buffers;

namespace CsToml.Formatter;

internal class DateTimeFormatter : ITomlValueFormatter<DateTime>
{
    public static readonly DateTimeFormatter Default = new DateTimeFormatter();

    public void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, DateTime value)
        where TBufferWriter : IBufferWriter<byte>
    {
        var totalMicrosecond = value.Millisecond * 1000 + value.Microsecond;
        if (totalMicrosecond == 0)
        {
            SerializeCore(ref writer, value, "s");
        }
        else
        {
            var length = TomlCodes.Number.DigitsDecimalUnroll4(totalMicrosecond);

            switch(length)
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
    }

    private void SerializeCore<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, DateTime value, ReadOnlySpan<char> format)
        where TBufferWriter : IBufferWriter<byte>
    {
        var length = 32;
        int bytesWritten;
        while (!value.TryFormat(writer.GetSpan(length), out bytesWritten, format))
        {
            length *= 2;
        }

        writer.Advance(bytesWritten);
    }

    public void Deserialize(ReadOnlySpan<byte> bytes, ref DateTime value)
    {
        if (bytes.Length < TomlCodes.DateTime.LocalDateTimeFormatLength)
            ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();

        if (TomlCodes.IsHyphen(bytes[4]) && TomlCodes.IsHyphen(bytes[7]) &&
            (TomlCodes.IsTabOrWhiteSpace(bytes[10]) || bytes[10] == TomlCodes.Alphabet.T || bytes[10] == TomlCodes.Alphabet.t))
        {
            DeserializeLocalDateTime(bytes, ref value);
            return;
        }

        ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
    }

    private void DeserializeLocalDateTime(ReadOnlySpan<byte> bytes, ref DateTime value)
    {
        DateOnly localDate = default;
        TimeOnly localTime = default;
        DateOnlyFormatter.Default.DeserializeUnsafe(bytes[..10], ref localDate);
        TimeOnlyFormatter.Default.DeserializeUnsafe(bytes[11..], ref localTime);

        try
        {
            value = new DateTime(localDate, localTime, DateTimeKind.Local);
        }
        catch (ArgumentOutOfRangeException e)
        {
            ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<DateTime>(e);
        }
    }
}
