﻿using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml;

internal ref struct CsTomlReader
{
    private Utf8SequenceReader sequenceReader;

    public long LineNumber { get; private set; }

    [DebuggerStepThrough]
    public CsTomlReader(ref Utf8SequenceReader reader)
    {
        sequenceReader = reader;
        LineNumber = 1;
    }

    public CsTomlString ReadComment()
    {
        Advance(1); // #

        var position = sequenceReader.Consumed;

        while (TryPeek(out var ch))
        {
            if (CsTomlSyntax.IsCr(ch))
            {
                Advance(1);
                if (TryPeek(out var linebreakCh) && CsTomlSyntax.IsLf(linebreakCh))
                {
                    Rewind(1);
                    break;
                }
                ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
            }
            else if (CsTomlSyntax.IsLf(ch))
            {
                break;
            }
            else if (CsTomlSyntax.IsEscape(ch))
            {
                ExceptionHelper.ThrowNumericConversionFailed(ch);
            }
            Advance(1);
        }

        var length = sequenceReader.Consumed - position;
        Rewind(length);

        if (sequenceReader.TryFullSpan(length, out var bytes))
        {
            if (Utf8Helper.ContainInvalidSequences(bytes))
                ExceptionHelper.ThrowInvalidCodePoints();

            return new CsTomlString(bytes, CsTomlStringType.Unquoted);
        }
        else
        {
            var bufferWriter = RecycleByteArrayPoolBufferWriter.Rent();
            try
            {
                sequenceReader.TryGetbytes(length, bufferWriter);
                if (Utf8Helper.ContainInvalidSequences(bufferWriter.WrittenSpan))
                    ExceptionHelper.ThrowInvalidCodePoints();

                return new CsTomlString(bufferWriter.WrittenSpan, CsTomlStringType.Unquoted);
            }
            finally
            {
                RecycleByteArrayPoolBufferWriter.Return(bufferWriter);
            }
        }
    }

    public void ReadKey(ref ExtendableArray<CsTomlDotKey> key)
    {
        SkipWhiteSpace();
        if (!Peek())
            ExceptionHelper.ThrowEndOfFileReached();

        var dot = true;
        while (TryPeek(out var c))
        {
            if (CsTomlSyntax.IsBareKey(c))
            {
                if (!dot) ExceptionHelper.ThrowDottedKeysAreNotJoinedByDots();
                dot = false;
                key.Add(ReadKeyString(false));
                continue;
            }
            switch (c)
            {
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.SPACE:
                    SkipWhiteSpace();
                    continue;
                case CsTomlSyntax.Symbol.EQUAL:
                    if (key.Count == 0)
                    {
                        ExceptionHelper.ThrowBareKeyIsEmpty();
                    }
                    goto BREAK;
                case CsTomlSyntax.Symbol.DOT:
                    if (dot)
                    {
                        if (key.Count > 0)
                            ExceptionHelper.ThrowDotsAreUsedMoreThanOnce();
                        else
                            ExceptionHelper.ThrowTheDotIsDefinedFirst();
                    }
                    dot = true;
                    Advance(1);
                    SkipWhiteSpace();
                    continue;
                case CsTomlSyntax.Symbol.DOUBLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowIncorrectTomlFormat();
                    dot = false;
                    key.Add(ReadDoubleQuoteSingleLineString<CsTomlDotKey>());
                    continue;
                case CsTomlSyntax.Symbol.SINGLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowIncorrectTomlFormat();
                    dot = false;
                    key.Add(ReadSingleQuoteSingleLineString<CsTomlDotKey>());
                    continue;
                default:
                    ExceptionHelper.ThrowIncorrectTomlFormat();
                    break;
            }

        BREAK:
            break;
        }
    }

    public void ReadTableHeader(ref ExtendableArray<CsTomlDotKey> tableHeaderKey)
    {
        Advance(1); // [

        SkipWhiteSpace();
        if (!Peek())
            ExceptionHelper.ThrowEndOfFileReached();

        var dot = true;
        var closingRightRightSquareBracket = false;
        while (TryPeek(out var c))
        {
            if (CsTomlSyntax.IsBareKey(c))
            {
                if (!dot) ExceptionHelper.ThrowIncorrectTomlFormat();
                dot = false;
                tableHeaderKey.Add(ReadKeyString(true));
                continue;
            }
            switch (c)
            {
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.SPACE:
                    SkipWhiteSpace();
                    continue;
                case CsTomlSyntax.Symbol.DOT:
                    if (dot)
                    {
                        if (tableHeaderKey.Count > 0)
                            ExceptionHelper.ThrowDotsAreUsedMoreThanOnce();
                        else
                            ExceptionHelper.ThrowTheDotIsDefinedFirst();
                    }
                    dot = true;
                    Advance(1);
                    SkipWhiteSpace();
                    continue;
                case CsTomlSyntax.Symbol.DOUBLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowIncorrectTomlFormat();
                    dot = false;
                    tableHeaderKey.Add(ReadDoubleQuoteSingleLineString<CsTomlDotKey>());
                    continue;
                case CsTomlSyntax.Symbol.SINGLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowIncorrectTomlFormat();
                    dot = false;
                    tableHeaderKey.Add(ReadSingleQuoteSingleLineString<CsTomlDotKey>());
                    continue;
                case CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET:
                    closingRightRightSquareBracket = true;
                    Advance(1);
                    goto BREAK; // ]
                default:
                    ExceptionHelper.ThrowIncorrectTomlFormat();
                    break;
            }

        BREAK:
            break;
        }

        if (!closingRightRightSquareBracket)
            ExceptionHelper.ThrowIncorrectTomlFormat();
    }

    public void ReadArrayOfTablesHeader(ref ExtendableArray<CsTomlDotKey> arrayOfTablesHeaderKey)
    {
        Advance(2); // [[
        SkipWhiteSpace();

        if (!Peek())
            ExceptionHelper.ThrowEndOfFileReached();

        var dot = true;
        var closingRightRightSquareBracket = false;
        while (TryPeek(out var c))
        {
            if (CsTomlSyntax.IsBareKey(c))
            {
                if (!dot) ExceptionHelper.ThrowIncorrectTomlFormat();
                dot = false;
                arrayOfTablesHeaderKey.Add(ReadKeyString(true));
                continue;
            }
            switch (c)
            {
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.SPACE:
                    SkipWhiteSpace();
                    continue;
                case CsTomlSyntax.Symbol.DOT:
                    if (dot)
                    {
                        if (arrayOfTablesHeaderKey.Count > 0)
                            ExceptionHelper.ThrowDotsAreUsedMoreThanOnce();
                        else
                            ExceptionHelper.ThrowTheDotIsDefinedFirst();
                    }
                    dot = true;
                    Advance(1);
                    SkipWhiteSpace();
                    continue;
                case CsTomlSyntax.Symbol.DOUBLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowIncorrectTomlFormat();
                    dot = false;
                    arrayOfTablesHeaderKey.Add(ReadDoubleQuoteSingleLineString<CsTomlDotKey>());
                    continue;
                case CsTomlSyntax.Symbol.SINGLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowIncorrectTomlFormat();
                    dot = false;
                    arrayOfTablesHeaderKey.Add(ReadSingleQuoteSingleLineString<CsTomlDotKey>());
                    continue;
                case CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET:
                    Advance(1);
                    if (TryPeek(out var tableHeaderArrayEndCh) && CsTomlSyntax.IsRightSquareBrackets(tableHeaderArrayEndCh))
                    {
                        Advance(1);
                        closingRightRightSquareBracket = true;
                    }
                    else
                    {
                        ExceptionHelper.ThrowEndOfFileReached();
                    }
                    goto BREAK;
                default:
                    ExceptionHelper.ThrowIncorrectTomlFormat();
                    break;
            }

        BREAK:
            break;
        }

        if (!closingRightRightSquareBracket)
            ExceptionHelper.ThrowIncorrectTomlFormat();

    }

    public CsTomlValue ReadValue()
    {
        if (!TryPeek(out var c)) ExceptionHelper.ThrowEndOfFileReached();  // value is nothing

        return c switch
        {
            CsTomlSyntax.Symbol.DOUBLEQUOTED => ReadDoubleQuoteString(),
            CsTomlSyntax.Symbol.SINGLEQUOTED => ReadSingleQuoteString(),
            CsTomlSyntax.Symbol.PLUS => ReadNumbericValueIfLeadingSign(),
            CsTomlSyntax.Symbol.MINUS => ReadNumbericValueIfLeadingSign(),
            CsTomlSyntax.Number.Zero => ReadNumericValueOrDateIfLeadingZero(),
            CsTomlSyntax.Number.One => ReadNumericValueOrDate(),
            CsTomlSyntax.Number.Two => ReadNumericValueOrDate(),
            CsTomlSyntax.Number.Three => ReadNumericValueOrDate(),
            CsTomlSyntax.Number.Four => ReadNumericValueOrDate(),
            CsTomlSyntax.Number.Five => ReadNumericValueOrDate(),
            CsTomlSyntax.Number.Six => ReadNumericValueOrDate(),
            CsTomlSyntax.Number.Seven => ReadNumericValueOrDate(),
            CsTomlSyntax.Number.Eight => ReadNumericValueOrDate(),
            CsTomlSyntax.Number.Nine => ReadNumericValueOrDate(),
            CsTomlSyntax.Symbol.LEFTSQUAREBRACKET => ReadArray(),
            CsTomlSyntax.Alphabet.f => ReadBool(false),
            CsTomlSyntax.Alphabet.i => ReadDoubleInfOrNan(),
            CsTomlSyntax.Alphabet.n => ReadDoubleInfOrNan(),
            CsTomlSyntax.Alphabet.t => ReadBool(true),
            CsTomlSyntax.Symbol.LEFTBRACES => ReadInlineTable(),
            _ => ExceptionHelper.NotReturnThrow<CsTomlValue>(ExceptionHelper.ThrowIncorrectTomlFormat),
        }; ;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipWhiteSpace()
    {
        var currentSpan = sequenceReader.UnreadSpan;
        while (Peek())
        {
            for (var index = 0; index < currentSpan.Length; index++)
            {
                if (!CsTomlSyntax.IsTabOrWhiteSpace(currentSpan[index]))
                {
                    Advance(index);
                    return;
                }
            }
            Advance(currentSpan.Length);
            currentSpan = sequenceReader.CurrentSpan;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipOneLine()
    {
        while (TryPeek(out var ch))
        {
            if (TrySkipIfNewLine(ch, false))
                break;
            Advance(1);
        }
    }

    public void SkipWhiteSpaceAndNewLine()
    {
        while (TryPeek(out var ch))
        {
            switch(ch)
            {
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.SPACE:
                    Advance(1);
                    continue;
                case CsTomlSyntax.Symbol.LINEFEED:
                    Advance(1);
                    IncreaseLineNumber();
                    continue;
                case CsTomlSyntax.Symbol.CARRIAGE:
                    Advance(1);
                    if (TryPeek(out var linebreakCh) && CsTomlSyntax.IsLf(linebreakCh))
                    {
                        Advance(1);
                        IncreaseLineNumber();
                        continue;
                    }
                    return;
                default:
                    return;
            }
        }
    }

    public bool TrySkipIfNewLine(byte ch, bool throwControlCharacter)
    {
        if (CsTomlSyntax.IsLf(ch))
        {
            Advance(1);
            IncreaseLineNumber();
            return true;
        }
        else if (CsTomlSyntax.IsCr(ch))
        {
            Advance(1);
            if (TryPeek(out var linebreakCh) && CsTomlSyntax.IsLf(linebreakCh))
            {
                Advance(1);
                IncreaseLineNumber();
                return true;
            }
            if (CsTomlSyntax.IsEscape(linebreakCh))
            {
                if (throwControlCharacter)
                    ExceptionHelper.ThrowEscapeCharactersIncluded(linebreakCh);
                return false;
            }
        }

        if (CsTomlSyntax.IsEscape(ch))
        {
            if (throwControlCharacter)
                ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
        }

        return false;
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(long length)
        => sequenceReader.Advance(length);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Rewind(long length)
        => sequenceReader.Rewind(length);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryPeek(out byte ch)
        => sequenceReader.TryPeek(out ch);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Peek()
        => sequenceReader.Peek();

    private CsTomlString ReadDoubleQuoteString()
    {
        var firstPosition = sequenceReader.Consumed;
        var currentSpan = sequenceReader.UnreadSpan;
        var doubleQuoteCount = 0;
        var fullSpan = true;

        while (this.Peek())
        {
            for (var index = 0; index < currentSpan.Length; index++)
            {
                var ch = currentSpan[index];
                if (CsTomlSyntax.IsDoubleQuoted(ch))
                {
                    doubleQuoteCount++;
                }
                else
                {
                    goto BREAK;
                }
            }
            fullSpan = false;
            Advance(currentSpan.Length);
            currentSpan = sequenceReader.CurrentSpan;
        }

    BREAK:
        if (!fullSpan)
        {
            var endPosition = sequenceReader.Consumed;
            var length = endPosition - firstPosition;
            Rewind(length);
        }

        switch (doubleQuoteCount)
        {
            case 1:
            case 2:
                return ReadDoubleQuoteSingleLineString<CsTomlString>();
            case 3:
            case 4:
            case 5:
                return ReadDoubleQuoteMultiLineString();
            case 6: // first(3) + end(3) = 6 
                Advance(6);
                return new CsTomlString(string.Empty, CsTomlStringType.MultiLineBasic);
            case 7: // first(3) + one adjacent mark(1) + end(3) = 7
                Advance(7);
                return new CsTomlString("\"", CsTomlStringType.MultiLineBasic);
            case 8: // first(3) + two adjacent mark(2) + end(3) = 8
                Advance(8);
                return new CsTomlString("\"\"", CsTomlStringType.MultiLineBasic);
        }

        return ExceptionHelper.NotReturnThrow<CsTomlString>(ExceptionHelper.ThrowThreeOrMoreQuotationMarks);
    }

    private T ReadDoubleQuoteSingleLineString<T>()
        where T : CsTomlValue, ICsTomlStringCreator<T>
    {
        Advance(1); // "

        var bufferWriter = RecycleByteArrayPoolBufferWriter.Rent();
        var utf8BufferWriter = new Utf8BufferWriter<ArrayPoolBufferWriter<byte>>(ref bufferWriter);
        try
        {
            var currentSpan = sequenceReader.UnreadSpan;
            var closingQuotationMarks = false;

            while (this.Peek())
            {
                for (var index = 0; index < currentSpan.Length; index++)
                {
                    var ch = currentSpan[index];
                    if (CsTomlSyntax.IsEscape(ch))
                    {
                        ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
                    }
                    else if (CsTomlSyntax.IsDoubleQuoted(ch))
                    {
                        Advance(index + 1);
                        closingQuotationMarks = true;
                        goto BREAK;
                    }
                    else if (CsTomlSyntax.IsBackSlash(ch))
                    {
                        Advance(index);
                        FormatEscapeSequence(ref bufferWriter, multiLine: false);
                        utf8BufferWriter.Flush();
                        goto RESET;
                    }
                    utf8BufferWriter.Write(ch);
                }
                Advance(currentSpan.Length);
                currentSpan = sequenceReader.CurrentSpan;
                continue;

            RESET:
                currentSpan = sequenceReader.UnreadSpan;
            }

        BREAK:
            if (!closingQuotationMarks)
                ExceptionHelper.ThrowBasicStringsIsNotClosedWithClosingQuotationMarks();

            if (Utf8Helper.ContainInvalidSequences(bufferWriter.WrittenSpan))
                ExceptionHelper.ThrowInvalidCodePoints();

            return T.CreateString(bufferWriter.WrittenSpan, CsTomlStringType.Basic);
        }
        finally
        {
            RecycleByteArrayPoolBufferWriter.Return(bufferWriter);
        }
    }

    private CsTomlString ReadDoubleQuoteMultiLineString()
    {
        Advance(3); // """

        if (TryPeek(out var newlineCh) && CsTomlSyntax.IsNewLine(newlineCh))
        {
            if (CsTomlSyntax.IsCr(newlineCh))
            {
                Advance(1);
                if (TryPeek(out var linebreakCh) && CsTomlSyntax.IsLf(linebreakCh))
                {
                    Advance(1);
                    IncreaseLineNumber();
                }
            }
            else if (CsTomlSyntax.IsLf(newlineCh))
            {
                Advance(1);
                IncreaseLineNumber();
            }
        }

        var closingThreeQuotationMarks = false;
        var bufferWriter = RecycleByteArrayPoolBufferWriter.Rent();
        var utf8BufferWriter = new Utf8BufferWriter<ArrayPoolBufferWriter<byte>>(ref bufferWriter);

        while (TryPeek(out var ch))
        {
            if (CsTomlSyntax.IsEscape(ch))
            {
                if (CsTomlSyntax.IsLf(ch))
                {
                    Advance(1);
                    utf8BufferWriter.Write(ch);
                    IncreaseLineNumber();
                    continue;
                }
                else if (CsTomlSyntax.IsCr(ch))
                {
                    Advance(1);
                    if (TryPeek(out var linebreakCh) && CsTomlSyntax.IsLf(linebreakCh))
                    {
                        Advance(1);
                        utf8BufferWriter.Write(ch);
                        utf8BufferWriter.Write(linebreakCh);
                        IncreaseLineNumber();
                        continue;
                    }
                }
                ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
            }
            else if (CsTomlSyntax.IsDoubleQuoted(ch))
            {
                var doubleQuotedCount = 1;
                Advance(1);
                while (TryPeek(out var ch2) && CsTomlSyntax.IsDoubleQuoted(ch2))
                {
                    Advance(1);
                    doubleQuotedCount++;
                    if (doubleQuotedCount == 3)
                    {
                        while (TryPeek(out var ch3))
                        {
                            if (CsTomlSyntax.IsDoubleQuoted(ch3))
                            {
                                if (++doubleQuotedCount >= 6)
                                    ExceptionHelper.ThrowConsecutiveQuotationMarksOf3();
                                Advance(1);
                                utf8BufferWriter.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
                                continue;
                            }
                            closingThreeQuotationMarks = true;
                            goto BREAK;
                        }
                        closingThreeQuotationMarks = true;
                        goto BREAK;
                    }
                }
                for (int i = 0; i < doubleQuotedCount; i++)
                {
                    utf8BufferWriter.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
                }
                continue;
            }
            else if (CsTomlSyntax.IsBackSlash(ch))
            {
                FormatEscapeSequence(ref bufferWriter, multiLine: true);
                utf8BufferWriter.Flush();
                continue;
            }

            utf8BufferWriter.Write(ch);
            Advance(1);
        }
    BREAK:

        if (!closingThreeQuotationMarks)
            ExceptionHelper.ThrowMultilineBasicStringsIsNotClosedWithClosingThreeQuotationMarks();

        try
        {
            if (Utf8Helper.ContainInvalidSequences(bufferWriter.WrittenSpan))
                ExceptionHelper.ThrowInvalidCodePoints();

            return new CsTomlString(bufferWriter.WrittenSpan, CsTomlStringType.MultiLineBasic);
        }
        finally
        {
            RecycleByteArrayPoolBufferWriter.Return(bufferWriter);
        }

    }

    private void FormatEscapeSequence<TBufferWriter>(ref TBufferWriter bufferWriter, bool multiLine)
        where TBufferWriter : IBufferWriter<byte>
    {
        Advance(1); // /

        var result = CsTomlString.TryFormatEscapeSequence(ref sequenceReader, ref bufferWriter, multiLine, true);
        switch(result)
        {
            case CsTomlString.EscapeSequenceResult.Success:
                return;
            case CsTomlString.EscapeSequenceResult.Failure:
                ExceptionHelper.ThrowInvalidEscapeSequence();
                return;
            case CsTomlString.EscapeSequenceResult.Unescaped:
                SkipWhiteSpace();
                if (TryPeek(out var ch) && !CsTomlSyntax.IsNewLine(ch))
                {
                    ExceptionHelper.ThrowInvalidEscapeSequence();
                }
                SkipWhiteSpaceAndNewLine();
                return;
            default:
                ExceptionHelper.ThrowInvalidEscapeSequence();
                return;
        }
    }

    private CsTomlString ReadSingleQuoteString()
    {
        var firstPosition = sequenceReader.Consumed;
        var currentSpan = sequenceReader.UnreadSpan;
        var singleQuoteCount = 0;
        var fullSpan = true;

        while (this.Peek())
        {
            for (var index = 0; index < currentSpan.Length; index++)
            {
                var ch = currentSpan[index];
                if (CsTomlSyntax.IsSingleQuoted(ch))
                {
                    singleQuoteCount++;
                }
                else
                {
                    goto BREAK;
                }
            }
            fullSpan = false;
            Advance(currentSpan.Length);
            currentSpan = sequenceReader.CurrentSpan;
        }

    BREAK:
        if (!fullSpan)
        {
            var endPosition = sequenceReader.Consumed;
            var length = endPosition - firstPosition;
            Rewind(length);
        }

        switch (singleQuoteCount)
        {
            case 1:
            case 2:
                return ReadSingleQuoteSingleLineString<CsTomlString>();
            case 3:
            case 4:
            case 5:
                return ReadSingleQuoteMultiLineString();
            case 6: // first(3) + end(3) = 6 
                Advance(6);
                return new CsTomlString(string.Empty, CsTomlStringType.MultiLineLiteral);
            case 7: // first(3) + one adjacent mark(1) + end(3) = 7
                Advance(7);
                return new CsTomlString("'", CsTomlStringType.MultiLineLiteral);
            case 8: // first(3) + two adjacent mark(2) + end(3) = 8
                Advance(8);
                return new CsTomlString("''", CsTomlStringType.MultiLineLiteral);
        }

        return ExceptionHelper.NotReturnThrow<CsTomlString>(ExceptionHelper.ThrowThreeOrMoreQuotationMarks);
    }

    private T ReadSingleQuoteSingleLineString<T>()
        where T : CsTomlValue, ICsTomlStringCreator<T>
    {
        Advance(1); // '

        var firstPosition = sequenceReader.Consumed;
        var currentSpan = sequenceReader.UnreadSpan;
        var closingQuotationMarks = false;
        var fullSpan = true;
        var totalLength = 0;

        while (this.Peek())
        {
            for (var index = 0; index < currentSpan.Length; index++)
            {
                var ch = currentSpan[index];
                if (CsTomlSyntax.IsEscape(ch))
                {
                    ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
                }
                else if (CsTomlSyntax.IsSingleQuoted(ch))
                {
                    if (!fullSpan) Advance(index);
                    closingQuotationMarks = true;
                    goto BREAK;
                }
                totalLength++;
            }
            Advance(currentSpan.Length);
            currentSpan = sequenceReader.CurrentSpan;
            fullSpan = false;
            continue;
        }

    BREAK:
        if (!closingQuotationMarks)
            ExceptionHelper.ThrowLiteralStringsIsNotClosedWithClosingQuoted();

        if (fullSpan)
        {
            Advance(totalLength + 1);
            return T.CreateString(currentSpan[..totalLength], CsTomlStringType.Literal);
        }
        else
        {
            var endPosition = sequenceReader.Consumed;
            var length = endPosition - firstPosition;
            Rewind(length);

            if (sequenceReader.TryFullSpan(length, out var bytes))
            {
                if (Utf8Helper.ContainInvalidSequences(bytes))
                    ExceptionHelper.ThrowInvalidCodePoints();
                try
                {
                    return T.CreateString(bytes, CsTomlStringType.Literal);
                }
                finally
                {
                    Advance(1);
                }
            }
            else
            {
                var bufferWriter = RecycleByteArrayPoolBufferWriter.Rent();
                try
                {
                    sequenceReader.TryGetbytes(length, bufferWriter);
                    if (Utf8Helper.ContainInvalidSequences(bufferWriter.WrittenSpan))
                        ExceptionHelper.ThrowInvalidCodePoints();

                    return T.CreateString(bufferWriter.WrittenSpan, CsTomlStringType.Literal);
                }
                finally
                {
                    Advance(1);
                    RecycleByteArrayPoolBufferWriter.Return(bufferWriter);
                }
            }
        }
    }

    private CsTomlString ReadSingleQuoteMultiLineString()
    {
        Advance(3); // '''

        var closingThreeSingleQuotes = false;
        var firstPosition = sequenceReader.Consumed;
        if (TryPeek(out var newlineCh) && CsTomlSyntax.IsNewLine(newlineCh))
        {
            if (CsTomlSyntax.IsCr(newlineCh))
            {
                Advance(1);
                if (TryPeek(out var linebreakCh) && CsTomlSyntax.IsLf(linebreakCh))
                {
                    IncreaseLineNumber();
                    Advance(1);
                }
                else
                {
                    Rewind(sequenceReader.Consumed - firstPosition);
                }
            }
            else if (CsTomlSyntax.IsLf(newlineCh))
            {
                IncreaseLineNumber();
                Advance(1);
            }
        }

        firstPosition = sequenceReader.Consumed;
        while (TryPeek(out var ch))
        {
            if (CsTomlSyntax.IsEscape(ch))
            {
                if (CsTomlSyntax.IsNewLine(ch))
                {
                    if (CsTomlSyntax.IsCr(ch))
                    {
                        Advance(1);
                        if (TryPeek(out var linebreakCh) && CsTomlSyntax.IsLf(linebreakCh))
                        {
                            Advance(1);
                            IncreaseLineNumber();
                            continue;
                        }
                        ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
                    }
                    Advance(1);
                    IncreaseLineNumber();
                    continue;
                }
                else if (CsTomlSyntax.IsTab(ch))
                {
                    Advance(1);
                    continue;
                }
                ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
            }
            else if (CsTomlSyntax.IsSingleQuoted(ch))
            {
                var singleQuotedCount = 1;
                Advance(1);
                while (TryPeek(out var ch2) && CsTomlSyntax.IsSingleQuoted(ch2))
                {
                    Advance(1);
                    singleQuotedCount++;
                    if (singleQuotedCount == 3)
                    {
                        while (TryPeek(out var ch3))
                        {
                            if (CsTomlSyntax.IsSingleQuoted(ch3))
                            {
                                if (++singleQuotedCount >= 6)
                                    ExceptionHelper.ThrowConsecutiveSingleQuotationMarksOf3();
                                Advance(1);
                                continue;
                            }
                            closingThreeSingleQuotes = true;
                            goto BREAK;
                        }
                        closingThreeSingleQuotes = true;
                        goto BREAK;
                    }
                }
                continue;
            }
            Advance(1);
        }
    BREAK:
        if (!closingThreeSingleQuotes)
            ExceptionHelper.ThrowMultilineLiteralStringsIsNotClosedWithThreeClosingQuoted();

        var endPosition = sequenceReader.Consumed;
        var length = endPosition - firstPosition - 3;
        Rewind(endPosition - firstPosition);

        if (sequenceReader.TryFullSpan(length, out var bytes))
        {
            if (Utf8Helper.ContainInvalidSequences(bytes))
                ExceptionHelper.ThrowInvalidCodePoints();
            try
            {
                return new CsTomlString(bytes, CsTomlStringType.MultiLineLiteral);
            }
            finally
            {
                Advance(3);
            }
        }
        else
        {
            var bufferWriter = RecycleByteArrayPoolBufferWriter.Rent();
            try
            {
                sequenceReader.TryGetbytes(length, bufferWriter);
                if (Utf8Helper.ContainInvalidSequences(bufferWriter.WrittenSpan))
                    ExceptionHelper.ThrowInvalidCodePoints();

                return new CsTomlString(bufferWriter.WrittenSpan, CsTomlStringType.MultiLineLiteral);
            }
            finally
            {
                Advance(3);
                RecycleByteArrayPoolBufferWriter.Return(bufferWriter);
            }
        }
    }

    private CsTomlDotKey ReadKeyString(bool isTableHeader = false)
    {
        var firstPosition = sequenceReader.Consumed;
        var currentSpan = sequenceReader.UnreadSpan;
        var fullSpan = true;
        var totalLength = 0;

        while (this.Peek())
        {
            for (var index = 0; index < currentSpan.Length; index++)
            {
                var ch = currentSpan[index];
                switch (ch)
                {
                    case CsTomlSyntax.Symbol.TAB:
                    case CsTomlSyntax.Symbol.SPACE:
                    case CsTomlSyntax.Symbol.DOT:
                    case CsTomlSyntax.Symbol.EQUAL:
                        if (!fullSpan) Advance(index);
                        goto BREAK;
                    case CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET:
                        if (isTableHeader)
                        {
                            if (!fullSpan) Advance(index);
                            goto BREAK;
                        }
                        break;
                    default:
                        if (!CsTomlSyntax.IsBareKey(ch))
                            ExceptionHelper.ThrowNumericConversionFailed(ch);
                        break;
                }
                totalLength++;
            }
            Advance(currentSpan.Length);
            currentSpan = sequenceReader.CurrentSpan;
            fullSpan = false;
        }

    BREAK:
        if (fullSpan)
        {
            Advance(totalLength);
            return new CsTomlDotKey(currentSpan[..totalLength], CsTomlStringType.Unquoted);
        }
        else
        {
            var endPosition = sequenceReader.Consumed;
            var length = endPosition - firstPosition;
            Rewind(length);

            if (sequenceReader.TryFullSpan(length, out var bytes))
            {
                return new CsTomlDotKey(bytes, CsTomlStringType.Unquoted);
            }

            var bufferWriter = RecycleByteArrayPoolBufferWriter.Rent();
            try
            {
                sequenceReader.TryGetbytes(length, bufferWriter);
                return new CsTomlDotKey(bufferWriter.WrittenSpan, CsTomlStringType.Unquoted);
            }
            finally
            {
                RecycleByteArrayPoolBufferWriter.Return(bufferWriter);
            }
        }
    }

    private CsTomlArray ReadArray()
    {
        Advance(1); // [

        var array = new CsTomlArray();
        var comma = true;
        var closingBracket = false;
        while (TryPeek(out var ch))
        {
            switch (ch)
            {
                case CsTomlSyntax.Symbol.LEFTSQUAREBRACKET:
                    comma = false;
                    array.Add(ReadArray());
                    break;
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.SPACE:
                    SkipWhiteSpace();
                    break;
                case CsTomlSyntax.Symbol.COMMA:
                    if (comma) ExceptionHelper.ThrowIncorrectTomlFormat();
                    comma = true;
                    Advance(1);
                    break;
                case CsTomlSyntax.Symbol.CARRIAGE:
                    Advance(1);
                    if (TryPeek(out var linebreakCh) && CsTomlSyntax.IsLf(linebreakCh))
                    {
                        Advance(1);
                        IncreaseLineNumber();
                        continue;
                    }
                    return ExceptionHelper.NotReturnThrow<CsTomlArray, byte>(ExceptionHelper.ThrowEscapeCharactersIncluded, ch);
                case CsTomlSyntax.Symbol.LINEFEED:
                    Advance(1);
                    IncreaseLineNumber();
                    continue;
                case CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET:
                    closingBracket = true;
                    Advance(1);
                    goto BREAK;
                case CsTomlSyntax.Symbol.NUMBERSIGN:
                    ReadComment();
                    break;
                default:
                    if (!comma) ExceptionHelper.ThrowNotSeparatedByCommas();
                    comma = false;
                    array.Add(ReadValue());
                    break;
            }
            continue;
        BREAK:
            break;
        }

        if (!closingBracket)
            ExceptionHelper.ThrowTheArrayIsNotClosedWithClosingBrackets();

        return array;
    }

    private CsTomlInlineTable ReadInlineTable()
    {
        Advance(1); // {
        SkipWhiteSpace();

        var inlineTable = new CsTomlInlineTable();
        if (TryPeek(out var c) && CsTomlSyntax.IsRightBraces(c)) // empty inlinetable
        {
            Advance(1); // }
            return inlineTable;
        }

        CsTomlTableNode? currentNode = inlineTable.RootNode;
        while (Peek())
        {
            var dotKeysForInlineTable = new ExtendableArray<CsTomlDotKey>();
            CsTomlTableNode node = CsTomlTableNode.Empty;
            try
            {
                ReadKey(ref dotKeysForInlineTable);
                Advance(1); // skip "="
                SkipWhiteSpace();
                // Register only the key, then set the value.
                node = inlineTable.AddKeyValue(dotKeysForInlineTable.AsSpan(), CsTomlValue.Empty, currentNode);
            }
            finally
            {
                dotKeysForInlineTable.Return();
            }

            node.Value = ReadValue();
            SkipWhiteSpace();
            if (TryPeek(out var ch))
            {
                if (CsTomlSyntax.IsComma(ch))
                {
                    Advance(1);
                    SkipWhiteSpace();
                    continue;
                }
                if (CsTomlSyntax.IsRightBraces(ch))
                {
                    Advance(1);
                    break;
                }
                ExceptionHelper.ThrowIncorrectTomlInlineTableFormat();
            }
        }

        return inlineTable;
    }

    private CsTomlBool ReadBool(bool predictedValue)
    {
        var length = predictedValue ? 4 : 5;
        var boolValue = false;
        if (sequenceReader.TryFullSpan(length, out var bytes))
        {
            var tempReader = new Utf8Reader(bytes);
            ValueFormatter.Deserialize(ref tempReader, (int)length, out boolValue);
        }
        else
        {
            var bufferWriter = RecycleByteArrayPoolBufferWriter.Rent();
            try
            {
                if (sequenceReader.TryGetbytes(length, bufferWriter))
                {
                    var tempReader = new Utf8Reader(bufferWriter.WrittenSpan);
                    ValueFormatter.Deserialize(ref tempReader, (int)length, out boolValue);
                }
                else
                {
                    ExceptionHelper.ThrowEndOfFileReached();
                }
            }
            finally
            {
                RecycleByteArrayPoolBufferWriter.Return(bufferWriter);
            }
        }

        // Verify that the trailing byte does not contain an invalid byte
        if (TryPeek(out var nextByte))
        {
            switch (nextByte)
            {
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.SPACE:
                case CsTomlSyntax.Symbol.CARRIAGE:
                case CsTomlSyntax.Symbol.LINEFEED:
                case CsTomlSyntax.Symbol.COMMA:
                case CsTomlSyntax.Symbol.RIGHTBRACES:
                case CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET:
                case CsTomlSyntax.Symbol.NUMBERSIGN:
                    break;
                default:
                    ExceptionHelper.ThrowIncorrectTomlBooleanFormat();
                    break;
            }
        }

        return boolValue ? CsTomlBool.True : CsTomlBool.False;
    }

    private CsTomlValue ReadNumericValueOrDate()
    {
        var firstPosition = sequenceReader.Consumed;

        // check localtime or localdatetime
        if (sequenceReader.Length >= sequenceReader.Consumed + CsTomlSyntax.DateTime.LocalTimeFormatLength)
        {
            var length = 5;
            Utf8Reader tempReader;
            ArrayPoolBufferWriter<byte>? bufferWriter = null;
            try
            {
                if (sequenceReader.TryFullSpan(length, out var bytes))
                {
                    tempReader = new Utf8Reader(bytes);
                }
                else
                {
                    bufferWriter = RecycleByteArrayPoolBufferWriter.Rent();
                    sequenceReader.TryGetbytes(length, bufferWriter);
                    tempReader = new Utf8Reader(bufferWriter.WrittenSpan);
                }

                Rewind(length);
                if (CsTomlSyntax.IsColon(tempReader[2])) // :
                {
                    if (ExistNoNewLineAndComment(8, out var newLineIndex))
                    {
                        if (sequenceReader.IsFullSpan)
                        {
                            return ReadLocalTime(ReadUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime());
                        }
                        else
                        {
                            var bufferWriter2 = RecycleByteArrayPoolBufferWriter.Rent();
                            try
                            {
                                WriteUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime(ref bufferWriter2);
                                return ReadLocalTime(bufferWriter2.WrittenSpan);
                            }
                            finally
                            {
                                RecycleByteArrayPoolBufferWriter.Return(bufferWriter2);
                            }
                        }
                    }
                }
                else if (CsTomlSyntax.IsHyphen(tempReader[4])) // -
                {
                    if (ExistNoNewLineAndComment(8, out var newLineIndex))
                    {
                        if (sequenceReader.IsFullSpan)
                        {
                            return ReadLocalDateTimeOrOffset(ReadUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime());
                        }
                        else
                        {
                            var bufferWriter2 = RecycleByteArrayPoolBufferWriter.Rent();
                            try
                            {
                                WriteUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime(ref bufferWriter2);
                                return ReadLocalDateTimeOrOffset(bufferWriter2.WrittenSpan);
                            }
                            finally
                            {
                                RecycleByteArrayPoolBufferWriter.Return(bufferWriter2);
                            }
                        }
                    }
                }
                Rewind(sequenceReader.Consumed - firstPosition);

            }
            finally
            {
                if (bufferWriter != null)
                    RecycleByteArrayPoolBufferWriter.Return(bufferWriter);
                bufferWriter = null;

            }
        }

        while(TryPeek(out var ch))
        {
            switch(ch)
            {
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.LINEFEED:
                case CsTomlSyntax.Symbol.CARRIAGE:
                case CsTomlSyntax.Symbol.SPACE:
                case CsTomlSyntax.Symbol.COMMA:
                    goto BREAK;
                case CsTomlSyntax.Symbol.DOT:
                case CsTomlSyntax.Alphabet.E:
                case CsTomlSyntax.Alphabet.e:
                    Rewind(sequenceReader.Consumed - firstPosition);
                    return ReadDouble();
            }
            Advance(1);
        }
    BREAK:
        // decimal
        Rewind(sequenceReader.Consumed - firstPosition);
        return ReadDecimalNumeric();
    }

    private CsTomlValue ReadNumericValueOrDateIfLeadingZero()
    {
        var firstPosition = sequenceReader.Consumed;
        Advance(1); // 0

        if (TryPeek(out var formatsCh))
        {
            if (CsTomlSyntax.IsAlphabet(formatsCh))
            {
                switch (formatsCh)
                {
                    case CsTomlSyntax.Alphabet.x:
                        Advance(1);
                        return ReadHexNumeric();
                    case CsTomlSyntax.Alphabet.o:
                        Advance(1);
                        return ReadOctalNumeric();
                    case CsTomlSyntax.Alphabet.b:
                        Advance(1);
                        return ReadBinaryNumeric();
                    case CsTomlSyntax.Alphabet.e: // 0e...
                    case CsTomlSyntax.Alphabet.E: // 0E...
                        Rewind(sequenceReader.Consumed - firstPosition);
                        return ReadDouble();
                    default:
                        return ExceptionHelper.NotReturnThrow<CsTomlValue, byte>(ExceptionHelper.ThrowIncorrectCompactEscapeCharacters, formatsCh);
                };
            }
            else if (CsTomlSyntax.IsTabOrWhiteSpace(formatsCh))
            {
                return CsTomlInt.Zero;
            }
            else if (CsTomlSyntax.IsNewLine(formatsCh))
            {
                if (CsTomlSyntax.IsCr(formatsCh))
                {
                    Advance(1);
                    if (TryPeek(out var linebreakCh) && CsTomlSyntax.IsLf(linebreakCh))
                    {
                        return CsTomlInt.Zero;
                    }
                    return ExceptionHelper.NotReturnThrow<CsTomlValue, byte>(ExceptionHelper.ThrowIncorrectCompactEscapeCharacters, linebreakCh);
                }
                return CsTomlInt.Zero;
            }
        }
        else
        {
            return CsTomlInt.Zero;
        }
        Rewind(sequenceReader.Consumed - firstPosition);

        // check localtime or localdatetime
        if (sequenceReader.Length >= sequenceReader.Consumed + CsTomlSyntax.DateTime.LocalTimeFormatLength)
        {
            var length = 5;
            Utf8Reader tempReader;
            ArrayPoolBufferWriter<byte>? bufferWriter = null;
            try
            {
                if (sequenceReader.TryFullSpan(length, out var bytes))
                {
                    tempReader = new Utf8Reader(bytes);
                }
                else
                {
                    bufferWriter = RecycleByteArrayPoolBufferWriter.Rent();
                    sequenceReader.TryGetbytes(length, bufferWriter);
                    tempReader = new Utf8Reader(bufferWriter.WrittenSpan);
                }

                Rewind(length);
                if (CsTomlSyntax.IsColon(tempReader[2])) // :
                {
                    if (ExistNoNewLineAndComment(8, out var newLineIndex))
                    {
                        if (sequenceReader.IsFullSpan)
                        {
                            return ReadLocalTime(ReadUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime());
                        }
                        else
                        {
                            var bufferWriter2 = RecycleByteArrayPoolBufferWriter.Rent();
                            try
                            {
                                WriteUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime(ref bufferWriter2);
                                return ReadLocalTime(bufferWriter2.WrittenSpan);
                            }
                            finally
                            {
                                RecycleByteArrayPoolBufferWriter.Return(bufferWriter2);
                            }
                        }
                    }
                }
                else if (CsTomlSyntax.IsHyphen(tempReader[4])) // -
                {
                    if (ExistNoNewLineAndComment(8, out var newLineIndex))
                    {
                        if (sequenceReader.IsFullSpan)
                        {
                            return ReadLocalDateTimeOrOffset(ReadUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime());
                        }
                        else
                        {
                            var bufferWriter2 = RecycleByteArrayPoolBufferWriter.Rent();
                            try
                            {
                                WriteUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime(ref bufferWriter2);
                                return ReadLocalDateTimeOrOffset(bufferWriter2.WrittenSpan);
                            }
                            finally
                            {
                                RecycleByteArrayPoolBufferWriter.Return(bufferWriter2);
                            }
                        }
                    }
                }
                Rewind(sequenceReader.Consumed - firstPosition);

            }
            finally
            {
                if (bufferWriter != null)
                    RecycleByteArrayPoolBufferWriter.Return(bufferWriter);
                bufferWriter = null;

            }
        }

        while (TryPeek(out var ch))
        {
            switch (ch)
            {
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.LINEFEED:
                case CsTomlSyntax.Symbol.CARRIAGE:
                case CsTomlSyntax.Symbol.SPACE:
                case CsTomlSyntax.Symbol.COMMA:
                    goto BREAK;
                case CsTomlSyntax.Symbol.DOT:
                case CsTomlSyntax.Alphabet.E:
                case CsTomlSyntax.Alphabet.e:
                    Rewind(sequenceReader.Consumed - firstPosition);
                    return ReadDouble();
            }
            Advance(1);
        }
    BREAK:
        // decimal
        Rewind(sequenceReader.Consumed - firstPosition);
        return ReadDecimalNumeric();
    }

    private CsTomlValue ReadNumbericValueIfLeadingSign()
    {
        var firstPosition = sequenceReader.Consumed;
        Advance(1); // + or -

        // check prefix and 0x or 0o or 0b
        if (TryPeek(out var first))
        {
            if (first == CsTomlSyntax.Number.Zero)
            {
                Advance(1);
                if (TryPeek(out var formatsCh))
                {
                    if (CsTomlSyntax.IsAlphabet(formatsCh))
                    {
                        switch (formatsCh)
                        {
                            case CsTomlSyntax.Alphabet.e: // 0e...
                            case CsTomlSyntax.Alphabet.E: // 0E...
                                Rewind(sequenceReader.Consumed - firstPosition);
                                return ReadDouble();
                            default:
                                return ExceptionHelper.NotReturnThrow<CsTomlValue, byte>(ExceptionHelper.ThrowIncorrectCompactEscapeCharacters, formatsCh);
                        };
                    }
                    else if (CsTomlSyntax.IsTabOrWhiteSpace(formatsCh))
                    {
                        return CsTomlInt.Zero;
                    }
                    else if (CsTomlSyntax.IsNewLine(formatsCh))
                    {
                        if (CsTomlSyntax.IsCr(formatsCh))
                        {
                            Advance(1);
                            if (TryPeek(out var linebreakCh) && CsTomlSyntax.IsLf(linebreakCh))
                            {
                                return CsTomlInt.Zero;
                            }
                            return ExceptionHelper.NotReturnThrow<CsTomlValue, byte>(ExceptionHelper.ThrowIncorrectCompactEscapeCharacters, linebreakCh);
                        }

                        return CsTomlInt.Zero;
                    }
                }
                else
                {
                    return CsTomlInt.Zero;
                }
                Rewind(sequenceReader.Consumed - firstPosition);
            }
            else if (first == CsTomlSyntax.Alphabet.i || first == CsTomlSyntax.Alphabet.n)
            {
                Rewind(sequenceReader.Consumed - firstPosition);
                return ReadDoubleInfOrNan();
            }
        }
        else
        {
            ExceptionHelper.ThrowIncorrectTomlIntegerFormat();
        }

        while (TryPeek(out var ch))
        {
            switch (ch)
            {
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.LINEFEED:
                case CsTomlSyntax.Symbol.CARRIAGE:
                case CsTomlSyntax.Symbol.SPACE:
                case CsTomlSyntax.Symbol.COMMA:
                    goto BREAK;
                case CsTomlSyntax.Symbol.DOT:
                case CsTomlSyntax.Alphabet.E:
                case CsTomlSyntax.Alphabet.e:
                    Rewind(sequenceReader.Consumed - firstPosition);
                    return ReadDouble();
            }
            Advance(1);
        }
    BREAK:
        // decimal
        Rewind(sequenceReader.Consumed - firstPosition);
        return ReadDecimalNumeric();
    }

    private bool ExistNoNewLineAndComment(int length, out int newLineIndex)
    {
        newLineIndex = -1;
        if (sequenceReader.Length <= sequenceReader.Consumed + length)
            return false;

        var firstPosition = sequenceReader.Consumed;
        for (int i = 0; i < length; i++)
        {
            TryPeek(out var ch);
            switch (ch)
            {
                case CsTomlSyntax.Symbol.LINEFEED:
                case CsTomlSyntax.Symbol.CARRIAGE:
                case CsTomlSyntax.Symbol.NUMBERSIGN:
                    newLineIndex = i;
                    Rewind(sequenceReader.Consumed - firstPosition);
                    return false;
            }
            Advance(1);
        }

        Rewind(sequenceReader.Consumed - firstPosition);
        return true;
    }

    private CsTomlInt ReadDecimalNumeric()
    {
        var writer = new SpanWriter(stackalloc byte[32]);

        var plusOrMinusSign = false;
        if (TryPeek(out var plusOrMinusCh) && CsTomlSyntax.IsPlusOrMinusSign(plusOrMinusCh))
        {
            plusOrMinusSign = true;
            writer.Write(plusOrMinusCh);
            Advance(1);
        }

        if (TryPeek(out var firstCh) && CsTomlSyntax.IsUnderScore(firstCh))
        {
            ExceptionHelper.ThrowUnderscoreUsedConsecutively();
        }

        var underscore = false;
        while (TryPeek(out var ch))
        {
            if (CsTomlSyntax.IsNumber(ch))
            {
                underscore = false;
                writer.Write(ch);
                Advance(1);
                continue;
            }

            switch (ch)
            {
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.SPACE:
                case CsTomlSyntax.Symbol.CARRIAGE:
                case CsTomlSyntax.Symbol.LINEFEED:
                case CsTomlSyntax.Symbol.COMMA:
                case CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET:
                case CsTomlSyntax.Symbol.RIGHTBRACES:
                case CsTomlSyntax.Symbol.NUMBERSIGN:
                    goto BREAK;
                case CsTomlSyntax.Symbol.UNDERSCORE:
                    // Each underscore is not surrounded by at least one digit on each side.
                    if (underscore) ExceptionHelper.ThrowUnderscoreUsedConsecutively();
                    underscore = true;
                    Advance(1);
                    continue;
                default:
                    return ExceptionHelper.NotReturnThrow<CsTomlInt, byte>(ExceptionHelper.ThrowEscapeCharactersIncluded, ch);
            }
        }
    BREAK:

        if (underscore)
            ExceptionHelper.ThrowUnderscoreIsUsedAtTheEnd();

        var writingSpan = writer.WrittenSpan;
        if (plusOrMinusSign)
        { 
            if (writingSpan.Length > 2)
            {
                // +00 or -01
                if (writingSpan[1] == CsTomlSyntax.Number.Zero)
                    ExceptionHelper.ThrowIncorrectTomlIntegerFormat();
            }
        }
        else
        {
            if (writingSpan.Length > 1)
            {
                // 00 or 01
                if (writingSpan[0] == CsTomlSyntax.Number.Zero)
                    ExceptionHelper.ThrowIncorrectTomlIntegerFormat();

            }
        }

        var tempReader = new Utf8Reader(writingSpan);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out long value);
        return CsTomlInt.Create(value);
    }

    private CsTomlInt ReadHexNumeric()
    {
        if (TryPeek(out var firstCh))
        {
            if (!CsTomlSyntax.IsHex(firstCh))
            {
                if (CsTomlSyntax.IsUnderScore(firstCh))
                    ExceptionHelper.ThrowUnderscoreUsedConsecutively();
                ExceptionHelper.ThrowIncorrectTomlIntegerHexadecimalFormat();
            }
        }
        else
        {
            ExceptionHelper.ThrowIncorrectTomlIntegerHexadecimalFormat();
        }

        var writer = new SpanWriter(stackalloc byte[32]);
        writer.Write(CsTomlSyntax.Number.Zero);
        writer.Write(CsTomlSyntax.Alphabet.x);

        var underscore = false;
        while (TryPeek(out var ch))
        {
            if (CsTomlSyntax.IsNumber(ch) || CsTomlSyntax.IsLowerHexAlphabet(ch) || CsTomlSyntax.IsUpperHexAlphabet(ch))
            {
                underscore = false;
                writer.Write(ch);
                Advance(1);
                continue;
            }

            switch (ch)
            {
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.SPACE:
                case CsTomlSyntax.Symbol.CARRIAGE:
                case CsTomlSyntax.Symbol.LINEFEED:
                case CsTomlSyntax.Symbol.COMMA:
                case CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET:
                    goto BREAK;
                case CsTomlSyntax.Symbol.UNDERSCORE:
                    // Each underscore is not surrounded by at least one digit on each side.
                    if (underscore) ExceptionHelper.ThrowUnderscoreUsedConsecutively();
                    underscore = true;
                    Advance(1);
                    continue;
                default:
                    return ExceptionHelper.NotReturnThrow<CsTomlInt, byte>(ExceptionHelper.ThrowEscapeCharactersIncluded, ch);
            }

        BREAK:
            break;
        }

        if (underscore)
            ExceptionHelper.ThrowUnderscoreIsUsedAtTheEnd();

        var tempReader = new Utf8Reader(writer.WrittenSpan);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out long value);
        return CsTomlInt.Create(value);
    }

    private CsTomlInt ReadOctalNumeric()
    {
        if (TryPeek(out var firstCh))
        {
            if (!CsTomlSyntax.IsOctal(firstCh))
            {
                if (CsTomlSyntax.IsUnderScore(firstCh))
                    ExceptionHelper.ThrowUnderscoreUsedConsecutively();
                ExceptionHelper.ThrowIncorrectTomlIntegerOctalFormat();
            }
        }
        else
        {
            ExceptionHelper.ThrowIncorrectTomlIntegerOctalFormat();
        }

        var writer = new SpanWriter(stackalloc byte[32]);
        writer.Write(CsTomlSyntax.Number.Zero);
        writer.Write(CsTomlSyntax.Alphabet.o);

        var underscore = false;
        while (TryPeek(out var ch))
        {
            if (CsTomlSyntax.IsOctal(ch))
            {
                underscore = false;
                writer.Write(ch);
                Advance(1);
                continue;
            }

            switch (ch)
            {
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.SPACE:
                case CsTomlSyntax.Symbol.CARRIAGE:
                case CsTomlSyntax.Symbol.LINEFEED:
                case CsTomlSyntax.Symbol.COMMA:
                case CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET:
                    goto BREAK;
                case CsTomlSyntax.Symbol.UNDERSCORE:
                    // Each underscore is not surrounded by at least one digit on each side.
                    if (underscore) ExceptionHelper.ThrowUnderscoreUsedConsecutively();
                    underscore = true;
                    Advance(1);
                    continue;
                default:
                    return ExceptionHelper.NotReturnThrow<CsTomlInt, byte>(ExceptionHelper.ThrowEscapeCharactersIncluded, ch);
            }

        BREAK:
            break;
        }

        if (underscore)
            ExceptionHelper.ThrowUnderscoreIsUsedAtTheEnd();

        var tempReader = new Utf8Reader(writer.WrittenSpan);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out long value);
        return CsTomlInt.Create(value);
    }

    private CsTomlInt ReadBinaryNumeric()
    {
        if (TryPeek(out var firstCh))
        {
            if (!CsTomlSyntax.IsBinary(firstCh))
            {
                if (CsTomlSyntax.IsUnderScore(firstCh))
                    ExceptionHelper.ThrowUnderscoreUsedConsecutively();
                ExceptionHelper.ThrowIncorrectTomlIntegerBinaryFormat();
            }
        }
        else
        {
            ExceptionHelper.ThrowIncorrectTomlIntegerBinaryFormat();
        }

        var writer = new SpanWriter(stackalloc byte[32]);
        writer.Write(CsTomlSyntax.Number.Zero);
        writer.Write(CsTomlSyntax.Alphabet.b);

        var underscore = false;
        while (TryPeek(out var ch))
        {
            if (CsTomlSyntax.IsBinary(ch))
            {
                underscore = false;
                writer.Write(ch);
                Advance(1);
                continue;
            }

            switch (ch)
            {
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.SPACE:
                case CsTomlSyntax.Symbol.CARRIAGE:
                case CsTomlSyntax.Symbol.LINEFEED:
                case CsTomlSyntax.Symbol.COMMA:
                case CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET:
                    goto BREAK;
                case CsTomlSyntax.Symbol.UNDERSCORE:
                    // Each underscore is not surrounded by at least one digit on each side.
                    if (underscore) ExceptionHelper.ThrowUnderscoreUsedConsecutively();
                    underscore = true;
                    Advance(1);
                    continue;
                default:
                    return ExceptionHelper.NotReturnThrow<CsTomlInt, byte>(ExceptionHelper.ThrowEscapeCharactersIncluded, ch);
            }

        BREAK:
            break;
        }

        if (underscore)
            ExceptionHelper.ThrowUnderscoreIsUsedAtTheEnd();

        var tempReader = new Utf8Reader(writer.WrittenSpan);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out long value);
        return CsTomlInt.Create(value);
    }

    private CsTomlFloat ReadDouble()
    {
        var writer = new SpanWriter(stackalloc byte[32]);

        if (TryPeek(out var plusOrMinusCh) && CsTomlSyntax.IsPlusOrMinusSign(plusOrMinusCh))
        {
            writer.Write(plusOrMinusCh);
            Advance(1);
        }

        var firstPosition = sequenceReader.Consumed;
        if (TryPeek(out var firstNumberCh))
        {
            switch (firstNumberCh)
            {
                case CsTomlSyntax.Symbol.UNDERSCORE:
                    return ExceptionHelper.NotReturnThrow<CsTomlFloat>(ExceptionHelper.ThrowUnderscoreUsedFirst);
                case CsTomlSyntax.Symbol.DOT:
                    return ExceptionHelper.NotReturnThrow<CsTomlFloat>(ExceptionHelper.ThrowDotIsUsedFirst);
                case CsTomlSyntax.Alphabet.i:
                case CsTomlSyntax.Alphabet.n:
                    if (CsTomlSyntax.IsPlusOrMinusSign(plusOrMinusCh)) Rewind(1);
                    return ReadDoubleInfOrNan();
                case var zero when zero == CsTomlSyntax.Number.Zero:
                    Advance(1);
                    if (TryPeek(out var secondNumberCh))
                    {
                        switch(secondNumberCh)
                        {
                            case CsTomlSyntax.Symbol.DOT: // 0.1 ..
                            case CsTomlSyntax.Alphabet.e: // 0e...
                            case CsTomlSyntax.Alphabet.E: // 0E...
                                break;
                            default:
                                ExceptionHelper.ThrowIncorrectTomlFloatFormat();
                                break;
                        }
                    }
                    Rewind(sequenceReader.Consumed - firstPosition);
                    break;
            }
        }

        var number = false;
        var sign = false;
        var underline = false;
        var dot = false;
        var exp = false;
        while (TryPeek(out var ch))
        {
            if (CsTomlSyntax.IsNumber(ch))
            {
                number = true;
                underline = false;
                writer.Write(ch);
                Advance(1);
                continue;
            }

            switch (ch)
            {
                case CsTomlSyntax.Symbol.UNDERSCORE:
                    if (!number) ExceptionHelper.ThrowUnderscoreUsedWhereNotSurroundedByNumbers();
                    // Each underscore is not surrounded by at least one digit on each side.
                    if (underline) ExceptionHelper.ThrowUnderscoreUsedConsecutively();
                    number = false;
                    underline = true;
                    Advance(1);
                    continue;
                case CsTomlSyntax.Symbol.DOT:
                    if (!number) ExceptionHelper.ThrowDotIsUsedWhereNotSurroundedByNumbers();
                    if (dot) ExceptionHelper.ThrowDotsAreUsedMoreThanOnce();
                    if (exp) ExceptionHelper.ThrowDecimalPointIsPresentAfterTheExponentialPartE();
                    number = false;
                    dot = true;
                    writer.Write(ch);
                    Advance(1);
                    continue;
                case CsTomlSyntax.Alphabet.e:
                case CsTomlSyntax.Alphabet.E:
                    if (!number) ExceptionHelper.ThrowExponentPartUsedWhereNotSurroundedByNumbers();
                    if (exp) ExceptionHelper.ThrowTheExponentPartUsedMoreThanOnce();
                    number = false;
                    sign = false;
                    exp = true;
                    writer.Write(ch);
                    Advance(1);
                    continue;
                case CsTomlSyntax.Symbol.PLUS:
                case CsTomlSyntax.Symbol.MINUS:
                    if (!exp || sign) ExceptionHelper.ThrowIncorrectPositivAndNegativeSigns();
                    number = false;
                    sign = true;
                    writer.Write(ch);
                    Advance(1);
                    continue;
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.SPACE:
                case CsTomlSyntax.Symbol.CARRIAGE:
                case CsTomlSyntax.Symbol.LINEFEED:
                case CsTomlSyntax.Symbol.COMMA:
                case CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET:
                case CsTomlSyntax.Symbol.RIGHTBRACES:
                case CsTomlSyntax.Symbol.NUMBERSIGN:
                    goto BREAK;
                default:
                    return ExceptionHelper.NotReturnThrow<CsTomlFloat, byte>(ExceptionHelper.ThrowEscapeCharactersIncluded, ch);
            }

        BREAK:
            break;

        }

        if (underline)
            ExceptionHelper.ThrowUnderscoreIsUsedAtTheEnd();

        var writingSpan = writer.WrittenSpan;
        switch(writingSpan[^1])
        {
            case CsTomlSyntax.Symbol.UNDERSCORE:
                ExceptionHelper.ThrowUnderscoreIsUsedAtTheEnd();
                break;
            case CsTomlSyntax.Symbol.DOT:
                ExceptionHelper.ThrowDotIsUsedAtTheEnd();
                break;
            case CsTomlSyntax.Alphabet.e:
            case CsTomlSyntax.Alphabet.E:
                ExceptionHelper.ThrowExponentPartIsUsedAtTheEnd();
                break;
            case CsTomlSyntax.Symbol.PLUS:
            case CsTomlSyntax.Symbol.MINUS:
                ExceptionHelper.ThrowIncorrectPositivAndNegativeSigns();
                break;
        }

        var tempReader = new Utf8Reader(writingSpan);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out double value);
        return new CsTomlFloat(value);
    }

    private CsTomlFloat ReadDoubleInfOrNan()
    {
        if (sequenceReader.Length < sequenceReader.Consumed + 3) ExceptionHelper.ThrowIncorrectTomlFloatFormat();

        var length = 3;
        Utf8Reader tempReader;
        ArrayPoolBufferWriter<byte>? bufferWriter = null;
        try
        {
            if (sequenceReader.TryFullSpan(length, out var bytes))
            {
                tempReader = new Utf8Reader(bytes);
            }
            else
            {
                bufferWriter = RecycleByteArrayPoolBufferWriter.Rent();
                sequenceReader.TryGetbytes(length, bufferWriter);
                tempReader = new Utf8Reader(bufferWriter.WrittenSpan);
            }

            if (tempReader[0] == CsTomlSyntax.Alphabet.i &&
                tempReader[1] == CsTomlSyntax.Alphabet.n &&
                tempReader[2] == CsTomlSyntax.Alphabet.f)
            {
                return CsTomlFloat.Inf;
            }
            else if (tempReader[0] == CsTomlSyntax.Alphabet.n &&
                tempReader[1] == CsTomlSyntax.Alphabet.a &&
                tempReader[2] == CsTomlSyntax.Alphabet.n)
            {
                return CsTomlFloat.Nan;
            }
        }
        finally
        {
            if (bufferWriter != null)
                RecycleByteArrayPoolBufferWriter.Return(bufferWriter);
            bufferWriter = null;
        }

        Rewind(3);
        if (sequenceReader.Length < sequenceReader.Consumed + 4) ExceptionHelper.ThrowIncorrectTomlFloatFormat();

        length = 4;
        try
        {
            if (sequenceReader.TryFullSpan(length, out var bytes))
            {
                tempReader = new Utf8Reader(bytes);
            }
            else
            {
                bufferWriter = RecycleByteArrayPoolBufferWriter.Rent();
                sequenceReader.TryGetbytes(length, bufferWriter);
                tempReader = new Utf8Reader(bufferWriter.WrittenSpan);
            }

            if (CsTomlSyntax.IsPlusSign(tempReader[0]))
            {
                if (tempReader[1] == CsTomlSyntax.Alphabet.i &&
                    tempReader[2] == CsTomlSyntax.Alphabet.n &&
                    tempReader[3] == CsTomlSyntax.Alphabet.f)
                {
                    return CsTomlFloat.Inf;
                }
                else if (tempReader[1] == CsTomlSyntax.Alphabet.n &&
                    tempReader[2] == CsTomlSyntax.Alphabet.a &&
                    tempReader[3] == CsTomlSyntax.Alphabet.n)
                {
                    return CsTomlFloat.Nan;
                }
            }
            else if (CsTomlSyntax.IsMinusSign(tempReader[0]))
            {
                if (tempReader[1] == CsTomlSyntax.Alphabet.i &&
                    tempReader[2] == CsTomlSyntax.Alphabet.n &&
                    tempReader[3] == CsTomlSyntax.Alphabet.f)
                {
                    return CsTomlFloat.NInf;
                }
                else if (tempReader[1] == CsTomlSyntax.Alphabet.n &&
                    tempReader[2] == CsTomlSyntax.Alphabet.a &&
                    tempReader[3] == CsTomlSyntax.Alphabet.n)
                {
                    return CsTomlFloat.PNan;
                }
            }
            Rewind(4);
            return ExceptionHelper.NotReturnThrow<CsTomlFloat>(ExceptionHelper.ThrowIncorrectTomlFloatFormat);
        }
        finally
        {
            if (bufferWriter != null)
                RecycleByteArrayPoolBufferWriter.Return(bufferWriter);
            bufferWriter = null;
        }
    }

    private CsTomlValue ReadLocalDateTimeOrOffset(ReadOnlySpan<byte> bytes)
    {
        // local date
        if (bytes.Length == CsTomlSyntax.DateTime.LocalDateFormatLength)
        {
            return ReadLocalDate(bytes);
        }

        // offset datetime
        if (bytes.Length >= CsTomlSyntax.DateTime.OffsetDateTimeZFormatLength)
        {
            if (bytes[^1] == CsTomlSyntax.Alphabet.Z || bytes[^1] == CsTomlSyntax.Alphabet.z)
            {
                return ReadOffsetDateTime(bytes);
            }
            else if (CsTomlSyntax.IsPlusOrMinusSign(bytes[19]))
            {
                return ReadOffsetDateTimeByNumber(bytes);
            }
            else if (CsTomlSyntax.IsDot(bytes[19]))
            {
                var index = 20;
                while (index < bytes.Length)
                {
                    var c = bytes[index++];
                    if (!CsTomlSyntax.IsNumber(c))
                    {
                        if (CsTomlSyntax.IsPlusOrMinusSign(c)) break;
                    }
                }
                if (index < bytes.Length)
                {
                    if (!CsTomlSyntax.IsNumber(bytes[index++])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
                    if (!CsTomlSyntax.IsNumber(bytes[index++])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
                    if (!CsTomlSyntax.IsColon(bytes[index++])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
                    if (!CsTomlSyntax.IsNumber(bytes[index++])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
                    if (!CsTomlSyntax.IsNumber(bytes[index++])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
                    return ReadOffsetDateTimeByNumber(bytes);
                }
            }
        }

        // local date time
        return ReadLocalDateTime(bytes);
    }

    private CsTomlLocalDateTime ReadLocalDateTime(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < CsTomlSyntax.DateTime.LocalDateTimeFormatLength) 
            ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();

        if (!CsTomlSyntax.IsNumber(bytes[0])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[1])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[2])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[3])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsHyphen(bytes[4])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[5])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[6])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsHyphen(bytes[7])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[8])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[9])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!(CsTomlSyntax.IsWhiteSpace(bytes[10]) || bytes[10] == CsTomlSyntax.Alphabet.T || bytes[10] == CsTomlSyntax.Alphabet.t)) 
            ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[11])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[12])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsColon(bytes[13])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[14])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[15])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsColon(bytes[16])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[17])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[18])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();

        if (bytes.Length > CsTomlSyntax.DateTime.LocalDateTimeFormatLength)
        {
            if (!CsTomlSyntax.IsDot(bytes[19])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
            var index = 20;
            while (index < bytes.Length)
            {
                if (!CsTomlSyntax.IsNumber(bytes[index++])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
            }
        }
        var tempReader = new Utf8Reader(bytes);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out DateTime value);
        return new CsTomlLocalDateTime(value);
    }

    private CsTomlLocalDate ReadLocalDate(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < CsTomlSyntax.DateTime.LocalDateFormatLength) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!CsTomlSyntax.IsNumber(bytes[0])) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!CsTomlSyntax.IsNumber(bytes[1])) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!CsTomlSyntax.IsNumber(bytes[2])) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!CsTomlSyntax.IsNumber(bytes[3])) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!CsTomlSyntax.IsHyphen(bytes[4])) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!CsTomlSyntax.IsNumber(bytes[5])) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!CsTomlSyntax.IsNumber(bytes[6])) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!CsTomlSyntax.IsHyphen(bytes[7])) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!CsTomlSyntax.IsNumber(bytes[8])) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!CsTomlSyntax.IsNumber(bytes[9])) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();

        var tempReader = new Utf8Reader(bytes);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out DateOnly value);
        return new CsTomlLocalDate(value);
    }

    private CsTomlLocalTime ReadLocalTime(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < CsTomlSyntax.DateTime.LocalTimeFormatLength) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[0])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[1])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!CsTomlSyntax.IsColon(bytes[2])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[3])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[4])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!CsTomlSyntax.IsColon(bytes[5])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[6])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[7])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();

        if (bytes.Length > CsTomlSyntax.DateTime.LocalTimeFormatLength)
        {
            if (!CsTomlSyntax.IsDot(bytes[8])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
            var index = 9;
            while(index < bytes.Length)
            {
                if (!CsTomlSyntax.IsNumber(bytes[index++])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
            }
        }

        var tempReader = new Utf8Reader(bytes);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out TimeOnly value);
        return new CsTomlLocalTime(value);
    }

    private CsTomlOffsetDateTime ReadOffsetDateTime(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < CsTomlSyntax.DateTime.OffsetDateTimeZFormatLength) 
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        if (!(bytes[^1] == CsTomlSyntax.Alphabet.Z || bytes[^1] == CsTomlSyntax.Alphabet.z)) 
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        if (!CsTomlSyntax.IsNumber(bytes[0])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[1])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[2])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[3])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsHyphen(bytes[4])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[5])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[6])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsHyphen(bytes[7])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[8])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[9])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!(CsTomlSyntax.IsWhiteSpace(bytes[10]) || bytes[10] == CsTomlSyntax.Alphabet.T || bytes[10] == CsTomlSyntax.Alphabet.t)) 
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[11])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[12])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsColon(bytes[13])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[14])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[15])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsColon(bytes[16])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[17])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[18])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        var tempReader = new Utf8Reader(bytes);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out DateTimeOffset value);
        return new CsTomlOffsetDateTime(value);
    }

    private CsTomlOffsetDateTime ReadOffsetDateTimeByNumber(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < CsTomlSyntax.DateTime.OffsetDateTimeZFormatLength) 
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        if (!CsTomlSyntax.IsNumber(bytes[0])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[1])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[2])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[3])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsHyphen(bytes[4])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[5])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[6])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsHyphen(bytes[7])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[8])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[9])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!(CsTomlSyntax.IsWhiteSpace(bytes[10]) || bytes[10] == CsTomlSyntax.Alphabet.T || bytes[10] == CsTomlSyntax.Alphabet.t)) 
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[11])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[12])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsColon(bytes[13])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[14])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[15])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsColon(bytes[16])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[17])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[18])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        if (CsTomlSyntax.IsHyphen(bytes[19]))
        {
            if (!CsTomlSyntax.IsNumber(bytes[20])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!CsTomlSyntax.IsNumber(bytes[21])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!CsTomlSyntax.IsColon(bytes[22])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!CsTomlSyntax.IsNumber(bytes[23])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!CsTomlSyntax.IsNumber(bytes[24])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        }
        else if (CsTomlSyntax.IsDot(bytes[19]))
        {
            var index = 20;
            while (index < bytes.Length)
            {
                var c = bytes[index++];
                if (!CsTomlSyntax.IsNumber(c))
                {
                    if (CsTomlSyntax.IsPlusOrMinusSign(c)) break;
                    ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
                }
            }
            if (!CsTomlSyntax.IsNumber(bytes[index++])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!CsTomlSyntax.IsNumber(bytes[index++])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!CsTomlSyntax.IsColon(bytes[index++])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!CsTomlSyntax.IsNumber(bytes[index++])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!CsTomlSyntax.IsNumber(bytes[index++])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        }

        var tempReader = new Utf8Reader(bytes);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out DateTimeOffset value);
        return new CsTomlOffsetDateTime(value);
    }

    private ReadOnlySpan<byte> ReadUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime()
    {
        var firstPosition = sequenceReader.Consumed;
        var delimiterSpace = false;
        while (TryPeek(out var ch))
        {
            switch (ch)
            {
                case CsTomlSyntax.Symbol.LINEFEED:
                case CsTomlSyntax.Symbol.COMMA:
                    goto BREAK;
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET:
                case CsTomlSyntax.Symbol.NUMBERSIGN:
                    if (delimiterSpace)
                    {
                        Rewind(1);
                    }
                    goto BREAK;
                case CsTomlSyntax.Symbol.SPACE:
                    if (sequenceReader.Consumed - firstPosition == 10) // space or T
                    {
                        delimiterSpace = true;
                        Advance(1);
                        continue;
                    }
                    goto BREAK;
                case CsTomlSyntax.Symbol.CARRIAGE:
                    if (delimiterSpace)
                    {
                        goto BREAK;
                    }
                    Advance(1);
                    if (TryPeek(out var linebreakCh) && CsTomlSyntax.IsLf(linebreakCh))
                    {
                        Rewind(1);
                        goto BREAK;
                    }
                    ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
                    return default;
                default:
                    Advance(1);
                    continue;
            }
        BREAK:
            break;
        }

        var endPosition = sequenceReader.Consumed;
        var length = endPosition - firstPosition;
        Rewind(length);

        sequenceReader.TryFullSpan(length, out var bytes);

        return bytes;
    }

    private void WriteUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime<TBufferWriter>(ref TBufferWriter bufferWriter)
        where TBufferWriter : IBufferWriter<byte>
    {
        var firstPosition = sequenceReader.Consumed;
        var delimiterSpace = false;
        var space = false;

        var writer = new Utf8Writer<TBufferWriter>(ref bufferWriter);
        while (TryPeek(out var ch))
        {
            switch (ch)
            {
                case CsTomlSyntax.Symbol.LINEFEED:
                case CsTomlSyntax.Symbol.COMMA:
                    goto BREAK;
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET:
                case CsTomlSyntax.Symbol.NUMBERSIGN:
                    if (delimiterSpace)
                    {
                        Rewind(1);
                    }
                    goto BREAK;
                case CsTomlSyntax.Symbol.SPACE:
                    if (sequenceReader.Consumed - firstPosition == 10) // space or T
                    {
                        delimiterSpace = true;
                        Advance(1);
                        space = true;
                        writer.GetSpan(1)[0] = ch;
                        continue;
                    }
                    goto BREAK;
                case CsTomlSyntax.Symbol.CARRIAGE:
                    if (delimiterSpace)
                    {
                        goto BREAK;
                    }
                    Advance(1);
                    if (TryPeek(out var linebreakCh) && CsTomlSyntax.IsLf(linebreakCh))
                    {
                        Rewind(1);
                        goto BREAK;
                    }
                    ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
                    return;
                default:
                    if (space)
                    {
                        writer.Advance(1);
                        space = false;
                    }
                    Advance(1);
                    writer.Write(ch);
                    continue;
            }
        BREAK:
            break;
        }
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IncreaseLineNumber()
    {
        if (Peek()) LineNumber++;
    }
}

