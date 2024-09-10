using CsToml.Error;
using CsToml.Extension;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

    [DebuggerStepThrough]
    public CsTomlReader(ReadOnlySpan<byte> tomlText)
    {
        sequenceReader = new Utf8SequenceReader(tomlText);
        LineNumber = 1;
    }

    [DebuggerStepThrough]
    public CsTomlReader(ReadOnlySequence<byte> tomlText)
    {
        sequenceReader = new Utf8SequenceReader(tomlText);
        LineNumber = 1;
    }

    public TomlString ReadComment()
    {
        Advance(1); // #

        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            while (TryPeek(out var ch))
            {
                if (TomlCodes.IsCr(ch))
                {
                    if (TryPeek(1, out var lf) && TomlCodes.IsLf(lf))
                    {
                        goto BREAK;
                    }
                    ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
                }
                else if (TomlCodes.IsLf(ch))
                {
                    goto BREAK;
                }
                else if (TomlCodes.IsEscape(ch))
                {
                    ExceptionHelper.ThrowNumericConversionFailed(ch);
                }
                bufferWriter.Write(ch);
                Advance(1);
            }

        BREAK:
            if (Utf8Helper.ContainInvalidSequences(bufferWriter.WrittenSpan))
                ExceptionHelper.ThrowInvalidCodePoints();

            return TomlString.Parse(bufferWriter.WrittenSpan, CsTomlStringType.Unquoted);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }

    public void ReadKey(ref ExtendableArray<TomlDottedKey> key)
    {
        SkipWhiteSpace();
        if (!Peek())
            ExceptionHelper.ThrowEndOfFileReached();

        var dot = true;
        while (TryPeek(out var c))
        {
            if (TomlCodes.IsBareKey(c))
            {
                if (!dot) ExceptionHelper.ThrowDottedKeysAreNotJoinedByDots();
                dot = false;
                key.Add(ReadUnquotedString(false));
                continue;
            }
            switch (c)
            {
                case TomlCodes.Symbol.TAB:
                case TomlCodes.Symbol.SPACE:
                    SkipWhiteSpace();
                    continue;
                case TomlCodes.Symbol.EQUAL:
                    if (key.Count == 0)
                    {
                        ExceptionHelper.ThrowBareKeyIsEmpty();
                    }
                    goto BREAK;
                case TomlCodes.Symbol.DOT:
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
                case TomlCodes.Symbol.DOUBLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowIncorrectTomlFormat();
                    dot = false;
                    key.Add(ReadDoubleQuoteSingleLineString<TomlDottedKey>());
                    continue;
                case TomlCodes.Symbol.SINGLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowIncorrectTomlFormat();
                    dot = false;
                    key.Add(ReadSingleQuoteSingleLineString<TomlDottedKey>());
                    continue;
                default:
                    ExceptionHelper.ThrowIncorrectTomlFormat();
                    break;
            }

        BREAK:
            break;
        }
    }

    public void ReadTableHeader(ref ExtendableArray<TomlDottedKey> tableHeaderKey)
    {
        Advance(1); // [

        SkipWhiteSpace();
        if (!Peek())
            ExceptionHelper.ThrowEndOfFileReached();

        var dot = true;
        var closingRightRightSquareBracket = false;
        while (TryPeek(out var c))
        {
            if (TomlCodes.IsBareKey(c))
            {
                if (!dot) ExceptionHelper.ThrowIncorrectTomlFormat();
                dot = false;
                tableHeaderKey.Add(ReadUnquotedString(true));
                continue;
            }
            switch (c)
            {
                case TomlCodes.Symbol.TAB:
                case TomlCodes.Symbol.SPACE:
                    SkipWhiteSpace();
                    continue;
                case TomlCodes.Symbol.DOT:
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
                case TomlCodes.Symbol.DOUBLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowIncorrectTomlFormat();
                    dot = false;
                    tableHeaderKey.Add(ReadDoubleQuoteSingleLineString<TomlDottedKey>());
                    continue;
                case TomlCodes.Symbol.SINGLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowIncorrectTomlFormat();
                    dot = false;
                    tableHeaderKey.Add(ReadSingleQuoteSingleLineString<TomlDottedKey>());
                    continue;
                case TomlCodes.Symbol.RIGHTSQUAREBRACKET:
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

    public void ReadArrayOfTablesHeader(ref ExtendableArray<TomlDottedKey> arrayOfTablesHeaderKey)
    {
        Advance(2); // [[
        SkipWhiteSpace();

        if (!Peek())
            ExceptionHelper.ThrowEndOfFileReached();

        var dot = true;
        var closingRightRightSquareBracket = false;
        while (TryPeek(out var c))
        {
            if (TomlCodes.IsBareKey(c))
            {
                if (!dot) ExceptionHelper.ThrowIncorrectTomlFormat();
                dot = false;
                arrayOfTablesHeaderKey.Add(ReadUnquotedString(true));
                continue;
            }
            switch (c)
            {
                case TomlCodes.Symbol.TAB:
                case TomlCodes.Symbol.SPACE:
                    SkipWhiteSpace();
                    continue;
                case TomlCodes.Symbol.DOT:
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
                case TomlCodes.Symbol.DOUBLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowIncorrectTomlFormat();
                    dot = false;
                    arrayOfTablesHeaderKey.Add(ReadDoubleQuoteSingleLineString<TomlDottedKey>());
                    continue;
                case TomlCodes.Symbol.SINGLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowIncorrectTomlFormat();
                    dot = false;
                    arrayOfTablesHeaderKey.Add(ReadSingleQuoteSingleLineString<TomlDottedKey>());
                    continue;
                case TomlCodes.Symbol.RIGHTSQUAREBRACKET:
                    Advance(1);
                    if (TryPeek(out var tableHeaderArrayEndCh) && TomlCodes.IsRightSquareBrackets(tableHeaderArrayEndCh))
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

    public TomlValue ReadValue()
    {
        if (!TryPeek(out var ch)) ExceptionHelper.ThrowEndOfFileReached();  // value is nothing

        switch(ch)
        {
            case TomlCodes.Symbol.DOUBLEQUOTED:
                return ReadDoubleQuoteString();
            case TomlCodes.Symbol.SINGLEQUOTED:
                return ReadSingleQuoteString();
            case TomlCodes.Symbol.PLUS:
            case TomlCodes.Symbol.MINUS:
                return ReadNumbericValueIfLeadingSign();
            case TomlCodes.Number.Zero:
                return ReadNumericValueOrDateIfLeadingZero();
            case TomlCodes.Number.One:
            case TomlCodes.Number.Two:
            case TomlCodes.Number.Three:
            case TomlCodes.Number.Four:
            case TomlCodes.Number.Five:
            case TomlCodes.Number.Six:
            case TomlCodes.Number.Seven:
            case TomlCodes.Number.Eight:
            case TomlCodes.Number.Nine:
                return ReadNumericValueOrDate();
            case TomlCodes.Symbol.LEFTSQUAREBRACKET:
                return ReadArray();
            case TomlCodes.Alphabet.f:
                return ReadBool(false);
            case TomlCodes.Alphabet.t:
                return ReadBool(true);
            case TomlCodes.Alphabet.i:
                return ReadDoubleInf(false);
            case TomlCodes.Alphabet.n:
                return ReadDoubleNan(false);
            case TomlCodes.Symbol.LEFTBRACES:
                return ReadInlineTable();
        }

        ExceptionHelper.NotReturnThrow<TomlValue>(ExceptionHelper.ThrowIncorrectTomlFormat);
        return default!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipWhiteSpace()
    {
        var currentSpan = sequenceReader.UnreadSpan;
        while (Peek())
        {
            for (var index = 0; index < currentSpan.Length; index++)
            {
                if (!TomlCodes.IsTabOrWhiteSpace(currentSpan.At(index)))
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
                case TomlCodes.Symbol.TAB:
                case TomlCodes.Symbol.SPACE:
                    Advance(1);
                    continue;
                case TomlCodes.Symbol.LINEFEED:
                    Advance(1);
                    IncreaseLineNumber();
                    continue;
                case TomlCodes.Symbol.CARRIAGE:
                    if (TryPeek(1, out var linebreakCh) && TomlCodes.IsLf(linebreakCh))
                    {
                        Advance(2);
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
        if (TomlCodes.IsLf(ch))
        {
            Advance(1);
            IncreaseLineNumber();
            return true;
        }
        else if (TomlCodes.IsCr(ch))
        {
            if (TryPeek(1, out var linebreakCh) && TomlCodes.IsLf(linebreakCh))
            {
                Advance(2);
                IncreaseLineNumber();
                return true;
            }
            if (TomlCodes.IsEscape(linebreakCh))
            {
                if (throwControlCharacter)
                    ExceptionHelper.ThrowEscapeCharactersIncluded(linebreakCh);
                return false;
            }
        }

        if (TomlCodes.IsEscape(ch))
        {
            if (throwControlCharacter)
                ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
        }

        return false;
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IncreaseLineNumber()
    {
        if (Peek()) LineNumber++;
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
    public readonly bool TryPeek(long offset, out byte value)
        => sequenceReader.TryPeek(offset, out value);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Peek()
        => !sequenceReader.End;

    internal TomlString ReadDoubleQuoteString()
    {
        var doubleQuoteCount = 0;
        var index = 0;
        while(TryPeek(index++, out var ch))
        {
            if (TomlCodes.IsDoubleQuoted(ch))
            {
                doubleQuoteCount++;
            }
            else
            {
                goto BREAK;
            }
        }

    BREAK:
        switch (doubleQuoteCount)
        {
            case 1:
            case 2:
                return ReadDoubleQuoteSingleLineString<TomlString>();
            case 3:
            case 4:
            case 5:
                return ReadDoubleQuoteMultiLineString();
            case 6: // first(3) + end(3) = 6 
                Advance(6);
                return new TomlString(string.Empty, CsTomlStringType.MultiLineBasic);
            case 7: // first(3) + one adjacent mark(1) + end(3) = 7
                Advance(7);
                return new TomlString("\"", CsTomlStringType.MultiLineBasic);
            case 8: // first(3) + two adjacent mark(2) + end(3) = 8
                Advance(8);
                return new TomlString("\"\"", CsTomlStringType.MultiLineBasic);
        }

        return ExceptionHelper.NotReturnThrow<TomlString>(ExceptionHelper.ThrowThreeOrMoreQuotationMarks);
    }

    internal T ReadDoubleQuoteSingleLineString<T>()
        where T : TomlValue, ITomlStringParser<T>
    {
        Advance(1); // "

        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            var currentSpan = sequenceReader.UnreadSpan;
            var closingQuotationMarks = false;

            while (this.Peek())
            {
                for (var index = 0; index < currentSpan.Length; index++)
                {
                    ref var ch = ref currentSpan.At(index);
                    if (TomlCodes.IsEscape(ch))
                    {
                        ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
                    }
                    else if (TomlCodes.IsDoubleQuoted(ch))
                    {
                        Advance(index + 1);
                        closingQuotationMarks = true;
                        goto BREAK;
                    }
                    else if (TomlCodes.IsBackSlash(ch))
                    {
                        Advance(index);
                        ParseEscapeSequence(bufferWriter, multiLine: false);
                        goto RESET;
                    }
                    bufferWriter.Write(ch);
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

            return T.Parse(bufferWriter.WrittenSpan, CsTomlStringType.Basic);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }

    private TomlString ReadDoubleQuoteMultiLineString()
    {
        Advance(3); // """

        if (TryPeek(out var newlineCh) && TomlCodes.IsNewLine(newlineCh))
        {
            if (TomlCodes.IsCr(newlineCh))
            {
                Advance(1);
                if (TryPeek(out var linebreakCh) && TomlCodes.IsLf(linebreakCh))
                {
                    Advance(1);
                    IncreaseLineNumber();
                }
            }
            else if (TomlCodes.IsLf(newlineCh))
            {
                Advance(1);
                IncreaseLineNumber();
            }
        }

        var closingThreeQuotationMarks = false;
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();

        while (TryPeek(out var ch))
        {
            if (TomlCodes.IsEscape(ch))
            {
                if (TomlCodes.IsLf(ch))
                {
                    Advance(1);
                    bufferWriter.Write(ch);
                    IncreaseLineNumber();
                    continue;
                }
                else if (TomlCodes.IsCr(ch))
                {
                    if (TryPeek(1, out var lf) && TomlCodes.IsLf(lf))
                    {
                        Advance(2);
                        bufferWriter.Write(ch);
                        bufferWriter.Write(lf);
                        IncreaseLineNumber();
                        continue;
                    }
                }
                ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
            }
            else if (TomlCodes.IsDoubleQuoted(ch))
            {
                var doubleQuotedCount = 1;
                Advance(1);
                while (TryPeek(out var ch2) && TomlCodes.IsDoubleQuoted(ch2))
                {
                    Advance(1);
                    doubleQuotedCount++;
                    if (doubleQuotedCount == 3)
                    {
                        while (TryPeek(out var ch3))
                        {
                            if (TomlCodes.IsDoubleQuoted(ch3))
                            {
                                if (++doubleQuotedCount >= 6)
                                    ExceptionHelper.ThrowConsecutiveQuotationMarksOf3();
                                Advance(1);
                                bufferWriter.Write(TomlCodes.Symbol.DOUBLEQUOTED);
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
                    bufferWriter.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                }
                continue;
            }
            else if (TomlCodes.IsBackSlash(ch))
            {
                ParseEscapeSequence(bufferWriter, multiLine: true);
                continue;
            }

            bufferWriter.Write(ch);
            Advance(1);
        }
    BREAK:

        if (!closingThreeQuotationMarks)
            ExceptionHelper.ThrowMultilineBasicStringsIsNotClosedWithClosingThreeQuotationMarks();

        try
        {
            if (Utf8Helper.ContainInvalidSequences(bufferWriter.WrittenSpan))
                ExceptionHelper.ThrowInvalidCodePoints();

            return TomlString.Parse(bufferWriter.WrittenSpan, CsTomlStringType.MultiLineBasic);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }

    }

    private void ParseEscapeSequence(ArrayPoolBufferWriter<byte> bufferWriter, bool multiLine)
    {
        Advance(1); // /

        var result = TomlCodes.TryParseEscapeSequence(ref sequenceReader, bufferWriter, multiLine, true);
        switch(result)
        {
            case EscapeSequenceResult.Success:
                return;
            case EscapeSequenceResult.Failure:
                ExceptionHelper.ThrowInvalidEscapeSequence();
                return;
            case EscapeSequenceResult.Unescaped:
                SkipWhiteSpace();
                if (TryPeek(out var ch) && !TomlCodes.IsNewLine(ch))
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

    internal TomlString ReadSingleQuoteString()
    {
        var singleQuoteCount = 0;
        var index = 0;
        while (TryPeek(index++, out var ch))
        {
            if (TomlCodes.IsSingleQuoted(ch))
            {
                singleQuoteCount++;
            }
            else
            {
                goto BREAK;
            }
        }

    BREAK:
        switch (singleQuoteCount)
        {
            case 1:
            case 2:
                return ReadSingleQuoteSingleLineString<TomlString>();
            case 3:
            case 4:
            case 5:
                return ReadSingleQuoteMultiLineString();
            case 6: // first(3) + end(3) = 6 
                Advance(6);
                return new TomlString(string.Empty, CsTomlStringType.MultiLineLiteral);
            case 7: // first(3) + one adjacent mark(1) + end(3) = 7
                Advance(7);
                return new TomlString("'", CsTomlStringType.MultiLineLiteral);
            case 8: // first(3) + two adjacent mark(2) + end(3) = 8
                Advance(8);
                return new TomlString("''", CsTomlStringType.MultiLineLiteral);
        }

        return ExceptionHelper.NotReturnThrow<TomlString>(ExceptionHelper.ThrowConsecutiveSingleQuotationMarksOf3);
    }

    internal T ReadSingleQuoteSingleLineString<T>()
        where T : TomlValue, ITomlStringParser<T>
    {
        Advance(1); // '

        var currentSpan = sequenceReader.UnreadSpan;
        var closingQuotationMarks = false;
        var fullSpan = true;
        var totalLength = 0;

        ArrayPoolBufferWriter<byte>? bufferWriter = default;
        while (this.Peek())
        {
            for (var index = 0; index < currentSpan.Length; index++)
            {
                ref var ch = ref currentSpan.At(index);
                if (TomlCodes.IsEscape(ch))
                {
                    ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
                }
                else if (TomlCodes.IsSingleQuoted(ch))
                {
                    if (!fullSpan)
                        bufferWriter!.Write(currentSpan.Slice(0, index));

                    Advance(index + 1);
                    closingQuotationMarks = true;
                    goto BREAK;
                }
                totalLength++;
            }
            bufferWriter ??= RecycleArrayPoolBufferWriter<byte>.Rent();
            bufferWriter.Write(currentSpan);
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
            return T.Parse(currentSpan[..totalLength], CsTomlStringType.Literal);
        }

        try
        {
            if (Utf8Helper.ContainInvalidSequences(bufferWriter!.WrittenSpan))
                ExceptionHelper.ThrowInvalidCodePoints();
            return T.Parse(bufferWriter!.WrittenSpan, CsTomlStringType.Literal);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter!);
        }
    }

    private TomlString ReadSingleQuoteMultiLineString()
    {
        Advance(3); // '''

        if (TryPeek(out var newlineCh))
        {
            if (TomlCodes.IsCr(newlineCh))
            {
                if (TryPeek(1, out var linebreakCh) && TomlCodes.IsLf(linebreakCh))
                {
                    IncreaseLineNumber();
                    Advance(2);
                }
            }
            else if (TomlCodes.IsLf(newlineCh))
            {
                IncreaseLineNumber();
                Advance(1);
            }
        }

        var currentSpan = sequenceReader.UnreadSpan;
        var fullSpan = true;
        var totalLength = 0;
        var singleQuotedContinuousCount = 0;

        ArrayPoolBufferWriter<byte>? bufferWriter = default;
        while (this.Peek())
        {
            for (var index = 0; index < currentSpan.Length; index++)
            {
                ref var ch = ref currentSpan.At(index);
                if (TomlCodes.IsEscape(ch))
                {
                    switch (ch)
                    {
                        case TomlCodes.Symbol.LINEFEED:
                            singleQuotedContinuousCount = 0;
                            totalLength++;
                            IncreaseLineNumber();
                            continue;
                        case TomlCodes.Symbol.CARRIAGE:
                            if (TryPeek(index + 1, out var lf) && TomlCodes.IsLf(lf))
                            {
                                singleQuotedContinuousCount = 0;
                                totalLength += 2;
                                if (++index >= currentSpan.Length)
                                {
                                    continue;
                                }
                                IncreaseLineNumber();
                                continue;
                            }
                            ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
                            return default;
                        case TomlCodes.Symbol.TAB:
                            singleQuotedContinuousCount = 0;
                            totalLength++;
                            continue;
                        default:
                            ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
                            return default;
                    }
                }
                else if (TomlCodes.IsSingleQuoted(ch))
                {
                    singleQuotedContinuousCount++;
                    if (singleQuotedContinuousCount == 3)
                    {
                        if (!fullSpan)
                            bufferWriter!.Write(currentSpan.Slice(0, index));
                        Advance(index + 1);
                        goto BREAK;
                    }
                    totalLength++;
                }
                else
                {
                    singleQuotedContinuousCount = 0;
                    totalLength++;
                }
            }
            bufferWriter ??= RecycleArrayPoolBufferWriter<byte>.Rent();
            bufferWriter.Write(currentSpan);
            Advance(currentSpan.Length);
            currentSpan = sequenceReader.CurrentSpan;
            fullSpan = false;
            continue;
        }
    BREAK:
        // not closed
        if (singleQuotedContinuousCount < 3)
        {
            ExceptionHelper.ThrowTheEndIsNotClosedInThreeSingleQuotationMarks();
            return default!;
        }

        var singleQuotedContinuousCountAtTheEnd = 0;
        if (TryPeek(out var quoteCh) && TomlCodes.IsSingleQuoted(quoteCh))
        {
            singleQuotedContinuousCountAtTheEnd++;
            bufferWriter?.Write(quoteCh);
            if (TryPeek(1, out quoteCh) && TomlCodes.IsSingleQuoted(quoteCh))
            {
                singleQuotedContinuousCountAtTheEnd++;
                bufferWriter?.Write(quoteCh);
                if (TryPeek(2, out quoteCh) && TomlCodes.IsSingleQuoted(quoteCh))
                {
                    if (bufferWriter != null)
                        RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter!);
                    ExceptionHelper.ThrowConsecutiveSingleQuotationMarksOf3();
                }
            }
            Advance(singleQuotedContinuousCountAtTheEnd);
        }

        if (fullSpan)
        {
            return TomlString.Parse(currentSpan.Slice(0, totalLength + singleQuotedContinuousCountAtTheEnd - 2), CsTomlStringType.MultiLineLiteral);
        }

        try
        {
            var written = bufferWriter!.WrittenSpan;
            var written2 = written.Slice(0, written.Length - 2);
            if (Utf8Helper.ContainInvalidSequences(written2))
                ExceptionHelper.ThrowInvalidCodePoints();

            return TomlString.Parse(written2, CsTomlStringType.MultiLineLiteral);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter!);
        }
    }

    internal TomlDottedKey ReadUnquotedString(bool isTableHeader = false)
    {
        var currentSpan = sequenceReader.UnreadSpan;
        var fullSpan = true;
        var totalLength = 0;
        ArrayPoolBufferWriter<byte>? bufferWriter = default;

        while (this.Peek())
        {
            for (var index = 0; index < currentSpan.Length; index++)
            {
                ref var ch = ref currentSpan.At(index);
                switch (ch)
                {
                    case TomlCodes.Symbol.TAB:
                    case TomlCodes.Symbol.SPACE:
                    case TomlCodes.Symbol.DOT:
                    case TomlCodes.Symbol.EQUAL:
                        if (!fullSpan)
                        {
                            bufferWriter!.Write(currentSpan.Slice(0, index));
                        }
                        Advance(index);
                        goto BREAK;
                    case TomlCodes.Symbol.RIGHTSQUAREBRACKET:
                        if (isTableHeader)
                        {
                            if (!fullSpan)
                            {
                                bufferWriter!.Write(currentSpan.Slice(0, index));
                            }
                            Advance(index);
                            goto BREAK;
                        }
                        break;
                    default:
                        if (!TomlCodes.IsBareKey(ch))
                            ExceptionHelper.ThrowNumericConversionFailed(ch);
                        break;
                }
                totalLength++;
            }
            bufferWriter ??= RecycleArrayPoolBufferWriter<byte>.Rent();
            bufferWriter.Write(currentSpan);
            Advance(currentSpan.Length);
            currentSpan = sequenceReader.CurrentSpan;
            fullSpan = false;
        }

    BREAK:
        if (fullSpan)
        {
            return new TomlDottedKey(currentSpan[..totalLength], CsTomlStringType.Unquoted);
        }
        try
        {
            var written = bufferWriter!.WrittenSpan;
            if (Utf8Helper.ContainInvalidSequences(written))
                ExceptionHelper.ThrowInvalidCodePoints();
            return new TomlDottedKey(written, CsTomlStringType.Unquoted);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter!);
        }
    }

    internal TomlArray ReadArray()
    {
        Advance(1); // [

        var array = new TomlArray();
        var comma = true;
        var closingBracket = false;
        while (TryPeek(out var ch))
        {
            switch (ch)
            {
                case TomlCodes.Symbol.LEFTSQUAREBRACKET:
                    comma = false;
                    array.Add(ReadArray());
                    break;
                case TomlCodes.Symbol.TAB:
                case TomlCodes.Symbol.SPACE:
                    SkipWhiteSpace();
                    break;
                case TomlCodes.Symbol.COMMA:
                    if (comma) ExceptionHelper.ThrowIncorrectTomlFormat();
                    comma = true;
                    Advance(1);
                    break;
                case TomlCodes.Symbol.CARRIAGE:
                    Advance(1);
                    if (TryPeek(out var linebreakCh) && TomlCodes.IsLf(linebreakCh))
                    {
                        Advance(1);
                        IncreaseLineNumber();
                        continue;
                    }
                    return ExceptionHelper.NotReturnThrow<TomlArray, byte>(ExceptionHelper.ThrowEscapeCharactersIncluded, ch);
                case TomlCodes.Symbol.LINEFEED:
                    Advance(1);
                    IncreaseLineNumber();
                    continue;
                case TomlCodes.Symbol.RIGHTSQUAREBRACKET:
                    closingBracket = true;
                    Advance(1);
                    goto BREAK;
                case TomlCodes.Symbol.NUMBERSIGN:
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

    private TomlInlineTable ReadInlineTable()
    {
        Advance(1); // {
        SkipWhiteSpace();

        var inlineTable = new TomlInlineTable();
        if (TryPeek(out var c) && TomlCodes.IsRightBraces(c)) // empty inlinetable
        {
            Advance(1); // }
            return inlineTable;
        }

        TomlTableNode? currentNode = inlineTable.RootNode;
        while (Peek())
        {
            var dotKeysForInlineTable = new ExtendableArray<TomlDottedKey>(16);
            TomlTableNode node = TomlTableNode.Empty;
            try
            {
                ReadKey(ref dotKeysForInlineTable);
                Advance(1); // skip "="
                SkipWhiteSpace();
                // Register only the key, then set the value.
                node = inlineTable.AddKeyValue(dotKeysForInlineTable.AsSpan(), TomlValue.Empty, currentNode);
            }
            finally
            {
                dotKeysForInlineTable.Return();
            }

            node.Value = ReadValue();
            SkipWhiteSpace();
            if (TryPeek(out var ch))
            {
                if (TomlCodes.IsComma(ch))
                {
                    Advance(1);
                    SkipWhiteSpace();
                    continue;
                }
                if (TomlCodes.IsRightBraces(ch))
                {
                    Advance(1);
                    break;
                }
                ExceptionHelper.ThrowIncorrectTomlInlineTableFormat();
            }
        }

        return inlineTable;
    }

    internal TomlBoolean ReadBool(bool predictedValue)
    {
        var length = predictedValue ? 4 : 5;
        TomlBoolean value = default!;
        if (sequenceReader.TryFullSpan(length, out var bytes))
        {
            value = TomlBoolean.Parse(bytes);
        }
        else
        {
            var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
            try
            {
                if (sequenceReader.TryGetbytes(length, bufferWriter))
                {
                    value = TomlBoolean.Parse(bufferWriter.WrittenSpan);
                }
                else
                {
                    ExceptionHelper.ThrowEndOfFileReached();
                }
            }
            finally
            {
                RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
            }
        }

        // Verify that the trailing byte does not contain an invalid byte
        if (TryPeek(out var nextByte))
        {
            switch (nextByte)
            {
                case TomlCodes.Symbol.TAB:
                case TomlCodes.Symbol.SPACE:
                case TomlCodes.Symbol.CARRIAGE:
                case TomlCodes.Symbol.LINEFEED:
                case TomlCodes.Symbol.COMMA:
                case TomlCodes.Symbol.RIGHTBRACES:
                case TomlCodes.Symbol.RIGHTSQUAREBRACKET:
                case TomlCodes.Symbol.NUMBERSIGN:
                    break;
                default:
                    ExceptionHelper.ThrowIncorrectTomlBooleanFormat();
                    break;
            }
        }

        return value;
    }

    internal TomlValue ReadNumericValueOrDate()
    {
        // check localtime or localdatetime
        if (sequenceReader.Length >= sequenceReader.Consumed + TomlCodes.DateTime.LocalTimeFormatLength)
        {
            if (TryPeek(2, out var colon) && TomlCodes.IsColon(colon))
            {
                if (ExistNoNewLineAndComment(8))
                {
                    if (sequenceReader.IsFullSpan)
                    {
                        return ReadLocalTime(ReadUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime());
                    }
                    else
                    {
                        var bufferWriter2 = RecycleArrayPoolBufferWriter<byte>.Rent();
                        try
                        {
                            WriteUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime(bufferWriter2);
                            return ReadLocalTime(bufferWriter2.WrittenSpan);
                        }
                        finally
                        {
                            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter2);
                        }
                    }
                }
            }
            else if (TryPeek(4, out var hyphen) && TomlCodes.IsHyphen(hyphen))
            {
                if (ExistNoNewLineAndComment(8))
                {
                    if (sequenceReader.IsFullSpan)
                    {
                        return ReadLocalDateTimeOrOffset(ReadUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime());
                    }
                    else
                    {
                        var bufferWriter2 = RecycleArrayPoolBufferWriter<byte>.Rent();
                        try
                        {
                            WriteUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime(bufferWriter2);
                            return ReadLocalDateTimeOrOffset(bufferWriter2.WrittenSpan);
                        }
                        finally
                        {
                            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter2);
                        }
                    }
                }
            }
        }

        var index = 1;
        while(TryPeek(index++, out var ch))
        {
            switch(ch)
            {
                case TomlCodes.Symbol.TAB:
                case TomlCodes.Symbol.LINEFEED:
                case TomlCodes.Symbol.CARRIAGE:
                case TomlCodes.Symbol.SPACE:
                case TomlCodes.Symbol.COMMA:
                    goto BREAK;
                case TomlCodes.Symbol.DOT:
                case TomlCodes.Alphabet.E:
                case TomlCodes.Alphabet.e:
                    return ReadDouble();
            }
        }
    BREAK:
        // decimal
        return ReadDecimalNumeric();
    }

    internal TomlValue ReadNumericValueOrDateIfLeadingZero()
    {
        // check hexadecimal, octal, or binary
        if (TryPeek(1, out var prefix))
        {
            switch (prefix)
            {
                case TomlCodes.Alphabet.x:
                    return ReadHexNumeric();
                case TomlCodes.Alphabet.o:
                    return ReadOctalNumeric();
                case TomlCodes.Alphabet.b:
                    return ReadBinaryNumeric();
                case TomlCodes.Symbol.DOT: // 0. ...
                case TomlCodes.Alphabet.e: // 0e ...
                case TomlCodes.Alphabet.E: // 0E ...
                    return ReadDouble();
                case TomlCodes.Symbol.TAB:
                case TomlCodes.Symbol.SPACE:
                case TomlCodes.Symbol.LINEFEED:
                case TomlCodes.Symbol.COMMA: // 0,
                case TomlCodes.Symbol.RIGHTSQUAREBRACKET: // 0]
                case TomlCodes.Symbol.RIGHTBRACES: // 0}
                    Advance(1); // 0
                    return TomlInteger.Zero;
                case TomlCodes.Symbol.CARRIAGE:
                    if (TryPeek(2, out var lf) && TomlCodes.IsLf(lf))
                    {
                        Advance(1); // 0
                        return TomlInteger.Zero;
                    }
                    return ExceptionHelper.NotReturnThrow<TomlValue, byte>(ExceptionHelper.ThrowIncorrectCompactEscapeCharacters, lf);
                case var alphabet when TomlCodes.IsAlphabet(alphabet):
                    return ExceptionHelper.NotReturnThrow<TomlValue, byte>(ExceptionHelper.ThrowIncorrectCompactEscapeCharacters, alphabet);
            };
        }
        else
        {
            return TomlInteger.Zero;
        }

        // check localtime or localdatetime
        if (sequenceReader.Length >= sequenceReader.Consumed + TomlCodes.DateTime.LocalTimeFormatLength)
        {
            if (TryPeek(2, out var colon) && TomlCodes.IsColon(colon))
            {
                if (ExistNoNewLineAndComment(8))
                {
                    if (sequenceReader.IsFullSpan)
                    {
                        return ReadLocalTime(ReadUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime());
                    }
                    else
                    {
                        var bufferWriter2 = RecycleArrayPoolBufferWriter<byte>.Rent();
                        try
                        {
                            WriteUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime(bufferWriter2);
                            return ReadLocalTime(bufferWriter2.WrittenSpan);
                        }
                        finally
                        {
                            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter2);
                        }
                    }
                }
            }
            else if (TryPeek(4, out var hyphen) && TomlCodes.IsHyphen(hyphen))
            {
                if (ExistNoNewLineAndComment(8))
                {
                    if (sequenceReader.IsFullSpan)
                    {
                        return ReadLocalDateTimeOrOffset(ReadUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime());
                    }
                    else
                    {
                        var bufferWriter2 = RecycleArrayPoolBufferWriter<byte>.Rent();
                        try
                        {
                            WriteUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime(bufferWriter2);
                            return ReadLocalDateTimeOrOffset(bufferWriter2.WrittenSpan);
                        }
                        finally
                        {
                            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter2);
                        }
                    }
                }
            }
        }

        ExceptionHelper.ThrowIncorrectTomlFormat();
        return default;
    }

    internal TomlValue ReadNumbericValueIfLeadingSign()
    {
        if (TryPeek(1, out var ch))
        {
            if (ch == TomlCodes.Number.Zero)
            {
                if (TryPeek(2, out var ch2))
                {
                    switch(ch2)
                    {
                        case TomlCodes.Symbol.DOT: // 0. ...
                        case TomlCodes.Alphabet.e: // 0e ...
                        case TomlCodes.Alphabet.E: // 0E ...
                            return ReadDouble();
                        case TomlCodes.Symbol.TAB:
                        case TomlCodes.Symbol.SPACE:
                        case TomlCodes.Symbol.LINEFEED:
                        case TomlCodes.Symbol.COMMA: // 0,
                        case TomlCodes.Symbol.RIGHTSQUAREBRACKET: // 0]
                        case TomlCodes.Symbol.RIGHTBRACES: // 0}
                            Advance(2); // 0
                            return TomlInteger.Zero;
                        case TomlCodes.Symbol.CARRIAGE:
                            if (TryPeek(2, out var lf) && TomlCodes.IsLf(lf))
                            {
                                Advance(2); // 0
                                return TomlInteger.Zero;
                            }
                            return ExceptionHelper.NotReturnThrow<TomlValue, byte>(ExceptionHelper.ThrowIncorrectCompactEscapeCharacters, lf);
                        case var alphabet when TomlCodes.IsAlphabet(alphabet):
                            return ExceptionHelper.NotReturnThrow<TomlValue, byte>(ExceptionHelper.ThrowIncorrectCompactEscapeCharacters, alphabet);
                    }
                }
                else
                {
                    return TomlInteger.Zero;
                }
            }
            else if (ch == TomlCodes.Alphabet.i)
            {
                return ReadDoubleInf(true);
            }
            else if (ch == TomlCodes.Alphabet.n)
            {
                return ReadDoubleNan(true);
            }
        }
        else
        {
            ExceptionHelper.ThrowIncorrectTomlIntegerFormat();
        }

        var index = 1;
        while (TryPeek(index++, out var ch3))
        {
            switch (ch3)
            {
                case TomlCodes.Symbol.TAB:
                case TomlCodes.Symbol.LINEFEED:
                case TomlCodes.Symbol.CARRIAGE:
                case TomlCodes.Symbol.SPACE:
                case TomlCodes.Symbol.COMMA:
                    return ReadDecimalNumeric();
                case TomlCodes.Symbol.DOT:
                case TomlCodes.Alphabet.E:
                case TomlCodes.Alphabet.e:
                    return ReadDouble();
            }
        }

        ExceptionHelper.ThrowIncorrectTomlFormat();
        return default!;
    }

    private bool ExistNoNewLineAndComment(int length)
    {
        if (sequenceReader.Length <= sequenceReader.Consumed + length)
            return false;

        var index = 0;
        while(index < length && TryPeek(index++, out var ch))
        {
            switch (ch)
            {
                case TomlCodes.Symbol.LINEFEED:
                case TomlCodes.Symbol.CARRIAGE:
                case TomlCodes.Symbol.NUMBERSIGN:
                    return false;
            }
        }
        return true;
    }

    private TomlInteger ReadDecimalNumeric()
    {
        var writer = new SpanWriter(stackalloc byte[32]);

        var plusOrMinusSign = false;
        if (TryPeek(out var plusOrMinusCh) && TomlCodes.IsPlusOrMinusSign(plusOrMinusCh))
        {
            plusOrMinusSign = true;
            writer.Write(plusOrMinusCh);
            Advance(1);
        }

        if (TryPeek(out var firstCh) && TomlCodes.IsUnderScore(firstCh))
        {
            ExceptionHelper.ThrowUnderscoreUsedConsecutively();
        }

        var underscore = false;
        while (TryPeek(out var ch))
        {
            if (TomlCodes.IsNumber(ch))
            {
                underscore = false;
                writer.Write(ch);
                Advance(1);
                continue;
            }

            switch (ch)
            {
                case TomlCodes.Symbol.TAB:
                case TomlCodes.Symbol.SPACE:
                case TomlCodes.Symbol.CARRIAGE:
                case TomlCodes.Symbol.LINEFEED:
                case TomlCodes.Symbol.COMMA:
                case TomlCodes.Symbol.RIGHTSQUAREBRACKET:
                case TomlCodes.Symbol.RIGHTBRACES:
                case TomlCodes.Symbol.NUMBERSIGN:
                    goto BREAK;
                case TomlCodes.Symbol.UNDERSCORE:
                    // Each underscore is not surrounded by at least one digit on each side.
                    if (underscore) ExceptionHelper.ThrowUnderscoreUsedConsecutively();
                    underscore = true;
                    Advance(1);
                    continue;
                default:
                    return ExceptionHelper.NotReturnThrow<TomlInteger, byte>(ExceptionHelper.ThrowEscapeCharactersIncluded, ch);
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
                if (writingSpan.At(1) == TomlCodes.Number.Zero)
                    ExceptionHelper.ThrowIncorrectTomlIntegerFormat();
            }
        }
        else
        {
            if (writingSpan.Length > 1)
            {
                // 00 or 01
                if (writingSpan.At(0) == TomlCodes.Number.Zero)
                    ExceptionHelper.ThrowIncorrectTomlIntegerFormat();

            }
        }

        return TomlInteger.Parse(writingSpan);
    }

    private TomlInteger ReadHexNumeric()
    {
        Advance(2); // 0x
        if (TryPeek(out var firstCh))
        {
            if (!TomlCodes.IsHex(firstCh))
            {
                if (TomlCodes.IsUnderScore(firstCh))
                    ExceptionHelper.ThrowUnderscoreUsedConsecutively();
                ExceptionHelper.ThrowIncorrectTomlIntegerHexadecimalFormat();
            }
        }
        else
        {
            ExceptionHelper.ThrowIncorrectTomlIntegerHexadecimalFormat();
        }

        var writer = new SpanWriter(stackalloc byte[32]);
        writer.Write(TomlCodes.Number.Zero);
        writer.Write(TomlCodes.Alphabet.x);

        var underscore = false;
        while (TryPeek(out var ch))
        {
            if (TomlCodes.IsNumber(ch) || TomlCodes.IsLowerHexAlphabet(ch) || TomlCodes.IsUpperHexAlphabet(ch))
            {
                underscore = false;
                writer.Write(ch);
                Advance(1);
                continue;
            }

            switch (ch)
            {
                case TomlCodes.Symbol.TAB:
                case TomlCodes.Symbol.SPACE:
                case TomlCodes.Symbol.CARRIAGE:
                case TomlCodes.Symbol.LINEFEED:
                case TomlCodes.Symbol.COMMA:
                case TomlCodes.Symbol.RIGHTSQUAREBRACKET:
                case TomlCodes.Symbol.RIGHTBRACES:
                    goto BREAK;
                case TomlCodes.Symbol.UNDERSCORE:
                    // Each underscore is not surrounded by at least one digit on each side.
                    if (underscore) ExceptionHelper.ThrowUnderscoreUsedConsecutively();
                    underscore = true;
                    Advance(1);
                    continue;
                default:
                    return ExceptionHelper.NotReturnThrow<TomlInteger, byte>(ExceptionHelper.ThrowEscapeCharactersIncluded, ch);
            }

        BREAK:
            break;
        }

        if (underscore)
            ExceptionHelper.ThrowUnderscoreIsUsedAtTheEnd();

        return TomlInteger.Parse(writer.WrittenSpan);
    }

    private TomlInteger ReadOctalNumeric()
    {
        Advance(2); // 0o
        if (TryPeek(out var firstCh))
        {
            if (!TomlCodes.IsOctal(firstCh))
            {
                if (TomlCodes.IsUnderScore(firstCh))
                    ExceptionHelper.ThrowUnderscoreUsedConsecutively();
                ExceptionHelper.ThrowIncorrectTomlIntegerOctalFormat();
            }
        }
        else
        {
            ExceptionHelper.ThrowIncorrectTomlIntegerOctalFormat();
        }

        var writer = new SpanWriter(stackalloc byte[32]);
        writer.Write(TomlCodes.Number.Zero);
        writer.Write(TomlCodes.Alphabet.o);

        var underscore = false;
        while (TryPeek(out var ch))
        {
            if (TomlCodes.IsOctal(ch))
            {
                underscore = false;
                writer.Write(ch);
                Advance(1);
                continue;
            }

            switch (ch)
            {
                case TomlCodes.Symbol.TAB:
                case TomlCodes.Symbol.SPACE:
                case TomlCodes.Symbol.CARRIAGE:
                case TomlCodes.Symbol.LINEFEED:
                case TomlCodes.Symbol.COMMA:
                case TomlCodes.Symbol.RIGHTSQUAREBRACKET:
                case TomlCodes.Symbol.RIGHTBRACES:
                    goto BREAK;
                case TomlCodes.Symbol.UNDERSCORE:
                    // Each underscore is not surrounded by at least one digit on each side.
                    if (underscore) ExceptionHelper.ThrowUnderscoreUsedConsecutively();
                    underscore = true;
                    Advance(1);
                    continue;
                default:
                    return ExceptionHelper.NotReturnThrow<TomlInteger, byte>(ExceptionHelper.ThrowEscapeCharactersIncluded, ch);
            }

        BREAK:
            break;
        }

        if (underscore)
            ExceptionHelper.ThrowUnderscoreIsUsedAtTheEnd();

        return TomlInteger.Parse(writer.WrittenSpan);
    }

    private TomlInteger ReadBinaryNumeric()
    {
        Advance(2); // 0b
        if (TryPeek(out var firstCh))
        {
            if (!TomlCodes.IsBinary(firstCh))
            {
                if (TomlCodes.IsUnderScore(firstCh))
                    ExceptionHelper.ThrowUnderscoreUsedConsecutively();
                ExceptionHelper.ThrowIncorrectTomlIntegerBinaryFormat();
            }
        }
        else
        {
            ExceptionHelper.ThrowIncorrectTomlIntegerBinaryFormat();
        }

        var writer = new SpanWriter(stackalloc byte[64]);
        writer.Write(TomlCodes.Number.Zero);
        writer.Write(TomlCodes.Alphabet.b);

        var underscore = false;
        while (TryPeek(out var ch))
        {
            if (TomlCodes.IsBinary(ch))
            {
                underscore = false;
                writer.Write(ch);
                Advance(1);
                continue;
            }

            switch (ch)
            {
                case TomlCodes.Symbol.TAB:
                case TomlCodes.Symbol.SPACE:
                case TomlCodes.Symbol.CARRIAGE:
                case TomlCodes.Symbol.LINEFEED:
                case TomlCodes.Symbol.COMMA:
                case TomlCodes.Symbol.RIGHTSQUAREBRACKET:
                case TomlCodes.Symbol.RIGHTBRACES:
                    goto BREAK;
                case TomlCodes.Symbol.UNDERSCORE:
                    // Each underscore is not surrounded by at least one digit on each side.
                    if (underscore) ExceptionHelper.ThrowUnderscoreUsedConsecutively();
                    underscore = true;
                    Advance(1);
                    continue;
                default:
                    return ExceptionHelper.NotReturnThrow<TomlInteger, byte>(ExceptionHelper.ThrowEscapeCharactersIncluded, ch);
            }

        BREAK:
            break;
        }

        if (underscore)
            ExceptionHelper.ThrowUnderscoreIsUsedAtTheEnd();

        return TomlInteger.Parse(writer.WrittenSpan);
    }

    private TomlFloat ReadDouble()
    {
        var writer = new SpanWriter(stackalloc byte[512]);
        if (TryPeek(out var plusOrMinusCh) && TomlCodes.IsPlusOrMinusSign(plusOrMinusCh))
        {
            writer.Write(plusOrMinusCh);
            Advance(1);
        }

        if (TryPeek(out var firstNumberCh))
        {
            switch (firstNumberCh)
            {
                case TomlCodes.Symbol.UNDERSCORE:
                    return ExceptionHelper.NotReturnThrow<TomlFloat>(ExceptionHelper.ThrowUnderscoreUsedFirst);
                case TomlCodes.Symbol.DOT:
                    return ExceptionHelper.NotReturnThrow<TomlFloat>(ExceptionHelper.ThrowDotIsUsedFirst);
                case var zero when zero == TomlCodes.Number.Zero:
                    if (TryPeek(1, out var secondNumberCh))
                    {
                        switch(secondNumberCh)
                        {
                            case TomlCodes.Symbol.DOT: // 0.1 ..
                            case TomlCodes.Alphabet.e: // 0e...
                            case TomlCodes.Alphabet.E: // 0E...
                                break;
                            default:
                                ExceptionHelper.ThrowIncorrectTomlFloatFormat();
                                break;
                        }
                    }
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
            if (TomlCodes.IsNumber(ch))
            {
                number = true;
                underline = false;
                writer.Write(ch);
                Advance(1);
                continue;
            }

            switch (ch)
            {
                case TomlCodes.Symbol.UNDERSCORE:
                    if (!number) ExceptionHelper.ThrowUnderscoreUsedWhereNotSurroundedByNumbers();
                    // Each underscore is not surrounded by at least one digit on each side.
                    if (underline) ExceptionHelper.ThrowUnderscoreUsedConsecutively();
                    number = false;
                    underline = true;
                    Advance(1);
                    continue;
                case TomlCodes.Symbol.DOT:
                    if (!number) ExceptionHelper.ThrowDotIsUsedWhereNotSurroundedByNumbers();
                    if (dot) ExceptionHelper.ThrowDotsAreUsedMoreThanOnce();
                    if (exp) ExceptionHelper.ThrowDecimalPointIsPresentAfterTheExponentialPartE();
                    number = false;
                    dot = true;
                    writer.Write(ch);
                    Advance(1);
                    continue;
                case TomlCodes.Alphabet.e:
                case TomlCodes.Alphabet.E:
                    if (!number) ExceptionHelper.ThrowExponentPartUsedWhereNotSurroundedByNumbers();
                    if (exp) ExceptionHelper.ThrowTheExponentPartUsedMoreThanOnce();
                    number = false;
                    sign = false;
                    exp = true;
                    writer.Write(ch);
                    Advance(1);
                    continue;
                case TomlCodes.Symbol.PLUS:
                case TomlCodes.Symbol.MINUS:
                    if (!exp || sign) ExceptionHelper.ThrowIncorrectPositivAndNegativeSigns();
                    number = false;
                    sign = true;
                    writer.Write(ch);
                    Advance(1);
                    continue;
                case TomlCodes.Symbol.TAB:
                case TomlCodes.Symbol.SPACE:
                case TomlCodes.Symbol.CARRIAGE:
                case TomlCodes.Symbol.LINEFEED:
                case TomlCodes.Symbol.COMMA:
                case TomlCodes.Symbol.RIGHTSQUAREBRACKET:
                case TomlCodes.Symbol.RIGHTBRACES:
                case TomlCodes.Symbol.NUMBERSIGN:
                    goto BREAK;
                default:
                    return ExceptionHelper.NotReturnThrow<TomlFloat, byte>(ExceptionHelper.ThrowEscapeCharactersIncluded, ch);
            }

        BREAK:
            break;

        }

        if (underline)
            ExceptionHelper.ThrowUnderscoreIsUsedAtTheEnd();

        var writingSpan = writer.WrittenSpan;
        switch(writingSpan[^1])
        {
            case TomlCodes.Symbol.UNDERSCORE:
                ExceptionHelper.ThrowUnderscoreIsUsedAtTheEnd();
                break;
            case TomlCodes.Symbol.DOT:
                ExceptionHelper.ThrowDotIsUsedAtTheEnd();
                break;
            case TomlCodes.Alphabet.e:
            case TomlCodes.Alphabet.E:
                ExceptionHelper.ThrowExponentPartIsUsedAtTheEnd();
                break;
            case TomlCodes.Symbol.PLUS:
            case TomlCodes.Symbol.MINUS:
                ExceptionHelper.ThrowIncorrectPositivAndNegativeSigns();
                break;
        }

        return TomlFloat.Parse(writingSpan);
    }

    internal TomlFloat ReadDoubleInf(bool prefixedWithPlusOrMinus)
    {
        if (prefixedWithPlusOrMinus)
        {
            if (sequenceReader.Length < sequenceReader.Consumed + 4) ExceptionHelper.ThrowIncorrectTomlFloatFormat();

            if (sequenceReader.TryFullSpan(4, out var span))
            {
                var i = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>(span));
                switch(i)
                {
                    case 1718511915:
                        return TomlFloat.Inf;
                    case 1718511917:
                        return TomlFloat.NInf;
                }
            }
            else
            {
                var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
                try
                {
                    if (sequenceReader.TryGetbytes(4, bufferWriter))
                    {
                        var i = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>(bufferWriter.WrittenSpan));
                        switch (i)
                        {
                            case 1718511915:
                                return TomlFloat.Inf;
                            case 1718511917:
                                return TomlFloat.NInf;
                        }
                    }
                }
                finally
                {
                    RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
                }
            }
        }
        else
        {
            if (sequenceReader.Length < sequenceReader.Consumed + 3) ExceptionHelper.ThrowIncorrectTomlFloatFormat();

            if (sequenceReader.TryFullSpan(3, out var span) && span.SequenceEqual("inf"u8))
            {
                return TomlFloat.Inf;
            }
            else
            {
                var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
                try
                {
                    if (sequenceReader.TryGetbytes(3, bufferWriter) && bufferWriter.WrittenSpan.SequenceEqual("inf"u8))
                    {
                        return TomlFloat.Inf;
                    }
                }
                finally
                {
                    RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
                }
            }
        }

        ExceptionHelper.ThrowIncorrectTomlFloatFormat();
        return default!;
    }

    internal TomlFloat ReadDoubleNan(bool prefixedWithPlusOrMinus)
    {
        if (prefixedWithPlusOrMinus)
        {
            if (sequenceReader.Length < sequenceReader.Consumed + 4) ExceptionHelper.ThrowIncorrectTomlFloatFormat();

            if (sequenceReader.TryFullSpan(4, out var span))
            {
                var i = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>(span));
                switch (i)
                {
                    case 1851878955:
                        return TomlFloat.Nan;
                    case 1851878957:
                        return TomlFloat.PNan;
                }
            }
            else
            {
                var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
                try
                {
                    if (sequenceReader.TryGetbytes(4, bufferWriter))
                    {
                        var i = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>(bufferWriter.WrittenSpan));
                        switch (i)
                        {
                            case 1851878955:
                                return TomlFloat.Nan;
                            case 1851878957:
                                return TomlFloat.PNan;
                        }
                    }
                }
                finally
                {
                    RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
                }
            }
        }
        else
        {
            if (sequenceReader.Length < sequenceReader.Consumed + 3) ExceptionHelper.ThrowIncorrectTomlFloatFormat();

            if (sequenceReader.TryFullSpan(3, out var span) && span.SequenceEqual("nan"u8))
            {
                return TomlFloat.Nan;
            }
            else
            {
                var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
                try
                {
                    if (sequenceReader.TryGetbytes(3, bufferWriter) && bufferWriter.WrittenSpan.SequenceEqual("nan"u8))
                    {
                        return TomlFloat.Nan;
                    }
                }
                finally
                {
                    RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
                }
            }
        }

        ExceptionHelper.ThrowIncorrectTomlFloatFormat();
        return default!;
    }

    private TomlValue ReadLocalDateTimeOrOffset(ReadOnlySpan<byte> bytes)
    {
        // local date
        if (bytes.Length == TomlCodes.DateTime.LocalDateFormatLength)
        {
            return ReadLocalDate(bytes);
        }

        // offset datetime
        if (bytes.Length >= TomlCodes.DateTime.OffsetDateTimeZFormatLength)
        {
            if (bytes[^1] == TomlCodes.Alphabet.Z || bytes[^1] == TomlCodes.Alphabet.z)
            {
                return ReadOffsetDateTime(bytes);
            }
            else if (TomlCodes.IsPlusOrMinusSign(bytes.At(19)))
            {
                return ReadOffsetDateTimeByNumber(bytes);
            }
            else if (TomlCodes.IsDot(bytes.At(19)))
            {
                var index = 20;
                while (index < bytes.Length)
                {
                    ref var c = ref bytes.At(index++);
                    if (!TomlCodes.IsNumber(c))
                    {
                        if (TomlCodes.IsPlusOrMinusSign(c)) break;
                    }
                }
                if (index < bytes.Length)
                {
                    if (!TomlCodes.IsNumber(bytes.At(index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
                    if (!TomlCodes.IsNumber(bytes.At(index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
                    if (!TomlCodes.IsColon( bytes.At(index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
                    if (!TomlCodes.IsNumber(bytes.At(index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
                    if (!TomlCodes.IsNumber(bytes.At(index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
                    return ReadOffsetDateTimeByNumber(bytes);
                }
            }
        }

        // local date time
        return ReadLocalDateTime(bytes);
    }

    private TomlLocalDateTime ReadLocalDateTime(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < TomlCodes.DateTime.LocalDateTimeFormatLength) 
            ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();

        if (!TomlCodes.IsNumber(bytes.At(0))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(1))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(2))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(3))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsHyphen(bytes.At(4))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(5))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(6))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsHyphen(bytes.At(7))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(8))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(9))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!(TomlCodes.IsWhiteSpace(bytes.At(10)) || bytes.At(10) == TomlCodes.Alphabet.T || bytes.At(10) == TomlCodes.Alphabet.t)) 
            ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(11))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(12))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsColon( bytes.At(13))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(14))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(15))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsColon( bytes.At(16))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(17))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(18))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();

        if (bytes.Length > TomlCodes.DateTime.LocalDateTimeFormatLength)
        {
            if (!TomlCodes.IsDot(bytes.At(19))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
            var index = 20;
            while (index < bytes.Length)
            {
                if (!TomlCodes.IsNumber(bytes.At(index++))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
            }
        }

        return TomlLocalDateTime.Parse(bytes);
    }

    private TomlLocalDate ReadLocalDate(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < TomlCodes.DateTime.LocalDateFormatLength) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsNumber(bytes.At(0))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsNumber(bytes.At(1))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsNumber(bytes.At(2))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsNumber(bytes.At(3))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsHyphen(bytes.At(4))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsNumber(bytes.At(5))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsNumber(bytes.At(6))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsHyphen(bytes.At(7))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsNumber(bytes.At(8))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsNumber(bytes.At(9))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();

        return TomlLocalDate.Parse(bytes);
    }

    private TomlLocalTime ReadLocalTime(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < TomlCodes.DateTime.LocalTimeFormatLength) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(0))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(1))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!TomlCodes.IsColon( bytes.At(2))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(3))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(4))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!TomlCodes.IsColon( bytes.At(5))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(6))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(7))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();

        if (bytes.Length > TomlCodes.DateTime.LocalTimeFormatLength)
        {
            if (!TomlCodes.IsDot(bytes.At(8))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
            var index = 9;
            while(index < bytes.Length)
            {
                if (!TomlCodes.IsNumber(bytes.At(index++))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
            }
        }

        return TomlLocalTime.Parse(bytes);
    }

    private TomlOffsetDateTime ReadOffsetDateTime(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < TomlCodes.DateTime.OffsetDateTimeZFormatLength) 
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        if (!(bytes[^1] == TomlCodes.Alphabet.Z || bytes[^1] == TomlCodes.Alphabet.z)) 
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        if (!TomlCodes.IsNumber(bytes.At(0))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(1))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(2))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(3))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsHyphen(bytes.At(4))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(5))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(6))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsHyphen(bytes.At(7))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(8))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(9))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!(TomlCodes.IsWhiteSpace(bytes.At(10)) || bytes.At(10) == TomlCodes.Alphabet.T || bytes.At(10) == TomlCodes.Alphabet.t)) 
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(11))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(12))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsColon( bytes.At(13))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(14))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(15))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsColon( bytes.At(16))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(17))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(18))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();


        return TomlOffsetDateTime.Parse(bytes);
    }

    private TomlOffsetDateTime ReadOffsetDateTimeByNumber(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < TomlCodes.DateTime.OffsetDateTimeZFormatLength) 
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        if (!TomlCodes.IsNumber(bytes.At(0))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(1))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(2))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(3))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsHyphen(bytes.At(4))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(5))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(6))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsHyphen(bytes.At(7))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(8))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(9))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!(TomlCodes.IsWhiteSpace(bytes.At(10)) || bytes.At(10) == TomlCodes.Alphabet.T || bytes.At(10) == TomlCodes.Alphabet.t)) 
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(11))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(12))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsColon( bytes.At(13))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(14))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(15))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsColon( bytes.At(16))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(17))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes.At(18))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        if (TomlCodes.IsHyphen(bytes.At(19)))
        {
            if (!TomlCodes.IsNumber(bytes.At(20))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(bytes.At(21))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsColon(bytes.At(22))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(bytes.At(23))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(bytes.At(24))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        }
        else if (TomlCodes.IsDot(bytes.At(19)))
        {
            var index = 20;
            while (index < bytes.Length)
            {
                ref var c = ref bytes.At(index++);
                if (!TomlCodes.IsNumber(c))
                {
                    if (TomlCodes.IsPlusOrMinusSign(c)) break;
                    ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
                }
            }
            if (!TomlCodes.IsNumber(bytes.At(index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(bytes.At(index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsColon(bytes.At(index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(bytes.At(index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(bytes.At(index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        }

        return TomlOffsetDateTime.Parse(bytes);
    }

    private ReadOnlySpan<byte> ReadUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime()
    {
        var currentSpan = sequenceReader.UnreadSpan;
        var totalLength = 0;
        var delimiterSpace = false;

        for (var index = 0; index < currentSpan.Length; index++)
        {
            var ch = currentSpan[index];
            switch (ch)
            {
                case TomlCodes.Symbol.LINEFEED:
                case TomlCodes.Symbol.COMMA:
                    goto BREAK;
                case TomlCodes.Symbol.TAB:
                case TomlCodes.Symbol.RIGHTSQUAREBRACKET:
                case TomlCodes.Symbol.RIGHTBRACES:
                case TomlCodes.Symbol.NUMBERSIGN:
                    if (delimiterSpace)
                    {
                        totalLength--;
                    }
                    goto BREAK;
                case TomlCodes.Symbol.SPACE:
                    if (index == 10) // space or T
                    {
                        delimiterSpace = true;
                        totalLength++;
                        continue;
                    }
                    goto BREAK;
                case TomlCodes.Symbol.CARRIAGE:
                    if (delimiterSpace)
                    {
                        goto BREAK;
                    }
                    if (index + 1 < currentSpan.Length && TomlCodes.IsLf(currentSpan.At(index + 1)))
                    {
                        goto BREAK;
                    }
                    ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
                    return default;
                default:
                    totalLength++;
                    continue;
            }
        }


    BREAK:
        sequenceReader.TryFullSpan(totalLength, out var bytes);

        return bytes;
    }

    private void WriteUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime(ArrayPoolBufferWriter<byte> bufferWriter)
    {
        var firstPosition = sequenceReader.Consumed;
        var delimiterSpace = false;

        while (TryPeek(out var ch))
        {
            switch (ch)
            {
                case TomlCodes.Symbol.LINEFEED:
                case TomlCodes.Symbol.COMMA:
                    goto BREAK;
                case TomlCodes.Symbol.TAB:
                case TomlCodes.Symbol.RIGHTSQUAREBRACKET:
                case TomlCodes.Symbol.RIGHTBRACES:
                case TomlCodes.Symbol.NUMBERSIGN:
                    if (delimiterSpace)
                    {
                        Rewind(1);
                    }
                    goto BREAK;
                case TomlCodes.Symbol.SPACE:
                    if (sequenceReader.Consumed - firstPosition == 10) // space or T
                    {
                        delimiterSpace = true;
                        Advance(1);
                        continue;
                    }
                    goto BREAK;
                case TomlCodes.Symbol.CARRIAGE:
                    if (delimiterSpace)
                    {
                        goto BREAK;
                    }
                    if (TryPeek(1, out var linebreakCh) && TomlCodes.IsLf(linebreakCh))
                    {
                        goto BREAK;
                    }
                    ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
                    return;
                default:
                    if (delimiterSpace)
                    {
                        bufferWriter.Write(TomlCodes.Symbol.SPACE);
                        bufferWriter.Write(ch);
                        delimiterSpace = false;
                    }
                    else
                    {
                        bufferWriter.Write(ch);
                    }
                    Advance(1);
                    continue;
            }
        BREAK:
            break;
        }
    }
}

