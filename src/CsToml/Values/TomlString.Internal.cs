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

    public static TomlString Parse(ReadOnlySpan<byte> utf16String)
    {
        if (Utf8Helper.ContainInvalidSequences(utf16String))
            ExceptionHelper.ThrowInvalidCodePoints();

        // check newline
        if (utf16String.Contains(TomlCodes.Symbol.LINEFEED))
        {
            if (Utf8Helper.ContainsEscapeChar(utf16String, true))
            {
                return TomlStringHelper.Parse<TomlMultiLineLiteralString>(utf16String);
            }

            return TomlStringHelper.Parse<TomlMultiLineBasicString>(utf16String);
        }

        // check escape
        if (Utf8Helper.ContainsEscapeChar(utf16String, true))
        {
            return TomlStringHelper.Parse<TomlLiteralString>(utf16String);
        }

        if (utf16String.Contains(TomlCodes.Symbol.BACKSLASH) && !utf16String.Contains(TomlCodes.Symbol.SINGLEQUOTED))
        {
            return TomlStringHelper.Parse<TomlBasicString>(utf16String);
        }

        if (utf16String.Contains(TomlCodes.Symbol.DOUBLEQUOTED))
        {
            if (!utf16String.Contains(TomlCodes.Symbol.SINGLEQUOTED))
            {
                return TomlStringHelper.Parse<TomlLiteralString>(utf16String);
            }
            return TomlStringHelper.Parse<TomlMultiLineLiteralString>(utf16String);
        }

        return TomlStringHelper.Parse<TomlBasicString>(utf16String);
    }

    public static TomlString Parse(ReadOnlySpan<char> utf16String)
    {
        if (utf16String.Length == 0)
            return TomlBasicString.EmptyString;

        var writer = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            Utf8Helper.FromUtf16(writer, utf16String);
            return Parse(writer.WrittenSpan);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(writer);
        }
    }
}