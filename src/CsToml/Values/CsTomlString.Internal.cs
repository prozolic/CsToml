using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;
using System.Buffers;

namespace CsToml.Values;

internal partial class CsTomlString
{
    internal enum EscapeSequenceResult : byte
    {
        Success,
        Failure,
        Unescaped
    }

    public static CsTomlString ParseKey(ReadOnlySpan<byte> utf16String)
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
                case CsTomlSyntax.Symbol.BACKSLASH:
                    backslash = true;
                    break;
                case CsTomlSyntax.Symbol.SINGLEQUOTED:
                    singleQuoted = true;
                    break;
                case CsTomlSyntax.Symbol.DOUBLEQUOTED:
                    doubleQuoted = true;
                    break;
                default:
                    barekey = CsTomlSyntax.IsBareKey(utf16String[i]);
                    break;
            }
        }
        if (barekey)
        {
            return new CsTomlString(utf16String, CsTomlStringType.Unquoted);
        }

        if (backslash && !singleQuoted)
        {
            return new CsTomlString(utf16String, CsTomlStringType.Basic);
        }

        if (doubleQuoted && !singleQuoted)
        {
            return new CsTomlString(utf16String, CsTomlStringType.Literal);
        }

        if (Utf8Helper.ContainsEscapeChar(utf16String, true))
        {
            return new CsTomlString(utf16String, CsTomlStringType.Literal);
        }
        return new CsTomlString(utf16String, CsTomlStringType.Basic);
    }

    public static CsTomlString ParseKey(ReadOnlySpan<char> utf16String)
    {
        using var writer = new ArrayPoolBufferWriter<byte>(128);
        var utf8Writer = new Utf8Writer(writer);
        ValueFormatter.Serialize(ref utf8Writer, utf16String);

        return ParseKey(writer.WrittenSpan);
    }

    public static CsTomlString Parse(ReadOnlySpan<byte> utf16String)
    {
        if (Utf8Helper.ContainInvalidSequences(utf16String))
            ExceptionHelper.ThrowInvalidCodePoints();

        // check newline
        if (utf16String.Contains(CsTomlSyntax.Symbol.LINEFEED))
        {
            if (Utf8Helper.ContainsEscapeChar(utf16String, true))
            {
                return new CsTomlString(utf16String, CsTomlStringType.MultiLineLiteral);
            }
            return new CsTomlString(utf16String, CsTomlStringType.MultiLineBasic);
        }

        // check escape
        if (Utf8Helper.ContainsEscapeChar(utf16String, true))
        {
            return new CsTomlString(utf16String, CsTomlStringType.Literal);
        }

        if (utf16String.Contains(CsTomlSyntax.Symbol.BACKSLASH) && !utf16String.Contains(CsTomlSyntax.Symbol.SINGLEQUOTED))
        {
            return new CsTomlString(utf16String, CsTomlStringType.Basic);
        }

        if (utf16String.Contains(CsTomlSyntax.Symbol.DOUBLEQUOTED))
        {
            if (!utf16String.Contains(CsTomlSyntax.Symbol.SINGLEQUOTED))
            {
                return new CsTomlString(utf16String, CsTomlStringType.Literal);
            }
            return new CsTomlString(utf16String, CsTomlStringType.MultiLineLiteral);
        }

        return new CsTomlString(utf16String, CsTomlStringType.Basic);
    }

    public static CsTomlString Parse(ReadOnlySpan<char> utf16String)
    {
        using var writer = new ArrayPoolBufferWriter<byte>(128);
        var utf8Writer = new Utf8Writer(writer);
        ValueFormatter.Serialize(ref utf8Writer, utf16String);

        return Parse(writer.WrittenSpan);
    }

    public static EscapeSequenceResult TryFormatEscapeSequence(ref Utf8Reader reader, ref Utf8Writer utf8Writer, bool multiLine, bool throwError)
    {
        if (!reader.TryPeek(out var ch)) ExceptionHelper.ThrowEndOfFileReached();

        if (CsTomlSyntax.IsEscapeSequence(ch))
        {
            switch (ch)
            {
                case CsTomlSyntax.Symbol.DOUBLEQUOTED:
                    utf8Writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
                    goto BREAK;
                case CsTomlSyntax.Symbol.SINGLEQUOTED:
                    utf8Writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
                    goto BREAK;
                case CsTomlSyntax.Symbol.BACKSLASH:
                    utf8Writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    goto BREAK;
                case CsTomlSyntax.AlphaBet.b:
                    utf8Writer.Write(CsTomlSyntax.Symbol.BACKSPACE);
                    goto BREAK;
                case CsTomlSyntax.AlphaBet.t:
                    utf8Writer.Write(CsTomlSyntax.Symbol.TAB);
                    goto BREAK;
                case CsTomlSyntax.AlphaBet.n:
                    utf8Writer.Write(CsTomlSyntax.Symbol.LINEFEED);
                    goto BREAK;
                case CsTomlSyntax.AlphaBet.f:
                    utf8Writer.Write(CsTomlSyntax.Symbol.FORMFEED);
                    goto BREAK;
                case CsTomlSyntax.AlphaBet.r:
                    utf8Writer.Write(CsTomlSyntax.Symbol.CARRIAGE);
                    goto BREAK;
                case CsTomlSyntax.AlphaBet.u:
                    reader.Advance(1);
                    try
                    {
                        WriteAfterParsingFrom16bitCodePointToUtf8(ref reader, ref utf8Writer);
                    }
                    catch (CsTomlException)
                    {
                        if (throwError) throw;
                        return EscapeSequenceResult.Failure;
                    }
                    return EscapeSequenceResult.Success;
                case CsTomlSyntax.AlphaBet.U:
                    reader.Advance(1);
                    try
                    {
                        WriteAfterParsingFrom32bitCodePointToUtf8(ref reader, ref utf8Writer);
                    }
                    catch (CsTomlException)
                    {
                        if (throwError) throw;
                        return EscapeSequenceResult.Failure;
                    }
                    return EscapeSequenceResult.Success;
                default:
                    return EscapeSequenceResult.Failure;
            }
        }
        else
        {
            return multiLine ? EscapeSequenceResult.Unescaped : EscapeSequenceResult.Failure;
        }

    BREAK:
        reader.Advance(1);
        return EscapeSequenceResult.Success;
    }

    public static EscapeSequenceResult TryFormatEscapeSequence(ref Utf8SequenceReader reader, ref Utf8Writer utf8Writer, bool multiLine, bool throwError)
    {
        if (!reader.TryPeek(out var ch)) ExceptionHelper.ThrowEndOfFileReached();

        if (CsTomlSyntax.IsEscapeSequence(ch))
        {
            switch (ch)
            {
                case CsTomlSyntax.Symbol.DOUBLEQUOTED:
                    utf8Writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
                    goto BREAK;
                case CsTomlSyntax.Symbol.SINGLEQUOTED:
                    utf8Writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
                    goto BREAK;
                case CsTomlSyntax.Symbol.BACKSLASH:
                    utf8Writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    goto BREAK;
                case CsTomlSyntax.AlphaBet.b:
                    utf8Writer.Write(CsTomlSyntax.Symbol.BACKSPACE);
                    goto BREAK;
                case CsTomlSyntax.AlphaBet.t:
                    utf8Writer.Write(CsTomlSyntax.Symbol.TAB);
                    goto BREAK;
                case CsTomlSyntax.AlphaBet.n:
                    utf8Writer.Write(CsTomlSyntax.Symbol.LINEFEED);
                    goto BREAK;
                case CsTomlSyntax.AlphaBet.f:
                    utf8Writer.Write(CsTomlSyntax.Symbol.FORMFEED);
                    goto BREAK;
                case CsTomlSyntax.AlphaBet.r:
                    utf8Writer.Write(CsTomlSyntax.Symbol.CARRIAGE);
                    goto BREAK;
                case CsTomlSyntax.AlphaBet.u:
                    reader.Advance(1);
                    try
                    {
                        WriteAfterParsingFrom16bitCodePointToUtf8(ref reader, ref utf8Writer);
                    }
                    catch (CsTomlException)
                    {
                        if (throwError) throw;
                        return EscapeSequenceResult.Failure;
                    }
                    return EscapeSequenceResult.Success;
                case CsTomlSyntax.AlphaBet.U:
                    reader.Advance(1);
                    try
                    {
                        WriteAfterParsingFrom32bitCodePointToUtf8(ref reader, ref utf8Writer);
                    }
                    catch (CsTomlException)
                    {
                        if (throwError) throw;
                        return EscapeSequenceResult.Failure;
                    }
                    return EscapeSequenceResult.Success;
                default:
                    return EscapeSequenceResult.Failure;
            }
        }
        else
        {
            return multiLine ? EscapeSequenceResult.Unescaped : EscapeSequenceResult.Failure;
        }

    BREAK:
        reader.Advance(1);
        return EscapeSequenceResult.Success;
    }

    private static void WriteAfterParsingFrom16bitCodePointToUtf8(ref Utf8Reader reader, ref Utf8Writer utf8Writer)
    {
        if (reader.Length < reader.Position + 4)
            ExceptionHelper.ThrowIncorrect16bitCodePoint();

        Span<byte> destination = stackalloc byte[4];
        var source = reader.ReadBytes(4);

        Utf8Helper.ParseFrom16bitCodePointToUtf8(destination, source, out int writtenCount);
        for (int i = 0; i < writtenCount; i++)
        {
            utf8Writer.Write(destination[i]);
        }
    }

    private static void WriteAfterParsingFrom32bitCodePointToUtf8(ref Utf8Reader reader, ref Utf8Writer utf8Writer)
    {
        if (reader.Length < reader.Position + 8)
            ExceptionHelper.ThrowIncorrect32bitCodePoint();

        Span<byte> destination = stackalloc byte[4];
        var source = reader.ReadBytes(8);

        Utf8Helper.ParseFrom32bitCodePointToUtf8(destination, source, out int writtenCount);
        for (int i = 0; i < writtenCount; i++)
        {
            utf8Writer.Write(destination[i]);
        }
    }

    private static void WriteAfterParsingFrom16bitCodePointToUtf8(ref Utf8SequenceReader reader, ref Utf8Writer utf8Writer)
    {
        if (reader.Length < reader.Consumed + 4)
            ExceptionHelper.ThrowIncorrect16bitCodePoint();

        var length = 4;
        Span<byte> destination = stackalloc byte[length];
        //var source = reader.ReadBytes(4);

        if (reader.TryFullSpan(length, out var bytes))
        {
            Utf8Helper.ParseFrom16bitCodePointToUtf8(destination, bytes, out int writtenCount);
            for (int i = 0; i < writtenCount; i++)
            {
                utf8Writer.Write(destination[i]);
            }
        }
        else
        {
            var rent = RecycleByteArrayPoolBufferWriter.Rent();
            try
            {
                reader.TryGetbytes(length, rent);
                Utf8Helper.ParseFrom16bitCodePointToUtf8(destination, rent.WrittenSpan, out int writtenCount);
                for (int i = 0; i < writtenCount; i++)
                {
                    utf8Writer.Write(destination[i]);
                }
            }
            finally
            {
                RecycleByteArrayPoolBufferWriter.Return(rent);
            }
        }
    }

    private static void WriteAfterParsingFrom32bitCodePointToUtf8(ref Utf8SequenceReader reader, ref Utf8Writer utf8Writer)
    {
        if (reader.Length < reader.Consumed + 8)
            ExceptionHelper.ThrowIncorrect32bitCodePoint();

        Span<byte> destination = stackalloc byte[4];

        var length = 8;
        if (reader.TryFullSpan(length, out var bytes))
        {
            Utf8Helper.ParseFrom32bitCodePointToUtf8(destination, bytes, out int writtenCount);
            for (int i = 0; i < writtenCount; i++)
            {
                utf8Writer.Write(destination[i]);
            }
        }
        else
        {
            var rent = RecycleByteArrayPoolBufferWriter.Rent();
            try
            {
                reader.TryGetbytes(length, rent);
                Utf8Helper.ParseFrom32bitCodePointToUtf8(destination, rent.WrittenSpan, out int writtenCount);
                for (int i = 0; i < writtenCount; i++)
                {
                    utf8Writer.Write(destination[i]);
                }
            }
            finally
            {
                RecycleByteArrayPoolBufferWriter.Return(rent);
            }
        }
    }

}

