using CsToml.Error;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml.Values;

[DebuggerDisplay("{Value}")]
internal sealed partial class TomlLocalTime(TimeOnly value) : TomlValue
{
    public TimeOnly Value { get; private set; } = value;

    public override bool HasValue => true;

    public override TomlValueType Type => TomlValueType.LocalTime;

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        writer.WriteTimeOnly(Value);
    }

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (format.Length == 0 && provider == null)
        {
            return Value.TryFormat(destination, out charsWritten, "HH:mm:ss.fffffff", provider);
        }
        return Value.TryFormat(destination, out charsWritten, format, provider);
    }

    public override string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format) && formatProvider == null)
        {
            return ToString();
        }
        return Value.ToString(format, formatProvider);
    }

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (format.Length == 0 && provider == null)
        {
            return Value.TryFormat(utf8Destination, out bytesWritten, "HH:mm:ss.fffffff", provider);
        }
        return Value.TryFormat(utf8Destination, out bytesWritten, format, provider);
    }

    public override string ToString() => Value.ToString("HH:mm:ss.fffffff");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TomlLocalTime Parse(ReadOnlySpan<byte> bytes)
    {
        return new TomlLocalTime(ParseTimeOnly(bytes));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TomlLocalTime ParseToOmitSeconds(ReadOnlySpan<byte> bytes)
    {
        return new TomlLocalTime(ParseTimeOnlyToOmitSeconds(bytes));
    }

    internal static TimeOnly ParseTimeOnly(ReadOnlySpan<byte> bytes)
    {
        var hour = TomlCodes.Number.ParseDecimal(bytes[0]) * 10 + TomlCodes.Number.ParseDecimal(bytes[1]);
        var minute = TomlCodes.Number.ParseDecimal(bytes[3]) * 10 + TomlCodes.Number.ParseDecimal(bytes[4]);
        var second = TomlCodes.Number.ParseDecimal(bytes[6]) * 10 + TomlCodes.Number.ParseDecimal(bytes[7]);

        // millisecond and microsecond is 0 ~ 999
        // https://learn.microsoft.com/en-us/dotnet/api/system.datetime.-ctor?view=net-8.0#system-datetime-ctor(system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32)
        var millisecond = 0;
        var microsecond = 0;
        if (bytes.Length > 8 && TomlCodes.IsDot(bytes[8]))
        {
            if (bytes.Length == 10)
            {
                millisecond = TomlCodes.Number.ParseDecimal(bytes[9]) * 100;
            }
            else if (bytes.Length == 11)
            {
                millisecond += TomlCodes.Number.ParseDecimal(bytes[9]) * 100;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[10]) * 10;
            }
            else if (bytes.Length == 12)
            {
                millisecond += TomlCodes.Number.ParseDecimal(bytes[9]) * 100;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[10]) * 10;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[11]);
            }
            else if (bytes.Length == 13)
            {
                millisecond += TomlCodes.Number.ParseDecimal(bytes[9]) * 100;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[10]) * 10;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[11]);
                microsecond += TomlCodes.Number.ParseDecimal(bytes[12]) * 100;
            }
            else if (bytes.Length == 14)
            {
                millisecond += TomlCodes.Number.ParseDecimal(bytes[9]) * 100;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[10]) * 10;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[11]);
                microsecond += TomlCodes.Number.ParseDecimal(bytes[12]) * 100;
                microsecond += TomlCodes.Number.ParseDecimal(bytes[13]) * 10;
            }
            else if (bytes.Length >= 15)
            {
                millisecond += TomlCodes.Number.ParseDecimal(bytes[9]) * 100;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[10]) * 10;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[11]);
                microsecond += TomlCodes.Number.ParseDecimal(bytes[12]) * 100;
                microsecond += TomlCodes.Number.ParseDecimal(bytes[13]) * 10;
                microsecond += TomlCodes.Number.ParseDecimal(bytes[14]);
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

    internal static TimeOnly ParseTimeOnlyToOmitSeconds(ReadOnlySpan<byte> bytes)
    {
        var hour = TomlCodes.Number.ParseDecimal(bytes[0]) * 10 + TomlCodes.Number.ParseDecimal(bytes[1]);
        var minute = TomlCodes.Number.ParseDecimal(bytes[3]) * 10 + TomlCodes.Number.ParseDecimal(bytes[4]);
        var second = 0;

        // millisecond and microsecond is 0 ~ 999
        // https://learn.microsoft.com/en-us/dotnet/api/system.datetime.-ctor?view=net-8.0#system-datetime-ctor(system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-int32)
        var millisecond = 0;
        var microsecond = 0;
        if (bytes.Length > 5 && TomlCodes.IsDot(bytes[5]))
        {
            if (bytes.Length == 7)
            {
                millisecond = TomlCodes.Number.ParseDecimal(bytes[6]) * 100;
            }
            else if (bytes.Length == 8)
            {
                millisecond += TomlCodes.Number.ParseDecimal(bytes[6]) * 100;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[7]) * 10;
            }
            else if (bytes.Length == 9)
            {
                millisecond += TomlCodes.Number.ParseDecimal(bytes[6]) * 100;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[7]) * 10;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[8]);
            }
            else if (bytes.Length == 10)
            {
                millisecond += TomlCodes.Number.ParseDecimal(bytes[6]) * 100;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[7]) * 10;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[8]);
                microsecond += TomlCodes.Number.ParseDecimal(bytes[9]) * 100;
            }
            else if (bytes.Length == 11)
            {
                millisecond += TomlCodes.Number.ParseDecimal(bytes[6]) * 100;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[7]) * 10;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[8]);
                microsecond += TomlCodes.Number.ParseDecimal(bytes[9]) * 100;
                microsecond += TomlCodes.Number.ParseDecimal(bytes[10]) * 10;
            }
            else if (bytes.Length >= 12)
            {
                millisecond += TomlCodes.Number.ParseDecimal(bytes[6]) * 100;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[7]) * 10;
                millisecond += TomlCodes.Number.ParseDecimal(bytes[8]);
                microsecond += TomlCodes.Number.ParseDecimal(bytes[9]) * 100;
                microsecond += TomlCodes.Number.ParseDecimal(bytes[10]) * 10;
                microsecond += TomlCodes.Number.ParseDecimal(bytes[11]);
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
}
