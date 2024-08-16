using CsToml.Error;
using CsToml.Utility;
using System.Buffers;

namespace CsToml.Formatter;

internal class DateOnlyFormatter : ITomlValueFormatter<DateOnly>
{
    public static readonly DateOnlyFormatter Default = new DateOnlyFormatter();

    public void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, DateOnly value)
        where TBufferWriter : IBufferWriter<byte>
    {
        value.TryFormat(writer.GetWrittenSpan(TomlCodes.DateTime.LocalDateFormatLength), out int bytesWritten, "yyyy-MM-dd");
    }

    public void Deserialize(ReadOnlySpan<byte> bytes, ref DateOnly value)
    {
        if (bytes.Length != TomlCodes.DateTime.LocalDateFormatLength)
            ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();

        // local date
        if (!(TomlCodes.IsHyphen(bytes[4]) && TomlCodes.IsHyphen(bytes[7])))
        {
            ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        }

        DeserializeUnsafe(bytes, ref value);
    }

    internal void DeserializeUnsafe(ReadOnlySpan<byte> bytes, ref DateOnly value)
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
            value = new DateOnly(year, month, day);
        }
        catch (ArgumentOutOfRangeException e)
        {
            ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<DateOnly>(e);
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

