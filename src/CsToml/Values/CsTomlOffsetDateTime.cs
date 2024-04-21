using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("{Value}")]
internal partial class CsTomlOffsetDateTime(DateTimeOffset value, bool byNumber) : 
    CsTomlValue(byNumber ? CsTomlType.OffsetDateTimeByNumber : CsTomlType.OffsetDateTime), 
    IEquatable<CsTomlOffsetDateTime?>
{
    public DateTimeOffset Value { get; private set; } = value;

    internal override bool ToTomlString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer)
    {
        ValueFormatter.Serialize(ref writer, Value);
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

    public override string ToString()
        => GetString();

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        => Value.TryFormat(destination, out charsWritten, format, provider);

    public override string ToString(string? format, IFormatProvider? formatProvider)
        => Value.ToString(format, formatProvider);

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        => Value.TryFormat(utf8Destination, out bytesWritten, format, provider);

}
