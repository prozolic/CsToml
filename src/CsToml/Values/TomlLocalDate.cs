using CsToml.Error;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("{Value}")]
internal sealed partial class TomlLocalDate(DateOnly value) : TomlValue
{
    public DateOnly Value { get; private set; } = value;

    public override bool HasValue => true;

    internal override bool ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        writer.WriteDateOnly(Value);
        return true;
    }

    public override string ToString()
        => GetString();

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        => Value.TryFormat(destination, out charsWritten, format, provider);

    public override string ToString(string? format, IFormatProvider? formatProvider)
        => Value.ToString(format, formatProvider);

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        => Value.TryFormat(utf8Destination, out bytesWritten, format, provider);

    public static TomlLocalDate Parse(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != TomlCodes.DateTime.LocalDateFormatLength)
            ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();

        // local date
        if (!(TomlCodes.IsHyphen(bytes[4]) && TomlCodes.IsHyphen(bytes[7])))
        {
            ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        }

        return new TomlLocalDate(ParseUnsafe(bytes));
    }

    public static DateOnly ParseUnsafe(ReadOnlySpan<byte> bytes)
    {
        var year = ParseDecimalByte(bytes[0]) * 1000;
        year += ParseDecimalByte(bytes[1]) * 100;
        year += ParseDecimalByte(bytes[2]) * 10;
        year += ParseDecimalByte(bytes[3]);

        var month = ParseDecimalByte(bytes[5]) * 10;
        month += ParseDecimalByte(bytes[6]);

        var day = ParseDecimalByte(bytes[8]) * 10;
        day += ParseDecimalByte(bytes[9]);

        try
        {
            return new DateOnly(year, month, day);
        }
        catch (ArgumentOutOfRangeException e)
        {
            ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<DateOnly>(e);
            return default;
        }
    }

    private static int ParseDecimalByte(byte utf8Byte)
    {
        if (!TomlCodes.IsNumber(utf8Byte))
        {
            ExceptionHelper.ThrowNumericConversionFailed(utf8Byte);
        }
        return TomlCodes.Number.ParseDecimal(utf8Byte);
    }

}
