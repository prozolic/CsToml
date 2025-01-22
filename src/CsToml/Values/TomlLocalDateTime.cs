using CsToml.Error;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("{Value}")]
internal sealed partial class TomlLocalDateTime(DateTime value) : TomlValue
{
    public DateTime Value { get; private set; } = value;

    public override bool HasValue => true;

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        writer.WriteDateTime(Value);
    }

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (format.Length == 0 && provider == null)
        {
            return Value.TryFormat(destination, out charsWritten, "yyyy-MM-ddTHH:mm:ss.fffffff", provider);
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
            return Value.TryFormat(utf8Destination, out bytesWritten, "yyyy-MM-ddTHH:mm:ss.fffffff", provider);
        }
        return Value.TryFormat(utf8Destination, out bytesWritten, format, provider);
    }

    public override string ToString() => Value.ToString("yyyy-MM-ddTHH:mm:ss.fffffff"); // Time zone information ('zzz') is not included.

    public static TomlLocalDateTime Parse(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < TomlCodes.DateTime.LocalDateTimeFormatLength)
            ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();

        if (TomlCodes.IsHyphen(bytes[4]) && TomlCodes.IsHyphen(bytes[7]) &&
            (TomlCodes.IsTabOrWhiteSpace(bytes[10]) || bytes[10] == TomlCodes.Alphabet.T || bytes[10] == TomlCodes.Alphabet.t))
        {
            return new TomlLocalDateTime(DeserializeLocalDateTime(bytes));
        }

        ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        return default!;
    }

    private static DateTime DeserializeLocalDateTime(ReadOnlySpan<byte> bytes)
    {
        DateOnly localDate = TomlLocalDate.ParseUnsafe(bytes[..10]);
        TimeOnly localTime = TomlLocalTime.ParseUnsafe(bytes[11..]);
        try
        {
            return new DateTime(localDate, localTime, DateTimeKind.Local);
        }
        catch (ArgumentOutOfRangeException e)
        {
            ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<DateTime>(e);
            return default!;
        }
    }
}
