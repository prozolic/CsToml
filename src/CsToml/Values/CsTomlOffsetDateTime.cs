﻿
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlOffsetDateTime: {Value}")]
internal class CsTomlOffsetDateTime(DateTimeOffset value, bool byNumber) 
    : CsTomlValue(byNumber ? CsTomlType.OffsetDateTimeByNumber : CsTomlType.OffsetDateTime)
    , IEquatable<CsTomlOffsetDateTime?>
{
    public DateTimeOffset Value { get; set; } = value;

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
}
