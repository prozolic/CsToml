using CsToml.Error;
using CsToml.Utility;
using System.Buffers;

namespace CsToml.Formatter;

internal class TimeOnlyFormatter : ITomlValueFormatter<TimeOnly>
{
    public static readonly TimeOnlyFormatter Default = new TimeOnlyFormatter();

    public void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, TimeOnly value)
        where TBufferWriter : IBufferWriter<byte>
    {
        var totalMicrosecond = value.Millisecond * 100 + value.Microsecond;
        if (totalMicrosecond == 0)
        {
            SerializeCore(ref writer, value, "HH:mm:ss");
        }
        else
        {
            var length = TomlCodes.Number.DigitsDecimalUnroll4(totalMicrosecond);

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

    public void Deserialize(ReadOnlySpan<byte> bytes, ref TimeOnly value)
    {
        if (bytes.Length < TomlCodes.DateTime.LocalTimeFormatLength)
            ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();

        // local time
        if (!(TomlCodes.IsColon(bytes[2]) && TomlCodes.IsColon(bytes[5])))
            ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();

        DeserializeUnsafe(bytes, ref value);
    }

    private void SerializeCore<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, TimeOnly value, ReadOnlySpan<char> format)
        where TBufferWriter : IBufferWriter<byte>
    {
        value.TryFormat(writer.GetWrittenSpan(format.Length), out var bytesWritten, format);
    }

    internal void DeserializeUnsafe(ReadOnlySpan<byte> bytes, ref TimeOnly value)
    {
        var hour = DeserializeDecimal(bytes[0]) * 10 + DeserializeDecimal(bytes[1]);
        var minute = DeserializeDecimal(bytes[3]) * 10 + DeserializeDecimal(bytes[4]);
        var second = DeserializeDecimal(bytes[6]) * 10 + DeserializeDecimal(bytes[7]);

        // millisecond and microsecond is 0 ~ 999
        // https://learn.microsoft.com/en-us/dotnet/api/system.datetime.-ctor?view=net-8.0#system-datetime-ctor(system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32)
        var millisecond = 0;
        var microsecond = 0;
        if (bytes.Length > 8 && TomlCodes.IsDot(bytes[8]))
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
            value =  new TimeOnly(hour, minute, second, millisecond, microsecond);
        }
        catch (ArgumentOutOfRangeException e)
        {
            ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<TimeOnly>(e);
        }
    }

    private int DeserializeDecimal(byte utf8Byte)
    {
        if (!TomlCodes.IsNumber(utf8Byte))
        {
            ExceptionHelper.ThrowNumericConversionFailed(utf8Byte);
        }
        return TomlCodes.Number.ParseDecimal(utf8Byte);
    }
}

