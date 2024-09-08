using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Text.Unicode;

namespace CsToml.Values;

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlDottedKey :
    TomlValue,
    ITomlStringParser<TomlDottedKey>,
    IEquatable<TomlDottedKey?>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private byte[] bytes;

    public override bool HasValue => true;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public CsTomlStringType TomlStringType { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public ReadOnlySpan<byte> Value => bytes.AsSpan();

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public string Utf16String => Utf8Helper.ToUtf16(Value);

    public static TomlDottedKey Parse(ReadOnlySpan<byte> value, CsTomlStringType type)
    {
        return new TomlDottedKey(value, type);
    }

    public static TomlDottedKey ParseKey(ReadOnlySpan<byte> utf16String)
    {
        if (Utf8Helper.ContainInvalidSequences(utf16String))
            ExceptionHelper.ThrowInvalidCodePoints();

        var barekey = false;
        var backslash = false;
        var singleQuoted = false;
        var doubleQuoted = false;
        for (int i = 0; i < utf16String.Length; i++)
        {
            switch (utf16String[i])
            {
                case TomlCodes.Symbol.BACKSLASH:
                    backslash = true;
                    break;
                case TomlCodes.Symbol.SINGLEQUOTED:
                    singleQuoted = true;
                    break;
                case TomlCodes.Symbol.DOUBLEQUOTED:
                    doubleQuoted = true;
                    break;
                default:
                    barekey = TomlCodes.IsBareKey(utf16String[i]);
                    break;
            }
        }
        if (barekey)
        {
            return new TomlDottedKey(utf16String, CsTomlStringType.Unquoted);
        }

        if (backslash && !singleQuoted)
        {
            return new TomlDottedKey(utf16String, CsTomlStringType.Basic);
        }

        if (doubleQuoted && !singleQuoted)
        {
            return new TomlDottedKey(utf16String, CsTomlStringType.Literal);
        }

        if (Utf8Helper.ContainsEscapeChar(utf16String, true))
        {
            return new TomlDottedKey(utf16String, CsTomlStringType.Literal);
        }
        return new TomlDottedKey(utf16String, CsTomlStringType.Basic);
    }

    public static TomlDottedKey ParseKey(ReadOnlySpan<char> utf16String)
    {
        var writer = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            Utf8Helper.FromUtf16(writer, utf16String);
            return ParseKey(writer.WrittenSpan);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(writer);
        }
    }

    public TomlDottedKey(ReadOnlySpan<byte> value, CsTomlStringType type = CsTomlStringType.Basic) : base()
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

    internal override bool ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
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
                        writer.WriteBytes(Value);
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
        if (obj.GetType() != typeof(TomlDottedKey)) return false;

        return Equals((TomlDottedKey)obj);
    }

    public bool Equals(TomlDottedKey? other)
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
    public static string GetJoinName(this ReadOnlySpan<TomlDottedKey> key)
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            var writer = new Utf8TomlDocumentWriter<ArrayPoolBufferWriter<byte>>(ref bufferWriter);

            for (int i = 0; i < key.Length; i++)
            {
                key[i].ToTomlString(ref writer);
                if (i < key.Length - 1)
                    writer.Write(TomlCodes.Symbol.DOT);
            }

            return Utf8Helper.ToUtf16(bufferWriter.WrittenSpan); ;
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }
}