
using CsToml.Error;
using CsToml.Utility;

namespace CsToml.Values;

internal static class TomlDottedKeyHelper
{
    public static TomlDottedKey ParseKey(ReadOnlySpan<byte> utf8String, bool supportsEscapeSequenceE, bool supportsEscapeSequenceX)
    {
        var keyType = TomlDottedKeyHelper.GetTomlKeyType(utf8String, supportsEscapeSequenceE, supportsEscapeSequenceX);
        switch (keyType)
        {
            case TomlStringType.Unquoted:
                return new TomlUnquotedDottedKey(utf8String);
            case TomlStringType.Basic:
                return new TomlBasicDottedKey(utf8String);
            case TomlStringType.Literal:
                return new TomlLiteralDottedKey(utf8String);
            default:
                return new TomlLiteralDottedKey(utf8String);
        }
    }

    public static TomlDottedKey ParseKey(ReadOnlySpan<char> utf16String, bool supportsEscapeSequenceE, bool supportsEscapeSequenceX)
    {
        var writer = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            Utf8Helper.FromUtf16(writer, utf16String);
            return ParseKey(writer.WrittenSpan, supportsEscapeSequenceE, supportsEscapeSequenceX);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(writer);
        }
    }

    public static TomlDottedKey ParseKeyForPrimitive<T>(T value, bool supportsEscapeSequenceE, bool supportsEscapeSequenceX)
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            var documentWriter = new Utf8TomlDocumentWriter<ArrayPoolBufferWriter<byte>>(ref bufferWriter);
            documentWriter.WriteKeyForPrimitive(value);
            return ParseKey(bufferWriter.WrittenSpan, supportsEscapeSequenceE, supportsEscapeSequenceX);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }
    public static TomlStringType GetTomlKeyType(ReadOnlySpan<byte> utf8String, bool supportsEscapeSequenceE, bool supportsEscapeSequenceX)
    {
        if (Utf8Helper.ContainInvalidSequences(utf8String))
            ExceptionHelper.ThrowInvalidCodePoints();

        var barekey = true;
        var backslashCount = 0L;
        var escapeSequenceCount = 0L;
        var singleQuoted = false;
        var doubleQuoted = false;

        var sequenceReader = new Utf8SequenceReader(utf8String);
        while (sequenceReader.TryPeek(out var ch))
        {
            switch (ch)
            {
                case TomlCodes.Symbol.BACKSLASH:
                    backslashCount++;
                    sequenceReader.Advance(1);
                    if (sequenceReader.TryPeek(out var ch2))
                    {
                        switch (ch2)
                        {
                            case TomlCodes.Symbol.DOUBLEQUOTED:
                            case TomlCodes.Symbol.BACKSLASH:
                            case TomlCodes.Alphabet.b:
                            case TomlCodes.Alphabet.t:
                            case TomlCodes.Alphabet.n:
                            case TomlCodes.Alphabet.f:
                            case TomlCodes.Alphabet.r:
                                escapeSequenceCount++;
                                sequenceReader.Advance(1);
                                continue;
                            case TomlCodes.Alphabet.u:
                                sequenceReader.Advance(1);
                                if (sequenceReader.TryFullSpan(4, out var value))
                                {
                                    if (!TomlCodes.IsHex(value[0])) continue;
                                    if (!TomlCodes.IsHex(value[1])) continue;
                                    if (!TomlCodes.IsHex(value[2])) continue;
                                    if (!TomlCodes.IsHex(value[3])) continue;
                                    sequenceReader.Advance(4);
                                    escapeSequenceCount++;
                                    continue;
                                }
                                continue;
                            case TomlCodes.Alphabet.U:
                                sequenceReader.Advance(1);
                                if (sequenceReader.TryFullSpan(8, out var value2))
                                {
                                    if (!TomlCodes.IsHex(value2[0])) continue;
                                    if (!TomlCodes.IsHex(value2[1])) continue;
                                    if (!TomlCodes.IsHex(value2[2])) continue;
                                    if (!TomlCodes.IsHex(value2[3])) continue;
                                    if (!TomlCodes.IsHex(value2[4])) continue;
                                    if (!TomlCodes.IsHex(value2[5])) continue;
                                    if (!TomlCodes.IsHex(value2[6])) continue;
                                    if (!TomlCodes.IsHex(value2[7])) continue;
                                    sequenceReader.Advance(8);
                                    escapeSequenceCount++;
                                    continue;
                                }
                                continue;
                            case TomlCodes.Alphabet.e:
                                if (supportsEscapeSequenceE) // TOML v1.1.0
                                {
                                    sequenceReader.Advance(1);
                                    escapeSequenceCount++;
                                    continue;
                                }
                                continue;
                            case TomlCodes.Alphabet.x:
                                if (supportsEscapeSequenceX) // TOML v1.1.0
                                {
                                    sequenceReader.Advance(1);
                                    escapeSequenceCount++;
                                    continue;
                                }
                                continue;
                            default:
                                continue;
                        }
                    }
                    break;
                case TomlCodes.Symbol.SINGLEQUOTED:
                    singleQuoted = true;
                    sequenceReader.Advance(1);
                    break;
                case TomlCodes.Symbol.DOUBLEQUOTED:
                    doubleQuoted = true;
                    sequenceReader.Advance(1);
                    break;
                default:
                    if (barekey)
                    {
                        barekey = TomlCodes.IsBareKey(ch);
                    }
                    sequenceReader.Advance(1);
                    break;
            }
        }

        if (barekey)
        {
            if (utf8String.Length > 0)
            {
                return TomlStringType.Unquoted;
            }
            else
            {
                return TomlStringType.Basic;
            }
        }

        // If the key contains escape sequences or singleQuoted, it is basic string,
        if ((backslashCount == escapeSequenceCount && escapeSequenceCount > 0) || singleQuoted)
        {
            return TomlStringType.Basic;
        }

        // If the key contains backslash or doubleQuoted, it is literal string,
        if (backslashCount > 0 || (doubleQuoted && !singleQuoted) || Utf8Helper.ContainsEscapeChar(utf8String, false))
        {
            return TomlStringType.Literal;
        }

        return TomlStringType.Basic;
    }
}