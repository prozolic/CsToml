using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("{Utf16String}")]
internal sealed class CsTomlDotKey :
    CsTomlString, 
    IEquatable<CsTomlDotKey?>
{
    public CsTomlDotKey(ReadOnlySpan<byte> value, CsTomlStringType type = CsTomlStringType.Basic) : base(value, type)
    {}

    public CsTomlDotKey(byte[] value, CsTomlStringType type = CsTomlStringType.Basic) : base(value, type)
    {}

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != typeof(CsTomlDotKey)) return false;

        return Equals((CsTomlDotKey)obj);
    }

    public bool Equals(CsTomlDotKey? other)
    {
        if (other == null) return false;

        return Equals(other.Value);
    }

    public bool Equals(ReadOnlySpan<byte> other)
    {
        if (Length != other.Length) return false;
        if (Length == 0) return true;

        return Value.SequenceEqual(other);
    }

    public override int GetHashCode()
        => ByteArrayHash.ToInt32(Value);
}

