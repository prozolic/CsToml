
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlInt: {Value}")]
internal partial class CsTomlInt64(long value) 
    : CsTomlValue(CsTomlType.Integar), IEquatable<CsTomlInt64?>
{
    public long Value { get; private set; } = value;

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        Int64Formatter.Serialize(ref writer, Value);
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != typeof(CsTomlInt64)) return false;

        return Equals((CsTomlInt64)obj);
    }

    public bool Equals(CsTomlInt64? other)
    {
        if (other == null) return false;

        return Value.Equals(other.Value);
    }

    public override int GetHashCode()
        => Value.GetHashCode();
}

