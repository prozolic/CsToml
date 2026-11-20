using CsToml.Error;
using CsToml.Utility;
using System.Runtime.CompilerServices;

namespace CsToml.Values;

internal static class TomlStringHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Parse<T>(ReadOnlySpan<byte> value)
        where T : TomlValue, ITomlStringParser<T>
    {
        return T.Parse(value);
    }

    public static TomlStringType GetTomlStringType(ReadOnlySpan<byte> utf8String, bool supportsEscapeSequenceE, bool supportsEscapeSequenceX)
    {
        if (Utf8Helper.ContainInvalidSequences(utf8String))
            ExceptionHelper.ThrowInvalidCodePoints();

        var backslashCount = 0L;
        var escapeSequenceCount = 0L;
        var singleQuoted = false;
        var doubleQuoted = false;
        var newline = false;

        for (var i = 0; i < utf8String.Length; i++)
        {
            switch (utf8String[i])
            {
                case TomlCodes.Symbol.BACKSLASH:
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
                    singleQuoted = true;
                    break;
                case TomlCodes.Symbol.DOUBLEQUOTED:
                    doubleQuoted = true;
                    break;
                case TomlCodes.Symbol.LINEFEED:
                    newline = true;
                    break;
            }
        }

        // check newline
        if (newline)
        {
            return TomlStringType.MultiLineBasic;
        }

        // If the key contains escape sequences, singleQuoted or control characters, it is basic string.
        if ((backslashCount == escapeSequenceCount && escapeSequenceCount > 0) || singleQuoted || Utf8Helper.ContainsEscapeChar(utf8String))
        {
            return TomlStringType.Basic;
        }

        // If the key contains backslash or doubleQuoted, it is literal string.
        if (backslashCount > 0 || (doubleQuoted && !singleQuoted))
        {
            return TomlStringType.Literal;
        }

        return TomlStringType.Basic;
    }
}