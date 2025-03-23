using CsToml.Error;
using System.Diagnostics;
using System.Globalization;

namespace CsToml.Values;

[DebuggerDisplay("{Value}")]
internal sealed partial class TomlFloat(double value) : TomlValue
{
    public static readonly TomlFloat Inf = new(TomlCodes.Float.Inf);
    public static readonly TomlFloat NInf = new (TomlCodes.Float.NInf);
    public static readonly TomlFloat Nan = new (TomlCodes.Float.Nan);
    public static readonly TomlFloat PNan = new(TomlCodes.Float.Nan);

    public double Value { get; private set; } = value;

    public override bool HasValue => true;

    public override TomlValueType Type => TomlValueType.Float;

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        writer.WriteDouble(Value);
    }

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (format.Length == 0 && provider == null)
        {
            return Value.TryFormat(destination, out charsWritten, format, CultureInfo.InvariantCulture);
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
            return Value.TryFormat(utf8Destination, out bytesWritten, format, CultureInfo.InvariantCulture);
        }

        return Value.TryFormat(utf8Destination, out bytesWritten, format, provider);
    }

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    public static TomlFloat Parse(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 3) ExceptionHelper.ThrowIncorrectTomlFloatFormat();

        if (double.TryParse(bytes, CultureInfo.InvariantCulture, out var value))
        {
            return new TomlFloat(value);
        }

        ExceptionHelper.ThrowIncorrectTomlFloatFormat();
        return default!;
    }
}

