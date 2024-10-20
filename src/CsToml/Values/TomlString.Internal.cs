using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;
using System.Buffers;
using System.Text.Unicode;

namespace CsToml.Values;

internal partial class TomlString
{
    public static TomlString Parse(ReadOnlySpan<byte> utf16String)
    {
        if (Utf8Helper.ContainInvalidSequences(utf16String))
            ExceptionHelper.ThrowInvalidCodePoints();

        // check newline
        if (utf16String.Contains(TomlCodes.Symbol.LINEFEED))
        {
            if (Utf8Helper.ContainsEscapeChar(utf16String, true))
            {
                return TomlString.Parse(utf16String, CsTomlStringType.MultiLineLiteral);
            }
            return TomlString.Parse(utf16String, CsTomlStringType.MultiLineBasic);
        }

        // check escape
        if (Utf8Helper.ContainsEscapeChar(utf16String, true))
        {
            return TomlString.Parse(utf16String, CsTomlStringType.Literal);
        }

        if (utf16String.Contains(TomlCodes.Symbol.BACKSLASH) && !utf16String.Contains(TomlCodes.Symbol.SINGLEQUOTED))
        {
            return TomlString.Parse(utf16String, CsTomlStringType.Basic);
        }

        if (utf16String.Contains(TomlCodes.Symbol.DOUBLEQUOTED))
        {
            if (!utf16String.Contains(TomlCodes.Symbol.SINGLEQUOTED))
            {
                return TomlString.Parse(utf16String, CsTomlStringType.Literal);
            }
            return TomlString.Parse(utf16String, CsTomlStringType.MultiLineLiteral);
        }

        return TomlString.Parse(utf16String, CsTomlStringType.Basic);
    }

    public static TomlString Parse(ReadOnlySpan<char> utf16String)
    {
        if (utf16String.Length == 0)
            return TomlBasicString.Empty;

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

    public static TomlString Parse(ReadOnlySpan<byte> bytes, CsTomlStringType type)
    {
        if (bytes.Length == 0)
        {
            switch(type)
            {
                case CsTomlStringType.Basic:
                    return TomlBasicString.Empty;
                case CsTomlStringType.MultiLineBasic:
                    return TomlMultiLineBasicString.Empty;
                case CsTomlStringType.Literal:
                    return TomlLiteralString.Empty;
                case CsTomlStringType.MultiLineLiteral:
                    return TomlMultiLineLiteralString.Empty;
                case CsTomlStringType.Unquoted:
                    return TomlUnquotedString.Empty;
            }
        }
        switch (type)
        {
            case CsTomlStringType.Basic:
                return new TomlBasicString(Utf8Helper.ToUtf16(bytes));
            case CsTomlStringType.MultiLineBasic:
                return new TomlMultiLineBasicString(Utf8Helper.ToUtf16(bytes));
            case CsTomlStringType.Literal:
                return new TomlLiteralString(Utf8Helper.ToUtf16(bytes));
            case CsTomlStringType.MultiLineLiteral:
                return new TomlMultiLineLiteralString(Utf8Helper.ToUtf16(bytes));
            case CsTomlStringType.Unquoted:
                return new TomlUnquotedString(Utf8Helper.ToUtf16(bytes));
        }
        ExceptionHelper.ThrowIncorrectTomlStringFormat();
        return default;
    }
}

