
using CsToml.Utility;

namespace CsToml.Formatter;

internal class DateTimeOffsetFormatter : ICsTomlFormatter<DateTimeOffset>
{
    public static void Serialize(ref Utf8Writer writer, DateTimeOffset value)
    {
        var totalMicrosecond =  value.Millisecond * 1000 + value.Microsecond;
        var totalMinutes = value.Offset.TotalMinutes; // check timezone

        if (totalMicrosecond == 0 && totalMinutes == 0) 
        {
            SerializeCore(ref writer, value, "u");
        }
        else if (totalMicrosecond > 0 && totalMinutes != 0)
        {
            var length = CsTomlSyntax.Number.DigitsDecimalUnroll4(totalMicrosecond);

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
            var length = CsTomlSyntax.Number.DigitsDecimalUnroll4(totalMicrosecond);

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

    private static void SerializeCore(ref Utf8Writer writer, DateTimeOffset value, ReadOnlySpan<char> format)
    {
        var length = 32;
        int bytesWritten;
        while (!value.TryFormat(writer.GetSpan(length), out bytesWritten, format))
        {
            length *= 2;
        }

        writer.Advance(bytesWritten);
    }


    public static DateTimeOffset Deserialize(ref Utf8Reader reader, int length)
    {
        var bytes = reader.ReadBytes(length);
        if (bytes.Length < CsTomlSyntax.DateTime.OffsetDateTimeZFormat.Length) throw new ArgumentException();

        if (CsTomlSyntax.IsHyphen(bytes[4]) && CsTomlSyntax.IsHyphen(bytes[7]) && (bytes[10] == CsTomlSyntax.AlphaBet.T || CsTomlSyntax.IsTabOrWhiteSpace(bytes[10])))
        {
            if (bytes[bytes.Length - 1] ==  CsTomlSyntax.AlphaBet.Z)
                return DeserializeDateTimeOffset(bytes[..^1], bytes.Slice(bytes.Length - 1, 1));

            return DeserializeDateTimeOffset(bytes[..^6], bytes.Slice(bytes.Length - 6, 6));
        }

        throw new Exception();
    }

    private static DateTimeOffset DeserializeDateTimeOffset(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> offsetBytes)
    {
        var year = CsTomlSyntax.Number.ParseDecimal(bytes[0]) * 1000;
        year += CsTomlSyntax.Number.ParseDecimal(bytes[1]) * 100;
        year += CsTomlSyntax.Number.ParseDecimal(bytes[2]) * 10;
        year += CsTomlSyntax.Number.ParseDecimal(bytes[3]);

        var month = CsTomlSyntax.Number.ParseDecimal(bytes[5]) * 10;
        month += CsTomlSyntax.Number.ParseDecimal(bytes[6]);

        var day = CsTomlSyntax.Number.ParseDecimal(bytes[8]) * 10;
        day += CsTomlSyntax.Number.ParseDecimal(bytes[9]);

        var hour = CsTomlSyntax.Number.ParseDecimal(bytes[11]) * 10 + CsTomlSyntax.Number.ParseDecimal(bytes[12]);
        var minute = CsTomlSyntax.Number.ParseDecimal(bytes[14]) * 10 + CsTomlSyntax.Number.ParseDecimal(bytes[15]);
        var second = CsTomlSyntax.Number.ParseDecimal(bytes[17]) * 10 + CsTomlSyntax.Number.ParseDecimal(bytes[18]);

        // millisecond and microsecond is 0 ~ 999
        // https://learn.microsoft.com/en-us/dotnet/api/system.datetime.-ctor?view=net-8.0#system-datetime-ctor(system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32)
        var millisecond = 0;
        var microsecond = 0;

        if (bytes.Length == 21)
        {
            millisecond = CsTomlSyntax.Number.ParseDecimal(bytes[20]) * 100;
        }
        else if (bytes.Length == 22)
        {
            millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[20]) * 100;
            millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[21]) * 10;
        }
        else if (bytes.Length == 23)
        {
            millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[20]) * 100;
            millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[21]) * 10;
            millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[22]);
        }
        else if (bytes.Length == 24)
        {
            millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[20]) * 100;
            millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[21]) * 10;
            millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[22]);
            microsecond += CsTomlSyntax.Number.ParseDecimal(bytes[23]) * 100;
        }
        else if (bytes.Length == 25)
        {
            millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[20]) * 100;
            millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[21]) * 10;
            millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[22]);
            microsecond += CsTomlSyntax.Number.ParseDecimal(bytes[23]) * 100;
            microsecond += CsTomlSyntax.Number.ParseDecimal(bytes[24]) * 10;
        }
        else if (bytes.Length >= 26)
        {
            millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[20]) * 100;
            millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[21]) * 10;
            millisecond += CsTomlSyntax.Number.ParseDecimal(bytes[22]);
            microsecond += CsTomlSyntax.Number.ParseDecimal(bytes[23]) * 100;
            microsecond += CsTomlSyntax.Number.ParseDecimal(bytes[24]) * 10;
            microsecond += CsTomlSyntax.Number.ParseDecimal(bytes[25]);
        }

        if (offsetBytes.Length == 1 && offsetBytes[0] == CsTomlSyntax.AlphaBet.Z)
        {
            return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, microsecond, TimeSpan.Zero);
        }

        if (offsetBytes.Length == 6)
        {
            var plusOrMinus = CsTomlSyntax.IsPlusSign(offsetBytes[0]) ? 1 : -1;
            var offsetHour = CsTomlSyntax.Number.ParseDecimal(offsetBytes[1]) * 10 + CsTomlSyntax.Number.ParseDecimal(offsetBytes[2]);
            var offsetMinute = CsTomlSyntax.Number.ParseDecimal(offsetBytes[4]) * 10 + CsTomlSyntax.Number.ParseDecimal(offsetBytes[5]);
            return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, microsecond, new TimeSpan(offsetHour * plusOrMinus, offsetMinute * plusOrMinus ,0));
        }

        return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, microsecond, TimeSpan.Zero);
    }


}


