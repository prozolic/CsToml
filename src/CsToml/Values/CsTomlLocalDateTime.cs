
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlLocalDateTime: {Value}")]
internal class CsTomlLocalDateTime(DateTime value)
    : CsTomlValue(CsTomlType.LocalDateTime), IEquatable<CsTomlLocalDateTime?>
{
    public DateTime Value { get; set; } = value;

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
}
