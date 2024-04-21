using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("{Value}")]
internal partial class CsTomlBool(bool value) :
    CsTomlValue(CsTomlType.Boolean), 
    IEquatable<CsTomlBool?>
{
    public bool Value { get; private set; } = value;

    internal override bool ToTomlString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer)
    {
        ValueFormatter.Serialize(ref writer, Value);
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != typeof(CsTomlBool)) return false;

        return Equals((CsTomlBool)obj);
    }

    public bool Equals(CsTomlBool? other)
    {
        if (other == null) return false;

        return Value.Equals(other.Value);
    }

    public override int GetHashCode()
        => Value ? 1 : 0;

    public override string ToString()
        => GetString();

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        var destinationSize = Value ? 4 : 5;
        if (destination.Length < destinationSize)
        {
            charsWritten = 0;
            return false;
        }

        GetString().TryCopyTo(destination);
        charsWritten = destinationSize;
        return true;
    }

    public override string ToString(string? format, IFormatProvider? formatProvider)
    {
        Span<char> destination = Value ? stackalloc char[4] : stackalloc char[5];
        TryFormat(destination, out var charsWritten, format.AsSpan(), formatProvider);
        return destination.ToString();
    }

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (Value)
        {
            if (utf8Destination.Length < 4)
            {
                bytesWritten = 0;
                return false;
            }
            utf8Destination[0] = CsTomlSyntax.AlphaBet.T;
            utf8Destination[1] = CsTomlSyntax.AlphaBet.r;
            utf8Destination[2] = CsTomlSyntax.AlphaBet.u;
            utf8Destination[3] = CsTomlSyntax.AlphaBet.e;
            bytesWritten = 4;
        }
        else
        {
            if (utf8Destination.Length < 5)
            {
                bytesWritten = 0;
                return false;
            }
            utf8Destination[0] = CsTomlSyntax.AlphaBet.F;
            utf8Destination[1] = CsTomlSyntax.AlphaBet.a;
            utf8Destination[2] = CsTomlSyntax.AlphaBet.l;
            utf8Destination[3] = CsTomlSyntax.AlphaBet.s;
            utf8Destination[4] = CsTomlSyntax.AlphaBet.e;
            bytesWritten = 5;
        }
        return true;
    }

}

