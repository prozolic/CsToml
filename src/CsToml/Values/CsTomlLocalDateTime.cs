using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlLocalDateTime: {Value}")]
internal partial class CsTomlLocalDateTime(DateTime value) : 
    CsTomlValue(CsTomlType.LocalDateTime),
    IEquatable<CsTomlLocalDateTime?>,
    ISpanFormattable
{
    public DateTime Value { get; private set; } = value;

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        DateTimeFormatter.Serialize(ref writer, Value);
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != typeof(CsTomlLocalDateTime)) return false;

        return Equals((CsTomlLocalDateTime)obj);
    }

    public bool Equals(CsTomlLocalDateTime? other)
    {
        if (other == null) return false;

        return Value.Equals(other.Value);
    }

    public override int GetHashCode()
        => Value.GetHashCode();

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => Value.TryFormat(destination, out charsWritten, format, provider);

    public string ToString(string? format, IFormatProvider? formatProvider)
        => Value.ToString(format, formatProvider);

}
