using CsToml.Formatter;
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
        => Value.SequenceEqual(other);

    public override int GetHashCode()
        => ByteArrayHash.ToInt32(Value);
}

internal static class CsTomlDotKeyExtensions
{
    public static void Recycle(this ArrayPoolList<CsTomlDotKey> key)
    {
        if (key.IsRent)
        {
            RecycleArrayPoolList<CsTomlDotKey>.Return(key);
        }
    }

    public static string GetJoinName(this ReadOnlySpan<CsTomlDotKey> key)
    {
        var bufferWriter = new ArrayPoolBufferWriter<byte>();
        using var _ = bufferWriter;
        var writer = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref bufferWriter);

        for (int i = 0; i < key.Length; i++)
        {
            key[i].ToTomlString(ref writer);
            if (i < key.Length - 1)
                writer.Write(CsTomlSyntax.Symbol.PERIOD);
        }

        var tempReader = new Utf8Reader(bufferWriter.WrittenSpan);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out string value);
        return value;
    }
}