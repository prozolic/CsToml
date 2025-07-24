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

        var escapeChar = false;
        var backslashCount = 0L;
        var escapeSequenceCount = 0L;
        var singleQuoted = false;
        var doubleQuoted = false;
        var newline = false;

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
                    break;
                case TomlCodes.Symbol.DOUBLEQUOTED:
                    doubleQuoted = true;
                    break;
                case TomlCodes.Symbol.LINEFEED:
                    newline = true;
                    break;
                case TomlCodes.Symbol.CARRIAGE:
                    sequenceReader.Advance(1);
                    if (sequenceReader.TryPeek(out var linebreakCh) && TomlCodes.IsLf(linebreakCh))
                    {
                        newline = true;
                    }
                    break;
                default:
                    if (!escapeChar)
                    {
                        escapeChar = TomlCodes.IsEscape(ch);
                    }
                    break;
            }

            sequenceReader.Advance(1);
        }

        // check newline
        if (newline)
        {
            if (Utf8Helper.ContainsEscapeChar(utf8String, true))
            {
                return TomlStringType.MultiLineLiteral;
            }
            return TomlStringType.MultiLineBasic;
        }

        // If the key contains escape sequences or singleQuoted, it is basic string.
        if ((backslashCount == escapeSequenceCount && escapeSequenceCount > 0) || singleQuoted)
        {
            return TomlStringType.Basic;
        }

        // If the key contains backslash or doubleQuoted, it is literal string.
        if (backslashCount > 0 || (doubleQuoted && !singleQuoted) || Utf8Helper.ContainsEscapeChar(utf8String, false))
        {
            return TomlStringType.Literal;
        }

        return TomlStringType.Basic;
    }
}