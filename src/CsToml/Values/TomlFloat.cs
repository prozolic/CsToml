using CsToml.Error;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("{Value}")]
internal sealed partial class TomlFloat(double value, TomlFloat.FloatKind kind = TomlFloat.FloatKind.Normal) : TomlValue
{
    public readonly static TomlFloat Inf = new(TomlCodes.Float.Inf, FloatKind.Inf);
    public readonly static TomlFloat NInf = new (TomlCodes.Float.NInf, FloatKind.NInf);
    public readonly static TomlFloat Nan = new (TomlCodes.Float.Nan, FloatKind.Nan);
    public readonly static TomlFloat PNan = new(TomlCodes.Float.Nan, FloatKind.PNan);

    internal enum FloatKind : byte
    {
        Normal,
        Inf,
        NInf,
        Nan,
        PNan
    }

    public double Value { get; private set; } = value;

    public override bool HasValue => true;

    internal FloatKind Kind { get; } = kind;

    internal override bool ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        if (Kind == FloatKind.Normal)
        {
            writer.WriteDouble(Value);
            return true;
        }

        switch(Kind)
        {
            case FloatKind.Inf:
                writer.WriteBytes("inf"u8);
                break;
            case FloatKind.NInf:
                writer.WriteBytes("-inf"u8);
                break;
            case FloatKind.Nan:
                writer.WriteBytes("nan"u8);
                break;
            case FloatKind.PNan:
                writer.WriteBytes("-nan"u8);
                break;
            default:
                return false;
        }
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


    public static TomlFloat Parse(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 3) ExceptionHelper.ThrowIncorrectTomlFloatFormat();

        if (double.TryParse(bytes, out var value))
        {
            return new TomlFloat(value);
        }

        ExceptionHelper.ThrowIncorrectTomlFloatFormat();
        return default!;
    }
}

