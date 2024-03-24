using CsToml.Error;
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

            return new CsTomlString(bytes, CsTomlString.CsTomlStringType.Unquoted);
        }
        else
        {
            var rent = RecycleByteArrayPoolBufferWriter.Rent();
            try
            {
                sequenceReader.TryGetbytes(length, rent);
                if (Utf8Helper.ContainInvalidSequences(rent.WrittenSpan))
                    ExceptionHelper.ThrowInvalidCodePoints();

                return new CsTomlString(rent.WrittenSpan, CsTomlString.CsTomlStringType.Unquoted);
            }
            finally
            {
                RecycleByteArrayPoolBufferWriter.Return(rent);
            }
        }
    }

    public CsTomlKey ReadKey()
    {
        SkipWhiteSpace();
        if (!Peek()) ExceptionHelper.ThrowEndOfFileReached();

        var isTableHeader = false;
        var isArrayOfTablesHeader = false;
        if (TryPeek(out var tableHeaderCh) && CsTomlSyntax.IsLeftSquareBrackets(tableHeaderCh))
        {
            isTableHeader = true;
            Advance(1);
            if (TryPeek(out var ArrayOfTablesHeaderCh) && CsTomlSyntax.IsLeftSquareBrackets(ArrayOfTablesHeaderCh))
            {
                isArrayOfTablesHeader = true;
                Advance(1);
            }
        }

        var key = new CsTomlKey();
        var period = true;
        while (TryPeek(out var c))
        {
            if (CsTomlSyntax.IsBareKey(c))
            {
                if (!period) ExceptionHelper.ThrowIncorrectTomlFormat();
                period = false;
                key.Add(ReadKeyString(isTableHeader));
                continue;
            }
            switch(c)
            {
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.SPACE:
                    SkipWhiteSpace();
                    if (TryPeek(out var c2))
                    {
                        switch(c2)
                        {
                            case CsTomlSyntax.Symbol.EQUAL:
                                goto BREAK;
                            case CsTomlSyntax.Symbol.PERIOD:
                                if (period)
                                {
                                    if (key.DotKeys.Count > 0)
                                        ExceptionHelper.ThrowPeriodUsedMoreThanOnce();
                                    else
                                        ExceptionHelper.ThrowTheDotIsDefinedFirst();
                                }
                                period = true;
                                Advance(1);
                                SkipWhiteSpace();
                                continue;
                            case CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET:
                                continue;
                            case CsTomlSyntax.Symbol.DOUBLEQUOTED:
                            case CsTomlSyntax.Symbol.SINGLEQUOTED:
                            case var alphabet when CsTomlSyntax.IsAlphabet(alphabet):
                            case var number when CsTomlSyntax.IsNumber(number):
                                if (key.DotKeys.Count > 0 && !period)
                                    ExceptionHelper.ThrowIncorrectTomlFormat();
                                continue;
                        }
                        ExceptionHelper.ThrowIncorrectTomlFormat();
                    }
                    goto BREAK;
                case CsTomlSyntax.Symbol.EQUAL:
                    if (key.DotKeys.Count == 0)
                    {
                        ExceptionHelper.ThrowBareKeyIsEmpty();
                    }
                    goto BREAK;
                case CsTomlSyntax.Symbol.PERIOD:
                    if (period)
                    {
                        if (key.DotKeys.Count > 0)
                            ExceptionHelper.ThrowPeriodUsedMoreThanOnce();
                        else
                            ExceptionHelper.ThrowTheDotIsDefinedFirst();
                    }
                    period = true;
                    Advance(1);
                    SkipWhiteSpace();
                    continue;
                case CsTomlSyntax.Symbol.DOUBLEQUOTED:
                    if (!period) ExceptionHelper.ThrowIncorrectTomlFormat();
                    period = false;
                    key.Add(ReadDoubleQuoteSingleLineString());
                    continue;
                case CsTomlSyntax.Symbol.SINGLEQUOTED:
                    if (!period) ExceptionHelper.ThrowIncorrectTomlFormat();
                    period = false;
                    key.Add(ReadSingleQuoteSingleLineString());
                    continue;
                case CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET:
                    if (isTableHeader)
                    {
                        Advance(1);
                        if (isArrayOfTablesHeader)
                        {
                            if (TryPeek(out var tableHeaderArrayEndCh) && CsTomlSyntax.IsRightSquareBrackets(tableHeaderArrayEndCh))
                            {
                                Advance(1);
                            }
                            else
                            {
                                ExceptionHelper.ThrowEndOfFileReached();
                            }
                        }
                        goto BREAK;
                    }
                    return ExceptionHelper.NotReturnThrow<CsTomlKey>(ExceptionHelper.ThrowIncorrectTomlFormat);
                default:
                    return ExceptionHelper.NotReturnThrow<CsTomlKey>(ExceptionHelper.ThrowIncorrectTomlFormat);
            }

        BREAK:
            break;
        }

        return key;
    }

    public CsTomlValue ReadValue()
    {
        SkipWhiteSpace();

        if (!TryPeek(out var c)) ExceptionHelper.ThrowEndOfFileReached();

        return c switch
        {
            CsTomlSyntax.Symbol.DOUBLEQUOTED => ReadDoubleQuoteString(),
            CsTomlSyntax.Symbol.SINGLEQUOTED => ReadSingleQuoteString(),
            CsTomlSyntax.Symbol.PLUS => ReadNumericOrDate(),
            CsTomlSyntax.Symbol.MINUS => ReadNumericOrDate(),
            CsTomlSyntax.Symbol.LEFTSQUAREBRACKET => ReadArray(),
            CsTomlSyntax.Symbol.LEFTBRACES => ReadInlineTable(),
            CsTomlSyntax.AlphaBet.t => ReadBool(),
            CsTomlSyntax.AlphaBet.f => ReadBool(),
            CsTomlSyntax.AlphaBet.i => ReadDoubleInfOrNan(),
            CsTomlSyntax.AlphaBet.n => ReadDoubleInfOrNan(),
            { } when CsTomlSyntax.IsNumber(c) => ReadNumericOrDate(),
            _ => ExceptionHelper.NotReturnThrow<CsTomlValue>(ExceptionHelper.ThrowIncorrectTomlFormat),
        }; ;
    }

    public void SkipWhiteSpace()
    {
        while (TryPeek(out var ch))
        {
            if (!CsTomlSyntax.IsTabOrWhiteSpace(ch))
                break;
            Advance(1);
        }
    }

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
                    break;
                }
            }
            else if (!CsTomlSyntax.IsTabOrWhiteSpace(ch))
            {
                break;
            }

            Advance(1);
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
    public bool TryPeek(out byte ch)
        => sequenceReader.TryPeek(out ch);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Peek()
        => sequenceReader.Peek();

    private CsTomlString ReadDoubleQuoteString()
    {
        Advance(1);
        var doubleQuoteCount = 1;
        while (TryPeek(out var first) && (CsTomlSyntax.IsDoubleQuoted(first)))
        {
            if (doubleQuoteCount < 3)
            {
                doubleQuoteCount++;
                Advance(1);
                continue;
            }
            break;
        }

        switch (doubleQuoteCount)
        {
            case 1:
            case 2:
                Rewind(doubleQuoteCount);
                return ReadDoubleQuoteSingleLineString();
            case 3:
            case 4: 
            case 5:
                Rewind(doubleQuoteCount);
                return ReadDoubleQuoteMultiLineString();
        }

        return ExceptionHelper.NotReturnThrow<CsTomlString>(ExceptionHelper.ThrowThreeOrMoreQuotationMarks);
    }

    private CsTomlString ReadDoubleQuoteSingleLineString()
    {
        Advance(1); // "

        var writer = RecycleByteArrayPoolBufferWriter.Rent();
        var utf8Writer = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref writer);

        var closingQuotationMarks = false;
        while (TryPeek(out var ch))
        {
            if (CsTomlSyntax.IsEscape(ch))
                ExceptionHelper.ThrowNumericConversionFailed(ch);
            else if (CsTomlSyntax.IsDoubleQuoted(ch))
            {
                closingQuotationMarks = true;
                break;
            }
            else if (CsTomlSyntax.IsBackSlash(ch))
            {
                FormatEscapeSequence(ref writer, multiLine: false);
                continue;
            }
            utf8Writer.Write(ch);
            Advance(1);
        }
        if (!closingQuotationMarks)
            ExceptionHelper.ThrowBasicStringsIsNotClosedWithClosingQuotationMarks();

        Advance(1); // "

        try
        {
            if (Utf8Helper.ContainInvalidSequences(writer.WrittenSpan))
                ExceptionHelper.ThrowInvalidCodePoints();

            return new CsTomlString(writer.WrittenSpan, CsTomlString.CsTomlStringType.Basic);
        }
        finally
        {
            RecycleByteArrayPoolBufferWriter.Return(writer);
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
        var writer = RecycleByteArrayPoolBufferWriter.Rent();
        var utf8Writer = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref writer);

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
                            utf8Writer.Write(ch);
                            utf8Writer.Write(linebreakCh);
                            IncreaseLineNumber();
                            continue;
                        }
                        ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
                    }
                    Advance(1);
                    utf8Writer.Write(ch);
                    IncreaseLineNumber();
                    continue;
                }
                else
                {
                    ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
                }
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
                                utf8Writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
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
                    utf8Writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
                }
                continue;
            }
            else if (CsTomlSyntax.IsBackSlash(ch))
            {
                FormatEscapeSequence(ref writer, multiLine: true);
                continue;
            }

            utf8Writer.Write(ch);
            Advance(1);
        }
    BREAK:

        if (!closingThreeQuotationMarks)
            ExceptionHelper.ThrowMultilineBasicStringsIsNotClosedWithClosingThreeQuotationMarks();

        try
        {
            if (Utf8Helper.ContainInvalidSequences(writer.WrittenSpan))
                ExceptionHelper.ThrowInvalidCodePoints();

            return new CsTomlString(writer.WrittenSpan, CsTomlString.CsTomlStringType.MultiLineBasic);
        }
        finally
        {
            RecycleByteArrayPoolBufferWriter.Return(writer);
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
        var singleQuoteCount = 0;
        while (TryPeek(out var first))
        {
            if (CsTomlSyntax.IsSingleQuoted(first) && singleQuoteCount < 3)
            {
                singleQuoteCount++;
                Advance(1);
                continue;
            }
            break;
        }

        switch (singleQuoteCount)
        {
            case 1:
            case 2:
                Rewind(singleQuoteCount);
                return ReadSingleQuoteSingleLineString();
            case 3:
                Rewind(singleQuoteCount);
                return ReadSingleQuoteMultiLineString();
        }

        return ExceptionHelper.NotReturnThrow<CsTomlString>(ExceptionHelper.ThrowIncorrectTomlFormat);
    }

    private CsTomlString ReadSingleQuoteSingleLineString()
    {
        var firstPosition = sequenceReader.Consumed;

        Advance(1); // '
        var closingSingleQuote = false;
        while (TryPeek(out var ch))
        {
            if (CsTomlSyntax.IsEscape(ch))
            {
                if (CsTomlSyntax.IsTab(ch))
                {
                    Advance(1);
                    continue;
                }
                ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
            }
            else if (CsTomlSyntax.IsSingleQuoted(ch))
            {
                closingSingleQuote = true;
                break;
            }
            Advance(1);
        }
        if (!closingSingleQuote)
            ExceptionHelper.ThrowLiteralStringsIsNotClosedWithClosingQuoted();

        var endPosition = sequenceReader.Consumed;
        var length = endPosition - firstPosition - 1;
        Rewind(length);

        if (sequenceReader.TryFullSpan(length, out var bytes))
        {
            if (Utf8Helper.ContainInvalidSequences(bytes))
                ExceptionHelper.ThrowInvalidCodePoints();
            try
            {
                return new CsTomlString(bytes, CsTomlString.CsTomlStringType.Literal);
            }
            finally
            {
                Advance(1);
            }
        }
        else
        {
            var rent = RecycleByteArrayPoolBufferWriter.Rent();
            try
            {
                sequenceReader.TryGetbytes(length, rent);
                if (Utf8Helper.ContainInvalidSequences(rent.WrittenSpan))
                    ExceptionHelper.ThrowInvalidCodePoints();

                return new CsTomlString(rent.WrittenSpan, CsTomlString.CsTomlStringType.Literal);
            }
            finally
            {
                Advance(1);
                RecycleByteArrayPoolBufferWriter.Return(rent);
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
                return new CsTomlString(bytes, CsTomlString.CsTomlStringType.MultiLineLiteral);
            }
            finally
            {
                Advance(3);
            }
        }
        else
        {
            var rent = RecycleByteArrayPoolBufferWriter.Rent();
            try
            {
                sequenceReader.TryGetbytes(length, rent);
                if (Utf8Helper.ContainInvalidSequences(rent.WrittenSpan))
                    ExceptionHelper.ThrowInvalidCodePoints();

                return new CsTomlString(rent.WrittenSpan, CsTomlString.CsTomlStringType.MultiLineLiteral);
            }
            finally
            {
                Advance(3);
                RecycleByteArrayPoolBufferWriter.Return(rent);
            }
        }
    }

    private CsTomlString ReadKeyString(bool isTableHeader = false)
    {
        var firstPosition = sequenceReader.Consumed;
        while (TryPeek(out var ch))
        {
            if (CsTomlSyntax.IsTabOrWhiteSpace(ch))
                break;
            else if (CsTomlSyntax.IsPeriod(ch))
                break;
            else if (CsTomlSyntax.IsEqual(ch))
                break;
            else if (isTableHeader && CsTomlSyntax.IsRightSquareBrackets(ch))
                break;
            if (!CsTomlSyntax.IsBareKey(ch)) 
                ExceptionHelper.ThrowNumericConversionFailed(ch);
            Advance(1);
        }
        var endPosition = sequenceReader.Consumed;
        var length = endPosition - firstPosition;
        Rewind(length);

        if (sequenceReader.TryFullSpan(length, out var bytes))
        {
            return new CsTomlString(bytes, CsTomlString.CsTomlStringType.Unquoted);
        }
        var rent = RecycleByteArrayPoolBufferWriter.Rent();
        try
        {
            sequenceReader.TryGetbytes(length, rent);
            return new CsTomlString(rent.WrittenSpan, CsTomlString.CsTomlStringType.Unquoted);
        }
        finally
        {
            RecycleByteArrayPoolBufferWriter.Return(rent);
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
        CsTomlTableNode? currentNode = inlineTable.RootNode;
        if (TryPeek(out var c) && CsTomlSyntax.IsRightBraces(c)) // empty inlinetable
        {
            Advance(1); // }
            return inlineTable;
        }

        while (Peek())
        {
            var key = ReadKey();
            SkipWhiteSpace();

            if (!TryPeek(out var equalCh)) ExceptionHelper.ThrowEndOfFileReached(); // = or value is nothing
            if (!CsTomlSyntax.IsEqual(equalCh)) ExceptionHelper.ThrowNoEqualAfterTheKey(); // = is nothing

            Advance(1); // skip "="
            SkipWhiteSpace();

            if (!Peek()) ExceptionHelper.ThrowEndOfFileReached(); // value is nothing
            inlineTable.AddKeyValue(key, ReadValue(), currentNode);

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

    private CsTomlBool ReadBool()
    {
        var firstPosition = sequenceReader.Consumed;
        while (TryPeek(out var ch))
        {
            switch(ch)
            {
                case CsTomlSyntax.Symbol.TAB:
                case CsTomlSyntax.Symbol.SPACE:
                case CsTomlSyntax.Symbol.CARRIAGE:
                case CsTomlSyntax.Symbol.LINEFEED:
                case CsTomlSyntax.Symbol.COMMA:
                case CsTomlSyntax.Symbol.RIGHTBRACES:
                case CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET:
                case CsTomlSyntax.Symbol.NUMBERSIGN:
                    goto BREAK;
            }
            Advance(1);
        }
    BREAK:
        var endPosition = sequenceReader.Consumed;
        var length = endPosition - firstPosition;
        Rewind(length);

        if (sequenceReader.TryFullSpan(length, out var bytes))
        {
            var tempReader = new Utf8Reader(bytes);
            ValueFormatter.Deserialize(ref tempReader, (int)length, out bool value);
            return new CsTomlBool(value);
        }
        else
        {
            var rent = RecycleByteArrayPoolBufferWriter.Rent();
            try
            {
                sequenceReader.TryGetbytes(length, rent);
                var tempReader = new Utf8Reader(rent.WrittenSpan);
                ValueFormatter.Deserialize(ref tempReader, (int)length, out bool value);
                return new CsTomlBool(value);
            }
            finally
            {
                RecycleByteArrayPoolBufferWriter.Return(rent);
            }
        }
    }

    private CsTomlValue ReadNumericOrDate()
    {
        var firstPosition = sequenceReader.Consumed;

        // check prefix
        if (TryPeek(out var first))
        {
            // check 0x or 0o or 0b
            if (first == CsTomlSyntax.Number.Value10[0])
            {
                Advance(1);
                if (TryPeek(out var formatsCh))
                {
                    if (CsTomlSyntax.IsLowerAlphabet(formatsCh))
                    {
                        switch (formatsCh)
                        {
                            case CsTomlSyntax.AlphaBet.x:
                                Advance(1);
                                return ReadHexNumeric();
                            case CsTomlSyntax.AlphaBet.o:
                                Advance(1);
                                return ReadOctalNumeric();
                            case CsTomlSyntax.AlphaBet.b:
                                Advance(1);
                                return ReadBinaryNumeric();
                            case CsTomlSyntax.AlphaBet.e: // 0e...
                            case CsTomlSyntax.AlphaBet.E: // 0E...
                                Rewind(sequenceReader.Consumed - firstPosition);
                                return ReadDouble();
                            default:
                                return ExceptionHelper.NotReturnThrow<CsTomlValue, byte>(ExceptionHelper.ThrowIncorrectCompactEscapeCharacters, formatsCh);
                        };
                    }
                    else if (CsTomlSyntax.IsTabOrWhiteSpace(formatsCh))
                    {
                        Rewind(sequenceReader.Consumed - firstPosition);
                        return ReadDecimalNumeric();
                    }
                    else if (CsTomlSyntax.IsNewLine(formatsCh))
                    {
                        Rewind(sequenceReader.Consumed - firstPosition);
                        return ReadDecimalNumeric();
                    }
                }
                Rewind(sequenceReader.Consumed - firstPosition);
            }
        }

        // check localtime or localdatetime
        if (sequenceReader.Length >= sequenceReader.Consumed + CsTomlSyntax.DateTime.LocalTimeFormat.Length)
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

                Rewind(5);
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
            if (CsTomlSyntax.IsTabOrWhiteSpace(ch) || CsTomlSyntax.IsComma(ch) || CsTomlSyntax.IsNewLine(ch))
                break;
            if (CsTomlSyntax.IsPeriod(ch) || CsTomlSyntax.IsExpSymbol(ch))
            {
                Rewind(sequenceReader.Consumed - firstPosition);
                return ReadDouble();
            }
            if (ch == CsTomlSyntax.AlphaBet.i || ch == CsTomlSyntax.AlphaBet.n)
            {
                Rewind(sequenceReader.Consumed - firstPosition);
                return ReadDoubleInfOrNan();
            }
            Advance(1);
        }

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

    private CsTomlInt64 ReadDecimalNumeric()
    {
        var writer = new SpanWriter(stackalloc byte[32]);

        var plusOrMinusSign = false;
        if (TryPeek(out var plusOrMinusCh) && CsTomlSyntax.IsPlusOrMinusSign(plusOrMinusCh))
        {
            plusOrMinusSign = true;
            writer.Write(plusOrMinusCh);
            Advance(1);
        }

        if (TryPeek(out var firstCh))
        {
            if (CsTomlSyntax.IsUnderScore(firstCh))
            {
                ExceptionHelper.ThrowUnderscoreUsedConsecutively();
            }
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
                    return ExceptionHelper.NotReturnThrow<CsTomlInt64, byte>(ExceptionHelper.ThrowEscapeCharactersIncluded, ch);
            }

        BREAK:
            break;

        }

        if (underscore)
            ExceptionHelper.ThrowUnderscoreIsUsedAtTheEnd();

        var writingSpan = writer.WrittenSpan;
        if (plusOrMinusSign)
        { 
            if (writingSpan.Length > 2)
            {
                // +00 or -01
                if (writingSpan[1] == CsTomlSyntax.Number.Value10[0])
                    ExceptionHelper.ThrowIncorrectTomlIntegerFormat();
            }
        }
        else
        {
            if (writingSpan.Length > 1)
            {
                // 00 or 01
                if (writingSpan[0] == CsTomlSyntax.Number.Value10[0])
                    ExceptionHelper.ThrowIncorrectTomlIntegerFormat();

            }
        }

        var tempReader = new Utf8Reader(writingSpan);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out long value);
        return new CsTomlInt64(value);
    }

    private CsTomlInt64 ReadHexNumeric()
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
        writer.Write(CsTomlSyntax.Number.Value10[0]);
        writer.Write(CsTomlSyntax.AlphaBet.x);

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
                    return ExceptionHelper.NotReturnThrow<CsTomlInt64, byte>(ExceptionHelper.ThrowEscapeCharactersIncluded, ch);
            }

        BREAK:
            break;
        }

        if (underscore)
            ExceptionHelper.ThrowUnderscoreIsUsedAtTheEnd();

        var tempReader = new Utf8Reader(writer.WrittenSpan);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out long value);
        return new CsTomlInt64(value);
    }

    private CsTomlInt64 ReadOctalNumeric()
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
        writer.Write(CsTomlSyntax.Number.Value10[0]);
        writer.Write(CsTomlSyntax.AlphaBet.o);

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
                    return ExceptionHelper.NotReturnThrow<CsTomlInt64, byte>(ExceptionHelper.ThrowEscapeCharactersIncluded, ch);
            }

        BREAK:
            break;
        }

        if (underscore)
            ExceptionHelper.ThrowUnderscoreIsUsedAtTheEnd();

        var tempReader = new Utf8Reader(writer.WrittenSpan);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out long value);
        return new CsTomlInt64(value);
    }

    private CsTomlInt64 ReadBinaryNumeric()
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
        writer.Write(CsTomlSyntax.Number.Value10[0]);
        writer.Write(CsTomlSyntax.AlphaBet.b);

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
                    return ExceptionHelper.NotReturnThrow<CsTomlInt64, byte>(ExceptionHelper.ThrowEscapeCharactersIncluded, ch);
            }

        BREAK:
            break;
        }

        if (underscore)
            ExceptionHelper.ThrowUnderscoreIsUsedAtTheEnd();

        var tempReader = new Utf8Reader(writer.WrittenSpan);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out long value);
        return new CsTomlInt64(value);
    }

    private CsTomlDouble ReadDouble()
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
                    return ExceptionHelper.NotReturnThrow<CsTomlDouble>(ExceptionHelper.ThrowUnderscoreUsedFirst);
                case CsTomlSyntax.Symbol.PERIOD:
                    return ExceptionHelper.NotReturnThrow<CsTomlDouble>(ExceptionHelper.ThrowPeriodUsedFirst);
                case CsTomlSyntax.AlphaBet.i:
                case CsTomlSyntax.AlphaBet.n:
                    if (CsTomlSyntax.IsPlusOrMinusSign(plusOrMinusCh)) Rewind(1);
                    return ReadDoubleInfOrNan();
                case var zero when zero == CsTomlSyntax.Number.Value10[0]:
                    Advance(1);
                    if (TryPeek(out var secondNumberCh))
                    {
                        switch(secondNumberCh)
                        {
                            case CsTomlSyntax.Symbol.PERIOD: // 0.1 ..
                            case CsTomlSyntax.AlphaBet.e: // 0e...
                            case CsTomlSyntax.AlphaBet.E: // 0E...
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
        var period = false;
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
                case CsTomlSyntax.Symbol.PERIOD:
                    if (!number) ExceptionHelper.ThrowPeriodUsedWhereNotSurroundedByNumbers();
                    if (period) ExceptionHelper.ThrowPeriodUsedMoreThanOnce();
                    if (exp) ExceptionHelper.ThrowDecimalPointIsPresentAfterTheExponentialPartE();
                    number = false;
                    period = true;
                    writer.Write(ch);
                    Advance(1);
                    continue;
                case CsTomlSyntax.AlphaBet.e:
                case CsTomlSyntax.AlphaBet.E:
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
                    return ExceptionHelper.NotReturnThrow<CsTomlDouble, byte>(ExceptionHelper.ThrowEscapeCharactersIncluded, ch);
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
            case CsTomlSyntax.Symbol.PERIOD:
                ExceptionHelper.ThrowPeriodIsUsedAtTheEnd();
                break;
            case CsTomlSyntax.AlphaBet.e:
            case CsTomlSyntax.AlphaBet.E:
                ExceptionHelper.ThrowExponentPartIsUsedAtTheEnd();
                break;
            case CsTomlSyntax.Symbol.PLUS:
            case CsTomlSyntax.Symbol.MINUS:
                ExceptionHelper.ThrowIncorrectPositivAndNegativeSigns();
                break;
        }

        var tempReader = new Utf8Reader(writingSpan);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out double value);
        return new CsTomlDouble(value);
    }

    private CsTomlDouble ReadDoubleInfOrNan()
    {
        if (sequenceReader.Length < sequenceReader.Consumed + 3) ExceptionHelper.ThrowIncorrectTomlFloatFormat();

        var length = 3;
        Utf8Reader tempReader;
        ArrayPoolBufferWriter<byte>? writer = null;
        try
        {
            if (sequenceReader.TryFullSpan(length, out var bytes))
            {
                tempReader = new Utf8Reader(bytes);
            }
            else
            {
                writer = RecycleByteArrayPoolBufferWriter.Rent();
                sequenceReader.TryGetbytes(length, writer);
                tempReader = new Utf8Reader(writer.WrittenSpan);
            }

            if (tempReader[0] == CsTomlSyntax.AlphaBet.i &&
                tempReader[1] == CsTomlSyntax.AlphaBet.n &&
                tempReader[2] == CsTomlSyntax.AlphaBet.f)
            {
                return CsTomlDouble.Inf;
            }
            else if (tempReader[0] == CsTomlSyntax.AlphaBet.n &&
                tempReader[1] == CsTomlSyntax.AlphaBet.a &&
                tempReader[2] == CsTomlSyntax.AlphaBet.n)
            {
                return CsTomlDouble.Nan;
            }
        }
        finally
        {
            if (writer != null)
                RecycleByteArrayPoolBufferWriter.Return(writer);
            writer = null;
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
                writer = RecycleByteArrayPoolBufferWriter.Rent();
                sequenceReader.TryGetbytes(length, writer);
                tempReader = new Utf8Reader(writer.WrittenSpan);
            }

            if (CsTomlSyntax.IsPlusSign(tempReader[0]))
            {
                if (tempReader[1] == CsTomlSyntax.AlphaBet.i &&
                    tempReader[2] == CsTomlSyntax.AlphaBet.n &&
                    tempReader[3] == CsTomlSyntax.AlphaBet.f)
                {
                    return CsTomlDouble.Inf;
                }
                else if (tempReader[1] == CsTomlSyntax.AlphaBet.n &&
                    tempReader[2] == CsTomlSyntax.AlphaBet.a &&
                    tempReader[3] == CsTomlSyntax.AlphaBet.n)
                {
                    return CsTomlDouble.Nan;
                }
            }
            else if (CsTomlSyntax.IsMinusSign(tempReader[0]))
            {
                if (tempReader[1] == CsTomlSyntax.AlphaBet.i &&
                    tempReader[2] == CsTomlSyntax.AlphaBet.n &&
                    tempReader[3] == CsTomlSyntax.AlphaBet.f)
                {
                    return CsTomlDouble.NInf;
                }
                else if (tempReader[1] == CsTomlSyntax.AlphaBet.n &&
                    tempReader[2] == CsTomlSyntax.AlphaBet.a &&
                    tempReader[3] == CsTomlSyntax.AlphaBet.n)
                {
                    return CsTomlDouble.PNan;
                }
            }
            Rewind(4);
            return ExceptionHelper.NotReturnThrow<CsTomlDouble>(ExceptionHelper.ThrowIncorrectTomlFloatFormat);
        }
        finally
        {
            if (writer != null)
                RecycleByteArrayPoolBufferWriter.Return(writer);
            writer = null;
        }
    }

    private CsTomlValue ReadLocalDateTimeOrOffset(ReadOnlySpan<byte> bytes)
    {
        // local date
        if (bytes.Length == CsTomlSyntax.DateTime.LocalDateFormat.Length)
        {
            return ReadLocalDate(bytes);
        }

        // offset datetime
        if (bytes.Length >= CsTomlSyntax.DateTime.OffsetDateTimeZFormat.Length)
        {
            if (bytes[^1] == CsTomlSyntax.AlphaBet.Z || bytes[^1] == CsTomlSyntax.AlphaBet.z)
            {
                return ReadOffsetDateTime(bytes);
            }
            else if (CsTomlSyntax.IsPlusOrMinusSign(bytes[19]))
            {
                return ReadOffsetDateTimeByNumber(bytes);
            }
            else if (CsTomlSyntax.IsPeriod(bytes[19]))
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
        if (bytes.Length < CsTomlSyntax.DateTime.LocalDateTimeFormat.Length) 
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
        if (!(CsTomlSyntax.IsWhiteSpace(bytes[10]) || bytes[10] == CsTomlSyntax.AlphaBet.T || bytes[10] == CsTomlSyntax.AlphaBet.t)) 
            ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[11])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[12])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsColon(bytes[13])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[14])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[15])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsColon(bytes[16])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[17])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[18])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();

        if (bytes.Length > CsTomlSyntax.DateTime.LocalDateTimeFormat.Length)
        {
            if (!CsTomlSyntax.IsPeriod(bytes[19])) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
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
        if (bytes.Length < CsTomlSyntax.DateTime.LocalDateFormat.Length) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
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
        if (bytes.Length < CsTomlSyntax.DateTime.LocalTimeFormat.Length) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[0])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[1])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!CsTomlSyntax.IsColon(bytes[2])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[3])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[4])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!CsTomlSyntax.IsColon(bytes[5])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[6])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!CsTomlSyntax.IsNumber(bytes[7])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();

        if (bytes.Length > CsTomlSyntax.DateTime.LocalTimeFormat.Length)
        {
            if (!CsTomlSyntax.IsPeriod(bytes[8])) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
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
        if (bytes.Length < CsTomlSyntax.DateTime.OffsetDateTimeZFormat.Length) 
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        if (!(bytes[^1] == CsTomlSyntax.AlphaBet.Z || bytes[^1] == CsTomlSyntax.AlphaBet.z)) 
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
        if (!(CsTomlSyntax.IsWhiteSpace(bytes[10]) || bytes[10] == CsTomlSyntax.AlphaBet.T || bytes[10] == CsTomlSyntax.AlphaBet.t)) 
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
        return new CsTomlOffsetDateTime(value, false);
    }

    private CsTomlOffsetDateTime ReadOffsetDateTimeByNumber(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < CsTomlSyntax.DateTime.OffsetDateTimeZFormat.Length) 
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
        if (!(CsTomlSyntax.IsWhiteSpace(bytes[10]) || bytes[10] == CsTomlSyntax.AlphaBet.T || bytes[10] == CsTomlSyntax.AlphaBet.t)) 
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
        else if (CsTomlSyntax.IsPeriod(bytes[19]))
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
        return new CsTomlOffsetDateTime(value, true);
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

