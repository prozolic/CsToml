using CsToml.Error;
using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Text.Unicode;

namespace CsToml.Values;

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlUnquotedDottedKey(ReadOnlySpan<byte> value) : TomlDottedKey(value)
{
    internal override bool ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        if (Value.Length > 0)
        {
            writer.WriteBytes(Value);
            return true;
        }
        return false;
    }
}

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlBasicDottedKey(ReadOnlySpan<byte> value) : TomlDottedKey(value)
{
    internal override bool ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        return TomlBasicString.ToTomlBasicString(ref writer, Value);
    }
}

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlLiteralDottedKey(ReadOnlySpan<byte> value) : TomlDottedKey(value)
{
    internal override bool ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        return TomlLiteralString.ToTomlLiteralString(ref writer, Value);
    }
}

[DebuggerDisplay("{Utf16String}")]
internal abstract partial class TomlDottedKey(ReadOnlySpan<byte> value) :
    TomlValue(),
    ITomlStringParser<TomlDottedKey>,
    IEquatable<TomlDottedKey?>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private byte[] bytes = value.ToArray();

    public override bool HasValue => true;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public ReadOnlySpan<byte> Value => bytes.AsSpan();

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public string Utf16String => Utf8Helper.ToUtf16(Value);

    public static TomlDottedKey Parse(ReadOnlySpan<byte> value, CsTomlStringType type)
    {
        switch(type)
        {
            case CsTomlStringType.Unquoted:
                return new TomlUnquotedDottedKey(value);
            case CsTomlStringType.Basic:
                return new TomlBasicDottedKey(value);
            case CsTomlStringType.Literal:
                return new TomlLiteralDottedKey(value);
        }
        ExceptionHelper.ThrowIncorrectTomlStringFormat();
        return default;
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
            return new TomlUnquotedDottedKey(utf16String);
        }

        if (backslash && !singleQuoted)
        {
            return new TomlBasicDottedKey(utf16String);
        }

        if (doubleQuoted && !singleQuoted)
        {
            return new TomlLiteralDottedKey(utf16String);
        }

        if (Utf8Helper.ContainsEscapeChar(utf16String, true))
        {
            return new TomlLiteralDottedKey(utf16String);
        }
        return new TomlBasicDottedKey(utf16String);
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

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;

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