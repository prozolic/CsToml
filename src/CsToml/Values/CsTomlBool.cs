using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlBool: {Value}")]
internal partial class CsTomlBool(bool value) :
    CsTomlValue(CsTomlType.Boolean), 
    IEquatable<CsTomlBool?>,
    ISpanFormattable
{
    public bool Value { get; private set; } = value;

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        BoolFormatter.Serialize(ref writer, Value);
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

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        var destinationSize = Value ? 4 : 5;
        if (destination.Length < destinationSize)
        {
            charsWritten = 0;
            return false;
        }

        (Value ? bool.TrueString : bool.FalseString).TryCopyTo(destination);
        charsWritten = destinationSize;
        return true;
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        Span<char> destination = Value ? stackalloc char[4] : stackalloc char[5];
        TryFormat(destination, out var charsWritten, format.AsSpan(), formatProvider);
        return destination.ToString();
    }

}

