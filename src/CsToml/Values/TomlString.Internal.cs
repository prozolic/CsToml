using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;

namespace CsToml.Values;

internal partial class TomlString
{
    internal static TomlDotKey ParseKey(ReadOnlySpan<byte> utf16String)
    {
        if (Utf8Helper.ContainInvalidSequences(utf16String))
            ExceptionHelper.ThrowInvalidCodePoints();

        var barekey = false;
        var backslash = false;
        var singleQuoted = false;
        var doubleQuoted = false;
        for (int i = 0; i < utf16String.Length; i++)
        {
            switch(utf16String[i])
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
            return new TomlDotKey(utf16String, CsTomlStringType.Unquoted);
        }

        if (backslash && !singleQuoted)
        {
            return new TomlDotKey(utf16String, CsTomlStringType.Basic);
        }

        if (doubleQuoted && !singleQuoted)
        {
            return new TomlDotKey(utf16String, CsTomlStringType.Literal);
        }

        if (Utf8Helper.ContainsEscapeChar(utf16String, true))
        {
            return new TomlDotKey(utf16String, CsTomlStringType.Literal);
        }
        return new TomlDotKey(utf16String, CsTomlStringType.Basic);
    }

    internal static TomlDotKey ParseKey(ReadOnlySpan<char> utf16String)
    {
        var writer = new ArrayPoolBufferWriter<byte>(128);
        using var _ = writer;
        var utf8Writer = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref writer);
        ValueFormatter.Serialize(ref utf8Writer, utf16String);

        return ParseKey(writer.WrittenSpan);
    }

    internal static TomlString Parse(ReadOnlySpan<byte> utf16String)
    {
        if (Utf8Helper.ContainInvalidSequences(utf16String))
            ExceptionHelper.ThrowInvalidCodePoints();

        // check newline
        if (utf16String.Contains(TomlCodes.Symbol.LINEFEED))
        {
            if (Utf8Helper.ContainsEscapeChar(utf16String, true))
            {
                return new TomlString(utf16String, CsTomlStringType.MultiLineLiteral);
            }
            return new TomlString(utf16String, CsTomlStringType.MultiLineBasic);
        }

        // check escape
        if (Utf8Helper.ContainsEscapeChar(utf16String, true))
        {
            return new TomlString(utf16String, CsTomlStringType.Literal);
        }

        if (utf16String.Contains(TomlCodes.Symbol.BACKSLASH) && !utf16String.Contains(TomlCodes.Symbol.SINGLEQUOTED))
        {
            return new TomlString(utf16String, CsTomlStringType.Basic);
        }

        if (utf16String.Contains(TomlCodes.Symbol.DOUBLEQUOTED))
        {
            if (!utf16String.Contains(TomlCodes.Symbol.SINGLEQUOTED))
            {
                return new TomlString(utf16String, CsTomlStringType.Literal);
            }
            return new TomlString(utf16String, CsTomlStringType.MultiLineLiteral);
        }

        return new TomlString(utf16String, CsTomlStringType.Basic);
    }

    internal static TomlString Parse(ReadOnlySpan<char> utf16String)
    {
        if (utf16String.Length == 0) return new TomlString(string.Empty, CsTomlStringType.Basic);

        var writer = new ArrayPoolBufferWriter<byte>(128);
        using var _ = writer;
        var utf8Writer = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref writer);
        ValueFormatter.Serialize(ref utf8Writer, utf16String);

        return Parse(writer.WrittenSpan);
    }
}

