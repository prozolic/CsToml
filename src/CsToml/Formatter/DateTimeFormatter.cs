using CsToml.Error;
using CsToml.Utility;
using System.Buffers;

namespace CsToml.Formatter;

internal class DateTimeFormatter : ICsTomlFormatter<DateTime>
{
    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, DateTime value)
        where TBufferWriter : IBufferWriter<byte>
    {
        var totalMicrosecond = value.Millisecond * 1000 + value.Microsecond;
        if (totalMicrosecond == 0)
        {
            SerializeCore(ref writer, value, "s");
        }
        else
        {
            var length = CsTomlSyntax.Number.DigitsDecimalUnroll4(totalMicrosecond);

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

    private static void SerializeCore<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, DateTime value, ReadOnlySpan<char> format)
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

    public static DateTime Deserialize(ref Utf8Reader reader, int length)
    {
        var bytes = reader.ReadBytes(length);
        if (bytes.Length < CsTomlSyntax.DateTime.LocalDateTimeFormatLength)
            ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();

        if (CsTomlSyntax.IsHyphen(bytes[4]) && CsTomlSyntax.IsHyphen(bytes[7]) &&
            (CsTomlSyntax.IsTabOrWhiteSpace(bytes[10]) || bytes[10] == CsTomlSyntax.Alphabet.T || bytes[10] == CsTomlSyntax.Alphabet.t))
        {
            return DeserializeLocalDateTime(bytes);
        }

        return ExceptionHelper.NotReturnThrow<DateTime>(ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat);
    }

    private static DateTime DeserializeLocalDateTime(ReadOnlySpan<byte> bytes)
    {
        var localDate = DateOnlyFormatter.DeserializeDateOnly(bytes[..10]);
        var localtime = TimeOnlyFormatter.DeserializeTimeOnly(bytes[11..]);

        try
        {
            return new DateTime(localDate, localtime, DateTimeKind.Local);
        }
        catch (ArgumentOutOfRangeException e)
        {
            return ExceptionHelper.NotReturnThrow<DateTime, ArgumentOutOfRangeException>(
                ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<DateTime>, e);
        }
    }
}
