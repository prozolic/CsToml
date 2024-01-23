
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlLocalTime: {Value}")]
internal partial class CsTomlLocalTime(TimeOnly value)
    : CsTomlValue(CsTomlType.LocalTime), IEquatable<CsTomlLocalTime?>
{
    public TimeOnly Value { get; private set; } = value;

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        TimeOnlyFormatter.Serialize(ref writer, Value);
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != typeof(CsTomlLocalTime)) return false;

        return Equals((CsTomlLocalTime)obj);
    }

    public bool Equals(CsTomlLocalTime? other)
    {
        if (other == null) return false;

        return Value.Equals(other.Value);
    }

    public override int GetHashCode()
        => Value.GetHashCode();
}
