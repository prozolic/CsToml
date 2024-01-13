
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlLocalDate: {Value}")]
internal class CsTomlLocalDate(DateOnly value)
    : CsTomlValue(CsTomlType.LocalDate), IEquatable<CsTomlLocalDate?>
{
    public DateOnly Value { get; private set; } = value;

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        DateOnlyFormatter.Serialize(ref writer, Value);
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != typeof(CsTomlLocalDate)) return false;

        return Equals((CsTomlLocalDate)obj);
    }

    public bool Equals(CsTomlLocalDate? other)
    {
        if (other == null) return false;

        return Value.Equals(other.Value);
    }

    public override int GetHashCode()
        => Value.GetHashCode();
}
