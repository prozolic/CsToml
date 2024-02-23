using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlDouble: {Value}")]
internal partial class CsTomlDouble(double value, CsTomlDouble.DoubleKind kind = CsTomlDouble.DoubleKind.Normal) : 
    CsTomlValue(CsTomlType.Float),
    IEquatable<CsTomlDouble?>
{
    public readonly static CsTomlDouble Inf = new(CsTomlSyntax.Double.Inf, DoubleKind.Inf);
    public readonly static CsTomlDouble NInf = new (CsTomlSyntax.Double.NInf, DoubleKind.NInf);
    public readonly static CsTomlDouble Nan = new (CsTomlSyntax.Double.Nan, DoubleKind.Nan);
    public readonly static CsTomlDouble PNan = new(CsTomlSyntax.Double.Inf, DoubleKind.PNan);

    internal enum DoubleKind : byte
    {
        Normal,
        Inf,
        NInf,
        Nan,
        PNan
    }

    public double Value { get; private set; } = value;

    internal DoubleKind Kind { get; } = kind;

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        if (Kind == DoubleKind.Normal)
        {
            DoubleFormatter.Serialize(ref writer, Value);
            return true;
        }

        switch(Kind)
        {
            case DoubleKind.Inf:
                writer.Write("inf"u8);
                break;
            case DoubleKind.NInf:
                writer.Write("-inf"u8);
                break;
            case DoubleKind.Nan:
                writer.Write("nan"u8);
                break;
            case DoubleKind.PNan:
                writer.Write("-nan"u8);
                break;
            default:
                return false;
        }
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != typeof(CsTomlDouble)) return false;

        return Equals((CsTomlDouble)obj);
    }

    public bool Equals(CsTomlDouble? other)
    {
        if (other == null) return false;

        return Value.Equals(other.Value);
    }

    public override int GetHashCode()
        => Value.GetHashCode();

    public override string ToString()
        => GetString();

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        => Value.TryFormat(destination, out charsWritten, format, provider);

    public override string ToString(string? format, IFormatProvider? formatProvider)
        => Value.ToString(format, formatProvider);

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        => Value.TryFormat(utf8Destination, out bytesWritten, format, provider);

}

