using CsToml.Formatter;
using CsToml.Utility;
using System;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlBool: {Value}")]
internal partial class CsTomlBool(bool value) 
    : CsTomlValue(CsTomlType.Boolean), IEquatable<CsTomlBool?>
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
}

