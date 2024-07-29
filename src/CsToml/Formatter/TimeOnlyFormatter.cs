using CsToml.Error;
using CsToml.Utility;
using System.Buffers;

namespace CsToml.Formatter;

internal class TimeOnlyFormatter : ICsTomlFormatter<TimeOnly>
{
    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, TimeOnly value)
        where TBufferWriter : IBufferWriter<byte>
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

    private static void SerializeCore<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, TimeOnly value, ReadOnlySpan<char> format)
        where TBufferWriter : IBufferWriter<byte>
    {
        value.TryFormat(writer.GetWrittenSpan(format.Length), out var bytesWritten, format);
    }

    public static TimeOnly Deserialize(ref Utf8Reader reader, int length)
    {
        var bytes = reader.ReadBytes(length);
        if (bytes.Length < CsTomlSyntax.DateTime.LocalTimeFormatLength)
            ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();

        // local time
        if (!(CsTomlSyntax.IsColon(bytes[2]) && CsTomlSyntax.IsColon(bytes[5])))
            ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();

        return DeserializeTimeOnly(bytes);
    }

    internal static TimeOnly DeserializeTimeOnly(ReadOnlySpan<byte> bytes)
    {
        var hour = DeserializeDecimal(bytes[0]) * 10 + DeserializeDecimal(bytes[1]);
        var minute = DeserializeDecimal(bytes[3]) * 10 + DeserializeDecimal(bytes[4]);
        var second = DeserializeDecimal(bytes[6]) * 10 + DeserializeDecimal(bytes[7]);

        // millisecond and microsecond is 0 ~ 999
        // https://learn.microsoft.com/en-us/dotnet/api/system.datetime.-ctor?view=net-8.0#system-datetime-ctor(system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32)
        var millisecond = 0;
        var microsecond = 0;
        if (bytes.Length > 8 && CsTomlSyntax.IsPeriod(bytes[8]))
        {
            if (bytes.Length == 10)
            {
                millisecond = DeserializeDecimal(bytes[9]) * 100;
            }
            else if (bytes.Length == 11)
            {
                millisecond += DeserializeDecimal(bytes[9]) * 100;
                millisecond += DeserializeDecimal(bytes[10]) * 10;
            }
            else if (bytes.Length == 12)
            {
                millisecond += DeserializeDecimal(bytes[9]) * 100;
                millisecond += DeserializeDecimal(bytes[10]) * 10;
                millisecond += DeserializeDecimal(bytes[11]);
            }
            else if (bytes.Length == 13)
            {
                millisecond += DeserializeDecimal(bytes[9]) * 100;
                millisecond += DeserializeDecimal(bytes[10]) * 10;
                millisecond += DeserializeDecimal(bytes[11]);
                microsecond += DeserializeDecimal(bytes[12]) * 100;
            }
            else if (bytes.Length == 14)
            {
                millisecond += DeserializeDecimal(bytes[9]) * 100;
                millisecond += DeserializeDecimal(bytes[10]) * 10;
                millisecond += DeserializeDecimal(bytes[11]);
                microsecond += DeserializeDecimal(bytes[12]) * 100;
                microsecond += DeserializeDecimal(bytes[13]) * 10;
            }
            else if (bytes.Length >= 15)
            {
                millisecond += DeserializeDecimal(bytes[9]) * 100;
                millisecond += DeserializeDecimal(bytes[10]) * 10;
                millisecond += DeserializeDecimal(bytes[11]);
                microsecond += DeserializeDecimal(bytes[12]) * 100;
                microsecond += DeserializeDecimal(bytes[13]) * 10;
                microsecond += DeserializeDecimal(bytes[14]);
            }
        }

        try
        {
            return new TimeOnly(hour, minute, second, millisecond, microsecond);
        }
        catch (ArgumentOutOfRangeException e)
        {
            return ExceptionHelper.NotReturnThrow<TimeOnly, ArgumentOutOfRangeException>(
                ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<TimeOnly>, e);
        }
    }

    internal static int DeserializeDecimal(byte utf8Byte)
    {
        if (!CsTomlSyntax.IsNumber(utf8Byte))
        {
            ExceptionHelper.ThrowNumericConversionFailed(utf8Byte);
        }
        return CsTomlSyntax.Number.ParseDecimal(utf8Byte);
    }

}

