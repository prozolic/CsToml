using CsToml.Error;
using CsToml.Utility;
using System.Runtime.CompilerServices;

namespace CsToml.Values;

internal static class TomlDottedKeyHelper
{
    public static TomlDottedKey ParseBareKey(ReadOnlySpan<byte> utf8String, bool skipValidation)
    {
        if (utf8String.Length == 0)
            ExceptionHelper.ThrowBareKeyIsEmpty();

        if (!skipValidation && Utf8Helper.ContainInvalidSequences(utf8String))
            ExceptionHelper.ThrowInvalidCodePoints();

        return new TomlUnquotedDottedKey(utf8String);
    }

    public static TomlDottedKey ParseBareKey(byte[] utf8String, bool skipValidation)
    {
        if (utf8String.Length == 0)
            ExceptionHelper.ThrowBareKeyIsEmpty();

        if (!skipValidation && Utf8Helper.ContainInvalidSequences(utf8String))
            ExceptionHelper.ThrowInvalidCodePoints();

        return new TomlUnquotedDottedKey(utf8String);
    }

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

    public static TomlDottedKey ParseKeyForPrimitive<T>(T value, CsTomlSerializerOptions options)
    {
        InlineArray4<TomlDottedKey> initialKeys = default;
        ref InlineArray4<TomlDottedKey> initialKeysRef = ref Unsafe.AsRef(in initialKeys);
        TempList<TomlDottedKey> keyList = new(initialKeysRef);

        InlineArray4<(TomlValueState state, int dottedKeyIndex)> initialStates = default;
        ref InlineArray4<(TomlValueState state, int dottedKeyIndex)> initialStatesRef = ref Unsafe.AsRef(in initialStates);
        TempList<(TomlValueState state, int dottedKeyIndex)> stateList = new(initialStatesRef);

        var documentWriter = new Utf8TomlDocumentWriter(new ArrayPoolBufferSource(1024), in keyList, in stateList, false, options);
        try
        {
            documentWriter.WriteKeyForPrimitive(value);
            return new TomlUnquotedDottedKey(documentWriter.WrittenSpan);
        }
        finally
        {
            documentWriter.Dispose();
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

        for (var i = 0; i < utf8String.Length; i++)
        {
            var ch = utf8String[i];
            switch (ch)
            {
                case TomlCodes.Symbol.BACKSLASH:
                    barekey = false;
                    backslashCount++;
                    var next = i + 1;
                    if ((uint)next >= (uint)utf8String.Length)
                        break;

                    switch (utf8String[next])
                    {
                        case TomlCodes.Symbol.DOUBLEQUOTED:
                        case TomlCodes.Symbol.BACKSLASH:
                        case TomlCodes.Alphabet.b:
                        case TomlCodes.Alphabet.t:
                        case TomlCodes.Alphabet.n:
                        case TomlCodes.Alphabet.f:
                        case TomlCodes.Alphabet.r:
                            escapeSequenceCount++;
                            i = next;
                            break;
                        case TomlCodes.Alphabet.u:
                            i = next;
                            var value = utf8String.Slice(next + 1);
                            if (value.Length >= 4 &&
                                TomlCodes.IsHex(value[0]) && TomlCodes.IsHex(value[1]) && TomlCodes.IsHex(value[2]) && TomlCodes.IsHex(value[3]))
                            {
                                i += 4;
                                escapeSequenceCount++;
                            }
                            break;
                        case TomlCodes.Alphabet.U:
                            i = next;
                            var value2 = utf8String.Slice(next + 1);
                            if (value2.Length >= 8 &&
                                TomlCodes.IsHex(value2[0]) && TomlCodes.IsHex(value2[1]) && TomlCodes.IsHex(value2[2]) && TomlCodes.IsHex(value2[3]) &&
                                TomlCodes.IsHex(value2[4]) && TomlCodes.IsHex(value2[5]) && TomlCodes.IsHex(value2[6]) && TomlCodes.IsHex(value2[7]))
                            {
                                i += 8;
                                escapeSequenceCount++;
                            }
                            break;
                        case TomlCodes.Alphabet.e:
                            if (supportsEscapeSequenceE)
                            {
                                escapeSequenceCount++;
                                i = next;
                            }
                            break;
                        case TomlCodes.Alphabet.x:
                            if (supportsEscapeSequenceX)
                            {
                                escapeSequenceCount++;
                                i = next;
                            }
                            break;
                    }
                    break;
                case TomlCodes.Symbol.SINGLEQUOTED:
                    barekey = false;
                    singleQuoted = true;
                    break;
                case TomlCodes.Symbol.DOUBLEQUOTED:
                    barekey = false;
                    doubleQuoted = true;
                    break;
                default:
                    if (barekey)
                    {
                        barekey = TomlCodes.IsBareKey(ch);
                    }
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

        // If the key contains escape sequences, singleQuoted or control characters, it is basic string,
        if ((backslashCount == escapeSequenceCount && escapeSequenceCount > 0) || singleQuoted || Utf8Helper.ContainsEscapeChar(utf8String))
        {
            return TomlStringType.Basic;
        }

        // If the key contains backslash or doubleQuoted, it is literal string,
        if (backslashCount > 0 || (doubleQuoted && !singleQuoted))
        {
            return TomlStringType.Literal;
        }

        return TomlStringType.Basic;
    }
}