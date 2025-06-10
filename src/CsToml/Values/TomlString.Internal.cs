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

    public static TomlStringType GetTomlStringType(ReadOnlySpan<byte> utf8String)
    {
        if (Utf8Helper.ContainInvalidSequences(utf8String))
            ExceptionHelper.ThrowInvalidCodePoints();

        // check newline
        if (utf8String.Contains(TomlCodes.Symbol.LINEFEED))
        {
            if (Utf8Helper.ContainsEscapeChar(utf8String, true))
            {
                return TomlStringType.MultiLineLiteral;
            }
            return TomlStringType.MultiLineBasic;
        }

        // check escape
        if (Utf8Helper.ContainsEscapeChar(utf8String, true))
        {
            return TomlStringType.Literal;
        }

        if (utf8String.Contains(TomlCodes.Symbol.BACKSLASH) && !utf8String.Contains(TomlCodes.Symbol.SINGLEQUOTED))
        {
            return TomlStringType.Basic;
        }

        if (utf8String.Contains(TomlCodes.Symbol.DOUBLEQUOTED))
        {
            if (!utf8String.Contains(TomlCodes.Symbol.SINGLEQUOTED))
            {
                return TomlStringType.Literal;
            }
            return TomlStringType.MultiLineLiteral;
        }
        return TomlStringType.Basic;
    }
}