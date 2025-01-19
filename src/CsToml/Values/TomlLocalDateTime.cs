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

    public override string ToString()
        => GetString();

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        => Value.TryFormat(destination, out charsWritten, format, provider);

    public override string ToString(string? format, IFormatProvider? formatProvider)
        => Value.ToString(format, formatProvider);

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        => Value.TryFormat(utf8Destination, out bytesWritten, format, provider);

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
