using CsToml.Error;
using CsToml.Utility;

namespace CsToml.Values;

internal partial class CsTomlString
{
    internal enum EscapeSequenceResult : byte
    {
        Success,
        Failure,
        Unescaped
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

