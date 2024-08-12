using CsToml.Formatter;
using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Text.Unicode;

namespace CsToml.Values;

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlDotKey :
    TomlValue,
    ITomlStringCreator<TomlDotKey>,
    IEquatable<TomlDotKey?>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private byte[] bytes;

    public override bool HasValue => true;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public CsTomlStringType TomlStringType { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public ReadOnlySpan<byte> Value => bytes.AsSpan();

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public string Utf16String
    {
        get
        {
            var tempReader = new Utf8Reader(Value);
            ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out string value);
            return value;
        }
    }

    public static TomlDotKey CreateString(ReadOnlySpan<byte> value, CsTomlStringType type = CsTomlStringType.Basic)
    {
        return new TomlDotKey(value, type);
    }

    public TomlDotKey(ReadOnlySpan<byte> value, CsTomlStringType type = CsTomlStringType.Basic) : base()
    {
        bytes = value.ToArray();
        TomlStringType = type;
    }

    public override string ToString()
        => Utf16String;

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        var status = Utf8.ToUtf16(Value, destination, out var bytesRead, out charsWritten, replaceInvalidSequences: false);
        return status == OperationStatus.Done;
    }

    public override string ToString(string? format, IFormatProvider? formatProvider)
        => GetString();

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (utf8Destination.Length < Value.Length)
        {
            bytesWritten = 0;
            return false;
        }

        Value.TryCopyTo(utf8Destination);
        bytesWritten = Value.Length;
        return true;
    }

    internal override bool ToTomlString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer)
    {
        switch (TomlStringType)
        {
            case CsTomlStringType.Basic:
                return TomlString.ToTomlBasicString(ref writer, Value);
            case CsTomlStringType.MultiLineBasic:
                return TomlString.ToTomlMultiLineBasicString(ref writer, Value);
            case CsTomlStringType.Literal:
                return TomlString.ToTomlLiteralString(ref writer, Value);
            case CsTomlStringType.MultiLineLiteral:
                return TomlString.ToTomlMultiLineLiteralString(ref writer, Value);
            case CsTomlStringType.Unquoted:
                {
                    if (Value.Length > 0)
                    {
                        writer.Write(Value);
                        return true;
                    }
                    break;
                }
        }

        return false;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != typeof(TomlDotKey)) return false;

        return Equals((TomlDotKey)obj);
    }

    public bool Equals(TomlDotKey? other)
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
    public static string GetJoinName(this ReadOnlySpan<TomlDotKey> key)
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            using var _ = bufferWriter;
            var writer = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref bufferWriter);

            for (int i = 0; i < key.Length; i++)
            {
                key[i].ToTomlString(ref writer);
                if (i < key.Length - 1)
                    writer.Write(TomlCodes.Symbol.DOT);
            }

            var tempReader = new Utf8Reader(bufferWriter.WrittenSpan);
            ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out string value);
            return value;
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }
}