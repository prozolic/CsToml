using CsToml.Error;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("{Value}")]
internal sealed partial class TomlLocalTime(TimeOnly value) : TomlValue
{
    public TimeOnly Value { get; private set; } = value;

    public override bool HasValue => true;

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        writer.WriteTimeOnly(Value);
    }

    public override string ToString()
        => GetString();

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        => Value.TryFormat(destination, out charsWritten, format, provider);

    public override string ToString(string? format, IFormatProvider? formatProvider)
        => Value.ToString(format, formatProvider);

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        => Value.TryFormat(utf8Destination, out bytesWritten, format, provider);

    public static TomlLocalTime Parse(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < TomlCodes.DateTime.LocalTimeFormatLength)
            ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();

        // local time
        if (!(TomlCodes.IsColon(bytes[2]) && TomlCodes.IsColon(bytes[5])))
            ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();

        return new TomlLocalTime(ParseUnsafe(bytes));
    }

    public static TimeOnly ParseUnsafe(ReadOnlySpan<byte> bytes)
    {
        var hour = ParseDecimal(bytes[0]) * 10 + ParseDecimal(bytes[1]);
        var minute = ParseDecimal(bytes[3]) * 10 + ParseDecimal(bytes[4]);
        var second = ParseDecimal(bytes[6]) * 10 + ParseDecimal(bytes[7]);

        // millisecond and microsecond is 0 ~ 999
        // https://learn.microsoft.com/en-us/dotnet/api/system.datetime.-ctor?view=net-8.0#system-datetime-ctor(system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32)
        var millisecond = 0;
        var microsecond = 0;
        if (bytes.Length > 8 && TomlCodes.IsDot(bytes[8]))
        {
            if (bytes.Length == 10)
            {
                millisecond = ParseDecimal(bytes[9]) * 100;
            }
            else if (bytes.Length == 11)
            {
                millisecond += ParseDecimal(bytes[9]) * 100;
                millisecond += ParseDecimal(bytes[10]) * 10;
            }
            else if (bytes.Length == 12)
            {
                millisecond += ParseDecimal(bytes[9]) * 100;
                millisecond += ParseDecimal(bytes[10]) * 10;
                millisecond += ParseDecimal(bytes[11]);
            }
            else if (bytes.Length == 13)
            {
                millisecond += ParseDecimal(bytes[9]) * 100;
                millisecond += ParseDecimal(bytes[10]) * 10;
                millisecond += ParseDecimal(bytes[11]);
                microsecond += ParseDecimal(bytes[12]) * 100;
            }
            else if (bytes.Length == 14)
            {
                millisecond += ParseDecimal(bytes[9]) * 100;
                millisecond += ParseDecimal(bytes[10]) * 10;
                millisecond += ParseDecimal(bytes[11]);
                microsecond += ParseDecimal(bytes[12]) * 100;
                microsecond += ParseDecimal(bytes[13]) * 10;
            }
            else if (bytes.Length >= 15)
            {
                millisecond += ParseDecimal(bytes[9]) * 100;
                millisecond += ParseDecimal(bytes[10]) * 10;
                millisecond += ParseDecimal(bytes[11]);
                microsecond += ParseDecimal(bytes[12]) * 100;
                microsecond += ParseDecimal(bytes[13]) * 10;
                microsecond += ParseDecimal(bytes[14]);
            }
        }

        try
        {
            return new TimeOnly(hour, minute, second, millisecond, microsecond);
        }
        catch (ArgumentOutOfRangeException e)
        {
            ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<TimeOnly>(e);
            return default;
        }
    }

    private static int ParseDecimal(byte utf8Byte)
    {
        if (!TomlCodes.IsNumber(utf8Byte))
        {
            ExceptionHelper.ThrowNumericConversionFailed(utf8Byte);
        }
        return TomlCodes.Number.ParseDecimal(utf8Byte);
    }
}
