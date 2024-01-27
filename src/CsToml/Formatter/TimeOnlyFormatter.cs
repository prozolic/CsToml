﻿using CsToml.Error;
using CsToml.Utility;

namespace CsToml.Formatter;

internal class TimeOnlyFormatter : ICsTomlFormatter<TimeOnly>
{
    public static void Serialize(ref Utf8Writer writer, TimeOnly value)
    {
        var totalMicrosecond = value.Millisecond * 100 + value.Microsecond;
        if (totalMicrosecond == 0)
        {
            SerializeCore(ref writer, value, "HH:mm:ss");
        }
        else
        {
            var length = CsTomlSyntax.Number.DigitsDecimalUnroll4(totalMicrosecond);

            switch (length)
            {
                case 1:
                    SerializeCore(ref writer, value, "HH:mm:ss.f");
                    break;
                case 2:
                    SerializeCore(ref writer, value, "HH:mm:ss.ff");
                    break;
                case 3:
                    SerializeCore(ref writer, value, "HH:mm:ss.fff");
                    break;
                case 4:
                    SerializeCore(ref writer, value, "HH:mm:ss.ffff");
                    break;
                case 5:
                    SerializeCore(ref writer, value, "HH:mm:ss.fffff");
                    break;
                case 6:
                    SerializeCore(ref writer, value, "HH:mm:ss.ffffff");
                    break;
                default:
                    SerializeCore(ref writer, value, "HH:mm:ss.ffffff");
                    break;

            }

        }
    }

    private static void SerializeCore(ref Utf8Writer writer, TimeOnly value, ReadOnlySpan<char> format)
    {
        value.TryFormat(writer.GetWriteSpan(format.Length), out var bytesWritten, format);
    }

    public static TimeOnly Deserialize(ref Utf8Reader reader, int length)
    {
        var bytes = reader.ReadBytes(length);
        if (bytes.Length < CsTomlSyntax.DateTime.LocalTimeFormat.Length)
            ExceptionHelper.ThrowIncorrectTomlFormat();

        // local time
        if (!(CsTomlSyntax.IsColon(bytes[2]) && CsTomlSyntax.IsColon(bytes[5])))
            ExceptionHelper.ThrowIncorrectTomlFormat();

        return DeserializeTimeOnly(bytes);
    }

    internal static TimeOnly DeserializeTimeOnly(ReadOnlySpan<byte> bytes)
    {
        var hour = CsTomlSyntax.Number.ParseDecimal(bytes[0]) * 10 + CsTomlSyntax.Number.ParseDecimal(bytes[1]);
        var minute = CsTomlSyntax.Number.ParseDecimal(bytes[3]) * 10 + CsTomlSyntax.Number.ParseDecimal(bytes[4]);
        var second = CsTomlSyntax.Number.ParseDecimal(bytes[6]) * 10 + CsTomlSyntax.Number.ParseDecimal(bytes[7]);

        // millisecond and microsecond is 0 ~ 999
        // https://learn.microsoft.com/en-us/dotnet/api/system.datetime.-ctor?view=net-8.0#system-datetime-ctor(system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32)
        var millisecond = 0;
        var microsecond = 0;
        if (bytes.Length > 8 && CsTomlSyntax.IsPeriod(bytes[8]))
        {
            if (bytes.Length == 10)
            {
                millisecond = CsTomlSyntax.Number.ParseDecimal(bytes[9]) * 100;
            }
            else if (bytes.Length == 11)
            {
                millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[9]) * 100;
                millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[10]) * 10;
            }
            else if (bytes.Length == 12)
            {
                millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[9]) * 100;
                millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[10]) * 10;
                millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[11]);
            }
            else if (bytes.Length == 13)
            {
                millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[9]) * 100;
                millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[10]) * 10;
                millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[11]);
                microsecond += CsTomlSyntax.Number.ParseDecimal(bytes[12]) * 100;
            }
            else if (bytes.Length == 14)
            {
                millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[9]) * 100;
                millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[10]) * 10;
                millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[11]);
                microsecond += CsTomlSyntax.Number.ParseDecimal(bytes[12]) * 100;
                microsecond += CsTomlSyntax.Number.ParseDecimal(bytes[13]) * 10;
            }
            else if (bytes.Length >= 15)
            {
                millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[9]) * 100;
                millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[10]) * 10;
                millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[11]);
                microsecond += CsTomlSyntax.Number.ParseDecimal(bytes[12]) * 100;
                microsecond += CsTomlSyntax.Number.ParseDecimal(bytes[13]) * 10;
                microsecond += CsTomlSyntax.Number.ParseDecimal(bytes[14]);
            }
        }
        return new TimeOnly(hour, minute, second, millisecond, microsecond);
    }

}
