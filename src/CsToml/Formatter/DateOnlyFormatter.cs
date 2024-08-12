using CsToml.Error;
using CsToml.Utility;
using System.Buffers;

namespace CsToml.Formatter;

internal class DateOnlyFormatter : ICsTomlFormatter<DateOnly>
{
    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, DateOnly value)
        where TBufferWriter : IBufferWriter<byte>
    {
        value.TryFormat(writer.GetWrittenSpan(TomlCodes.DateTime.LocalDateFormatLength), out int bytesWritten, "yyyy-MM-dd");
    }

    public static DateOnly Deserialize(ref Utf8Reader reader, int length)
    {
        var bytes = reader.ReadBytes(length);

        if (bytes.Length != TomlCodes.DateTime.LocalDateFormatLength)
            ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();

        // local date
        if (!(TomlCodes.IsHyphen(bytes[4]) && TomlCodes.IsHyphen(bytes[7])))
        {
            ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        }

        return DeserializeDateOnly(bytes);
    }

    internal static DateOnly DeserializeDateOnly(ReadOnlySpan<byte> bytes)
    {
        var year = DeserializeDecimal(bytes[0]) * 1000;
        year += DeserializeDecimal(bytes[1]) * 100;
        year += DeserializeDecimal(bytes[2]) * 10;
        year += DeserializeDecimal(bytes[3]);

        var month = DeserializeDecimal(bytes[5]) * 10;
        month += DeserializeDecimal(bytes[6]);

        var day = DeserializeDecimal(bytes[8]) * 10;
        day += DeserializeDecimal(bytes[9]);

        try
        {
            return new DateOnly(year, month, day);
        }
        catch (ArgumentOutOfRangeException e)
        {
            return ExceptionHelper.NotReturnThrow<DateOnly, ArgumentOutOfRangeException>(
                ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<DateOnly>, e);
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

