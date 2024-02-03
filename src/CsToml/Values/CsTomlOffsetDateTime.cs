using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlOffsetDateTime: {Value}")]
internal partial class CsTomlOffsetDateTime(DateTimeOffset value, bool byNumber) : 
    CsTomlValue(byNumber ? CsTomlType.OffsetDateTimeByNumber : CsTomlType.OffsetDateTime), 
    IEquatable<CsTomlOffsetDateTime?>,
    ISpanFormattable
{
    public DateTimeOffset Value { get; private set; } = value;

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        DateTimeOffsetFormatter.Serialize(ref writer, Value);
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != typeof(CsTomlOffsetDateTime)) return false;

        return Equals((CsTomlOffsetDateTime)obj);
    }

    public bool Equals(CsTomlOffsetDateTime? other)
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
