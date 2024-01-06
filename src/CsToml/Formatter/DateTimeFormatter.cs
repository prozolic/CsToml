﻿using CsToml.Error;
using CsToml.Utility;

namespace CsToml.Formatter;

internal class DateTimeFormatter : ICsTomlFormatter<DateTime>
{
    public static void Serialize(ref Utf8Writer writer, DateTime value)
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

    private static void SerializeCore(ref Utf8Writer writer, DateTime value, ReadOnlySpan<char> format)
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
        if (bytes.Length < CsTomlSyntax.DateTime.LocalDateTimeFormat.Length)
            ExceptionHelper.ThrowIncorrectTomlFormat();

        if (CsTomlSyntax.IsHyphen(bytes[4]) && CsTomlSyntax.IsHyphen(bytes[7]) &&
            (CsTomlSyntax.IsTabOrWhiteSpace(bytes[10]) || bytes[10] == CsTomlSyntax.AlphaBet.T))
        {
            return DeserializeLocalDateTime(bytes);
        }

        return ExceptionHelper.NotReturnThrow<DateTime>(ExceptionHelper.ThrowIncorrectTomlFormat);
    }

    private static DateTime DeserializeLocalDateTime(ReadOnlySpan<byte> bytes)
    {
        var localDate = DateOnlyFormatter.DeserializeDateOnly(bytes[..10]);
        var localtime = TimeOnlyFormatter.DeserializeTimeOnly(bytes[11..]);
        return new DateTime(localDate, localtime, DateTimeKind.Local);
    }
}
