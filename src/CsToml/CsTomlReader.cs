﻿using CsToml.Error;
using CsToml.Extension;
using CsToml.Utility;
using CsToml.Values;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml;

[StructLayout(LayoutKind.Auto)]
internal ref struct CsTomlReader
{
    private Utf8SequenceReader sequenceReader;
    private readonly TomlSpec spec;

    public long LineNumber { get; private set; }

    [DebuggerStepThrough]
    public CsTomlReader(ref Utf8SequenceReader sequenceReader, TomlSpec spec)
    {
        this.sequenceReader = sequenceReader;
        this.spec = spec;
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

            return TomlStringHelper.Parse<TomlUnquotedString>(bufferWriter.WrittenSpan);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadKey(ref ExtendableArray<TomlDottedKey> key)
    {
        SkipWhiteSpace();
        if (!Peek())
            ExceptionHelper.ThrowEndOfFileReached();

        if (spec.AllowUnicodeInBareKeys)
        {
            ReadKeyToAllowUnicodeInBareKeys(ref key);
        }
        else
        {
            ReadKeyToNotAllowUnicodeInBareKeys(ref key);
        }
    }

    private void ReadKeyToAllowUnicodeInBareKeys(ref ExtendableArray<TomlDottedKey> key)
    {
        var dot = true;
        while (TryPeek(out var c))
        {
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
                    if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
                    dot = false;
                    key.Add(ReadDoubleQuoteSingleLineString<TomlBasicDottedKey>());
                    continue;
                case TomlCodes.Symbol.SINGLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
                    dot = false;
                    key.Add(ReadSingleQuoteSingleLineString<TomlLiteralDottedKey>());
                    continue;
                default:
                    if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
                    dot = false;
                    key.Add(ReadUnquotedStringToAllowUnicode(false));
                    continue;
            }

        BREAK:
            break;
        }
    }

    private void ReadKeyToNotAllowUnicodeInBareKeys(ref ExtendableArray<TomlDottedKey> key)
    {
        var dot = true;
        while (TryPeek(out var c))
        {
            if (TomlCodes.IsBareKey(c))
            {
                if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
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
                    if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
                    dot = false;
                    key.Add(ReadDoubleQuoteSingleLineString<TomlBasicDottedKey>());
                    continue;
                case TomlCodes.Symbol.SINGLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
                    dot = false;
                    key.Add(ReadSingleQuoteSingleLineString<TomlLiteralDottedKey>());
                    continue;
                default:
                    ExceptionHelper.ThrowKeyContainsInvalid(c);
                    break;
            }

        BREAK:
            break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadTableHeader(ref ExtendableArray<TomlDottedKey> tableHeaderKey)
    {
        Advance(1); // [

        SkipWhiteSpace();
        if (!Peek())
            ExceptionHelper.ThrowEndOfFileReached();

        if (spec.AllowUnicodeInBareKeys)
        {
            ReadTableHeaderToAllowUnicodeInBareKeys(ref tableHeaderKey);
        }
        else
        {
            ReadTableHeaderToNotAllowUnicodeInBareKeys(ref tableHeaderKey);
        }
    }

    public void ReadTableHeaderToAllowUnicodeInBareKeys(ref ExtendableArray<TomlDottedKey> tableHeaderKey)
    {
        var dot = true;
        var closingRightRightSquareBracket = false;
        while (TryPeek(out var c))
        {
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
                    if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
                    dot = false;
                    tableHeaderKey.Add(ReadDoubleQuoteSingleLineString<TomlBasicDottedKey>());
                    continue;
                case TomlCodes.Symbol.SINGLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
                    dot = false;
                    tableHeaderKey.Add(ReadSingleQuoteSingleLineString<TomlLiteralDottedKey>());
                    continue;
                case TomlCodes.Symbol.RIGHTSQUAREBRACKET:
                    closingRightRightSquareBracket = true;
                    Advance(1);
                    goto BREAK; // ]
                default:
                    if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
                    dot = false;
                    tableHeaderKey.Add(ReadUnquotedStringToAllowUnicode(true));
                    continue;
            }

        BREAK:
            break;
        }

        if (!closingRightRightSquareBracket)
            ExceptionHelper.ThrowTableHeaderIsNotClosedWithClosingBrackets();
    }

    public void ReadTableHeaderToNotAllowUnicodeInBareKeys(ref ExtendableArray<TomlDottedKey> tableHeaderKey)
    {
        var dot = true;
        var closingRightRightSquareBracket = false;
        while (TryPeek(out var c))
        {
            if (TomlCodes.IsBareKey(c))
            {
                if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
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
                    if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
                    dot = false;
                    tableHeaderKey.Add(ReadDoubleQuoteSingleLineString<TomlBasicDottedKey>());
                    continue;
                case TomlCodes.Symbol.SINGLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
                    dot = false;
                    tableHeaderKey.Add(ReadSingleQuoteSingleLineString<TomlLiteralDottedKey>());
                    continue;
                case TomlCodes.Symbol.RIGHTSQUAREBRACKET:
                    closingRightRightSquareBracket = true;
                    Advance(1);
                    goto BREAK; // ]
                default:
                    ExceptionHelper.ThrowTableHeaderContainsInvalid(c);
                    break;
            }

        BREAK:
            break;
        }

        if (!closingRightRightSquareBracket)
            ExceptionHelper.ThrowTableHeaderIsNotClosedWithClosingBrackets();
    }

    public void ReadArrayOfTablesHeader(ref ExtendableArray<TomlDottedKey> arrayOfTablesHeaderKey)
    {
        Advance(2); // [[
        SkipWhiteSpace();

        if (!Peek())
            ExceptionHelper.ThrowEndOfFileReached();

        if (spec.AllowUnicodeInBareKeys)
        {
            ReadArrayOfTablesHeaderToAllowUnicodeInBareKeys(ref arrayOfTablesHeaderKey);
        }
        else
        {
            ReadArrayOfTablesHeaderToNotAllowUnicodeInBareKeys(ref arrayOfTablesHeaderKey);
        }
    }

    public void ReadArrayOfTablesHeaderToAllowUnicodeInBareKeys(ref ExtendableArray<TomlDottedKey> arrayOfTablesHeaderKey)
    {
        var dot = true;
        var closingRightRightSquareBracket = false;
        while (TryPeek(out var c))
        {
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
                    if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
                    dot = false;
                    arrayOfTablesHeaderKey.Add(ReadDoubleQuoteSingleLineString<TomlBasicDottedKey>());
                    continue;
                case TomlCodes.Symbol.SINGLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
                    dot = false;
                    arrayOfTablesHeaderKey.Add(ReadSingleQuoteSingleLineString<TomlLiteralDottedKey>());
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
                    if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
                    dot = false;
                    arrayOfTablesHeaderKey.Add(ReadUnquotedStringToAllowUnicode(true));
                    continue;
            }

        BREAK:
            break;
        }

        if (!closingRightRightSquareBracket)
            ExceptionHelper.ThrowArrayOfTablesHeaderIsNotClosedWithClosingBrackets();

    }

    public void ReadArrayOfTablesHeaderToNotAllowUnicodeInBareKeys(ref ExtendableArray<TomlDottedKey> arrayOfTablesHeaderKey)
    {
        var dot = true;
        var closingRightRightSquareBracket = false;
        while (TryPeek(out var c))
        {
            if (TomlCodes.IsBareKey(c))
            {
                if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
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
                    if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
                    dot = false;
                    arrayOfTablesHeaderKey.Add(ReadDoubleQuoteSingleLineString<TomlBasicDottedKey>());
                    continue;
                case TomlCodes.Symbol.SINGLEQUOTED:
                    if (!dot) ExceptionHelper.ThrowKeysAreNotJoinedByDots();
                    dot = false;
                    arrayOfTablesHeaderKey.Add(ReadSingleQuoteSingleLineString<TomlLiteralDottedKey>());
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
                    ExceptionHelper.ThrowArrayOfTablesHeaderContainsInvalid(c);
                    break;
            }

        BREAK:
            break;
        }

        if (!closingRightRightSquareBracket)
            ExceptionHelper.ThrowArrayOfTablesHeaderIsNotClosedWithClosingBrackets();

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
                return ReadNumericValueIfLeadingSign();
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

        ExceptionHelper.ThrowUnexpectedValueFound();
        return default!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipWhiteSpace()
    {
        if (!TryPeek(out var ch))
            return;

        if (!TomlCodes.IsTabOrWhiteSpace(ch))
            return;

        SkipWhiteSpaceSlow();
    }

    private void SkipWhiteSpaceSlow()
    {
        var currentSpan = sequenceReader.UnreadSpan;
        while (Peek())
        {
            ref var refSpan = ref MemoryMarshal.GetReference(currentSpan);
            for (var index = 0; index < currentSpan.Length; index++)
            {
                if (!TomlCodes.IsTabOrWhiteSpace(Unsafe.Add(ref refSpan, index)))
                {
                    Advance(index);
                    return;
                }
            }
            Advance(currentSpan.Length);
            currentSpan = sequenceReader.CurrentSpan;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadEqual()
    {
        if (!Peek())
            ExceptionHelper.ThrowEndOfFileReachedAfterKey();

        Advance(1); // skip "="
    }

    internal TomlString ReadDoubleQuoteString()
    {
        var doubleQuoteCount = 1;
        var index = 1;
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
                return ReadDoubleQuoteSingleLineString<TomlBasicString>();
            case 3:
            case 4:
            case 5:
                return ReadDoubleQuoteMultiLineString();
            case 6: // first(3) + end(3) = 6 
                Advance(6);
                return TomlMultiLineBasicString.EmptyString;
            case 7: // first(3) + one adjacent mark(1) + end(3) = 7
                Advance(7);
                return new TomlMultiLineBasicString("\"");
            case 8: // first(3) + two adjacent mark(2) + end(3) = 8
                Advance(8);
                return new TomlMultiLineBasicString("\"\"");
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
                ref var refSpan = ref MemoryMarshal.GetReference(currentSpan);
                
                for (var index = 0; index < currentSpan.Length; index++)
                {
                    ref var ch = ref Unsafe.Add(ref refSpan, index);
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

            return T.Parse(bufferWriter.WrittenSpan);
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

            return TomlStringHelper.Parse<TomlMultiLineBasicString>(bufferWriter.WrittenSpan);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }

    }

    private void ParseEscapeSequence(ArrayPoolBufferWriter<byte> bufferWriter, bool multiLine)
    {
        Advance(1); // /

        var result = TomlCodes.TryParseEscapeSequence(
            ref sequenceReader, 
            bufferWriter, 
            multiLine: multiLine, 
            supportsEscapeSequenceE: spec.SupportsEscapeSequenceE,
            supportsEscapeSequenceX: spec.SupportsEscapeSequenceX,
            throwError: true);
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
        var singleQuoteCount = 1;
        var index = 1;
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
                return ReadSingleQuoteSingleLineString<TomlLiteralString>();
            case 3:
            case 4:
            case 5:
                return ReadSingleQuoteMultiLineString();
            case 6: // first(3) + end(3) = 6 
                Advance(6);
                return TomlMultiLineLiteralString.EmptyString;
            case 7: // first(3) + one adjacent mark(1) + end(3) = 7
                Advance(7);
                return new TomlMultiLineLiteralString("'");
            case 8: // first(3) + two adjacent mark(2) + end(3) = 8
                Advance(8);
                return new TomlMultiLineLiteralString("''");
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
            ref var refSpan = ref MemoryMarshal.GetReference(currentSpan);
            for (var index = 0; index < currentSpan.Length; index++)
            {
                ref var ch = ref Unsafe.Add(ref refSpan, index);
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
            return T.Parse(currentSpan[..totalLength]);
        }

        try
        {
            if (Utf8Helper.ContainInvalidSequences(bufferWriter!.WrittenSpan))
                ExceptionHelper.ThrowInvalidCodePoints();
            return T.Parse(bufferWriter!.WrittenSpan);
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
            ref var refSpan = ref MemoryMarshal.GetReference(currentSpan);
            for (var index = 0; index < currentSpan.Length; index++)
            {
                ref var ch = ref Unsafe.Add(ref refSpan, index);
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
            return TomlStringHelper.Parse<TomlMultiLineLiteralString>(currentSpan.Slice(0, totalLength + singleQuotedContinuousCountAtTheEnd - 2));
        }

        try
        {
            var written = bufferWriter!.WrittenSpan;
            var written2 = written.Slice(0, written.Length - 2);
            if (Utf8Helper.ContainInvalidSequences(written2))
                ExceptionHelper.ThrowInvalidCodePoints();

            return TomlStringHelper.Parse<TomlMultiLineLiteralString>(written2);
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
            ref var refSpan = ref MemoryMarshal.GetReference(currentSpan);
            for (var index = 0; index < currentSpan.Length; index++)
            {
                ref var ch = ref Unsafe.Add(ref refSpan, index);
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
                        ExceptionHelper.ThrowBareKeyContainsInvalid(ch);
                        break;
                    default:
                        if (!TomlCodes.IsBareKey(ch))
                            ExceptionHelper.ThrowBareKeyContainsInvalid(ch);
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
            return new TomlUnquotedDottedKey(currentSpan[..totalLength]);
        }
        try
        {
            return new TomlUnquotedDottedKey(bufferWriter!.WrittenSpan);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter!);
        }
    }

    internal TomlDottedKey ReadUnquotedStringToAllowUnicode(bool isTableHeader = false)
    {
        var currentSpan = sequenceReader.UnreadSpan;
        var fullSpan = true;
        var totalLength = 0;
        ArrayPoolBufferWriter<byte>? bufferWriter = default;

        while (this.Peek())
        {
            ref var refSpan = ref MemoryMarshal.GetReference(currentSpan);
            for (var index = 0; index < currentSpan.Length; index++)
            {
                ref var ch = ref Unsafe.Add(ref refSpan, index);
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
                        ExceptionHelper.ThrowBareKeyContainsInvalid(ch);
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
            var keySpan = currentSpan[..totalLength];
            if (Utf8Helper.ContainInvalidSequencesInUnquotedKey(keySpan))
            {
                ExceptionHelper.ThrowInvalidCodePoints();
            }
            return new TomlUnquotedDottedKey(keySpan);
        }
        try
        {
            var keySpan = bufferWriter!.WrittenSpan;
            if (Utf8Helper.ContainInvalidSequencesInUnquotedKey(keySpan))
            {
                ExceptionHelper.ThrowInvalidCodePoints();
            }
            return new TomlUnquotedDottedKey(keySpan);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter!);
        }
    }


    internal TomlArray ReadArray()
    {
        Advance(1); // [

        var initialArray = default(InlineArray16<TomlValue>);
        Span<TomlValue> initialArraySpan = initialArray;
        var arrayBuilder = new InlineArrayBuilder<TomlValue>(initialArraySpan);

        var comma = true;
        var commaCount = 0;
        var closingBracket = false;
        while (TryPeek(out var ch))
        {
            switch (ch)
            {
                case TomlCodes.Symbol.LEFTSQUAREBRACKET:
                    comma = false;
                    arrayBuilder.Add(ReadArray());
                    break;
                case TomlCodes.Symbol.TAB:
                case TomlCodes.Symbol.SPACE:
                    SkipWhiteSpace();
                    break;
                case TomlCodes.Symbol.COMMA:
                    if (comma)
                    {
                        if (commaCount > 0)
                            ExceptionHelper.ThrowCommasAreUsedMoreThanOnce();
                        else
                            ExceptionHelper.ThrowTheCommaIsDefinedFirst();
                    }
                    comma = true;
                    commaCount++;
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
                    arrayBuilder.Add(ReadValue());
                    break;
            }
            continue;
        BREAK:
            break;
        }

        if (!closingBracket)
            ExceptionHelper.ThrowTheArrayIsNotClosedWithClosingBrackets();

        var array = new TomlArray(arrayBuilder.Count);
        if (arrayBuilder.Count > 0)
        {
            var listSpan = array.GetListSpan(arrayBuilder.Count);
            arrayBuilder.CopyToAndReturn(listSpan);
        }

        return array;
    }

    private TomlInlineTable ReadInlineTable()
    {
        Advance(1); // {

        if (spec.AllowNewlinesInInlineTables) // TOML v1.1.0
        {
            SkipWhiteSpaceAndNewLine();
        }
        else
        {
            SkipWhiteSpace();
        }

        if (TryPeek(out var c)) // empty inlinetable
        {
            if (TomlCodes.IsRightBraces(c))
            {
                Advance(1); // }
                return TomlInlineTable.Empty;
            }
        }
        else
        {
            ExceptionHelper.ThrowInlineTableIsNotClosedWithClosingCurlyBrackets();
            return default;
        }

        var inlineTable = new TomlInlineTable();
        TomlTableNode? currentNode = inlineTable.RootNode;
        var dotKeysForInlineTable = new ExtendableArray<TomlDottedKey>(16);
        try
        {
            do
            {
                dotKeysForInlineTable.Clear();
                TomlTableNode node = TomlTableNode.Empty;

                ReadKey(ref dotKeysForInlineTable);
                ReadEqual();
                SkipWhiteSpace();
                // Register only the key, then set the value.
                node = currentNode.AddKeyValue(dotKeysForInlineTable.AsSpan(), TomlValue.Empty);

                node.Value = ReadValue();
                if (spec.AllowNewlinesInInlineTables) // TOML v1.1.0
                {
                    SkipWhiteSpaceAndNewLine();
                }
                else
                {
                    SkipWhiteSpace();
                }
                if (TryPeek(out var ch))
                {
                    if (TomlCodes.IsComma(ch))
                    {
                        Advance(1);
                        if (spec.AllowNewlinesInInlineTables) // TOML v1.1.0
                        {
                            SkipWhiteSpaceAndNewLine();
                        }
                        else
                        {
                            SkipWhiteSpace();
                        }

                        if (TryPeek(out var ch2))
                        {
                            if (TomlCodes.IsRightBraces(ch2))
                            {
                                if (spec.AllowTrailingCommaInInlineTables) // TOML v1.1.0
                                {
                                    Advance(1);
                                    return inlineTable;
                                }
                                ExceptionHelper.ThrowTrailingCommaIsNotAllowed();
                            }
                            continue;
                        }
                    }
                    if (spec.AllowNewlinesInInlineTables) // TOML v1.1.0
                    {
                        SkipWhiteSpaceAndNewLine();
                        if (TryPeek(out var ch2) && TomlCodes.IsRightBraces(ch2))
                        {
                            Advance(1);
                            return inlineTable;
                        }
                    }
                    else
                    {
                        if (TomlCodes.IsRightBraces(ch))
                        {
                            Advance(1);
                            return inlineTable;
                        }
                    }
                    ExceptionHelper.ThrowIncorrectTomlInlineTableFormat();
                }
            } while (Peek());
        }
        finally
        {
            dotKeysForInlineTable.Return();
        }

        ExceptionHelper.ThrowInlineTableIsNotClosedWithClosingCurlyBrackets();
        return default;
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
        // First, a length check is performed.
        var remainingLength = sequenceReader.Remaining;
        if (spec.AllowSecondsOmissionInTime)
        {
            if (remainingLength >= TomlCodes.DateTime.LocalTimeOptionFormatLength
                && TryReadDateTimeOrDateOrTime(out var tomlValue))
            {
                return tomlValue;
            }
        }
        else
        {
            if (remainingLength >= TomlCodes.DateTime.LocalTimeFormatLength
                && TryReadDateTimeOrDateOrTime(out var tomlValue))
            {
                return tomlValue;
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
            }
        }
        else
        {
            return TomlInteger.Zero;
        }

        if (TryReadDateTimeOrDateOrTime(out var tomlValue))
        {
            return tomlValue;
        }

        ExceptionHelper.ThrowIncorrectTomlFormat();
        return default;
    }

    private bool TryReadDateTimeOrDateOrTime([NotNullWhen(true)] out TomlValue? tomlValue)
    {
        // check localtime or localdatetime
        if (TryPeek(2, out var colon) && TomlCodes.IsColon(colon))
        {
            var minimumLength = spec.AllowSecondsOmissionInTime ? TomlCodes.DateTime.LocalTimeOptionFormatLength : TomlCodes.DateTime.LocalTimeFormatLength;

            if (sequenceReader.IsFullSpan)
            {
                if (TryReadUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime(minimumLength, out var span))
                {
                    tomlValue = ReadLocalTime(span);
                    return true;
                }
            }
            else
            {
                if (ExistNoNewLineAndComment(minimumLength))
                {
                    var bufferWriter2 = RecycleArrayPoolBufferWriter<byte>.Rent();
                    try
                    {
                        WriteUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime(bufferWriter2);
                        if (bufferWriter2.WrittenSpan.Length >= minimumLength)
                        {
                            tomlValue = ReadLocalTime(bufferWriter2.WrittenSpan);
                            return true;
                        }
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
            if (sequenceReader.IsFullSpan)
            {
                if (TryReadUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime(TomlCodes.DateTime.LocalDateFormatLength, out var span))
                {
                    // local date
                    if (span.Length == TomlCodes.DateTime.LocalDateFormatLength)
                    {
                        tomlValue = ReadLocalDate(span);
                        return true;
                    }

                    if (span.Length > TomlCodes.DateTime.LocalDateFormatLength)
                    {
                        tomlValue = ReadLocalDateTimeOrOffset(span);
                        return true;
                    }
                }
            }
            else
            {
                if (ExistNoNewLineAndComment(TomlCodes.DateTime.LocalDateFormatLength))
                {
                    var bufferWriter2 = RecycleArrayPoolBufferWriter<byte>.Rent();
                    try
                    {
                        WriteUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime(bufferWriter2);
                        // local date
                        if (bufferWriter2.WrittenSpan.Length == TomlCodes.DateTime.LocalDateFormatLength)
                        {
                            tomlValue = ReadLocalDate(bufferWriter2.WrittenSpan);
                            return true;
                        }

                        if (bufferWriter2.WrittenSpan.Length > TomlCodes.DateTime.LocalDateFormatLength)
                        {
                            tomlValue = ReadLocalDateTimeOrOffset(bufferWriter2.WrittenSpan);
                            return true;
                        }

                    }
                    finally
                    {
                        RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter2);
                    }
                }
            }
        }

        tomlValue = null;
        return false;
    }

    internal TomlValue ReadNumericValueIfLeadingSign()
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
                    Advance(2); // +0 or -0
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

        // If EOF, read as integer.
        return ReadDecimalNumeric();
    }

    private bool ExistNoNewLineAndComment(int length)
    {
        if (sequenceReader.Remaining < length)
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

        var underscore = false;
        while (TryPeek(out var ch))
        {
            if (TomlCodes.IsHex(ch))
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

        return TomlInteger.ParseHex(writer.WrittenSpan);
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

        return TomlInteger.ParseOctal(writer.WrittenSpan);
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

        return TomlInteger.ParseBinary(writer.WrittenSpan);
    }

    private TomlFloat ReadDouble()
    {
        var writer = new SpanWriter(stackalloc byte[64]);
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
                    if (!exp || sign) ExceptionHelper.ThrowIncorrectPositiveAndNegativeSigns();
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
                ExceptionHelper.ThrowIncorrectPositiveAndNegativeSigns();
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
        // offset datetime
        if (bytes.Length >= TomlCodes.DateTime.OffsetDateTimeZFormatLength)
        {
            ref var refBytes = ref MemoryMarshal.GetReference(bytes);
            if (bytes[^1] == TomlCodes.Alphabet.Z || bytes[^1] == TomlCodes.Alphabet.z)
            {
                if (spec.AllowSecondsOmissionInTime && TomlCodes.IsDot(Unsafe.Add(ref refBytes, 16)))
                {
                    return ReadOffsetDateTimeToOmitSeconds(bytes);
                }
                return ReadOffsetDateTime(bytes);
            }
            else if (TomlCodes.IsDot(Unsafe.Add(ref refBytes, 19)))
            {
                var index = 20;
                while (index < bytes.Length)
                {
                    ref var c = ref Unsafe.Add(ref refBytes, index++);
                    if (TomlCodes.IsPlusOrMinusSign(c)) break;
                }
                if (index < bytes.Length)
                {
                    return ReadOffsetDateTimeByNumber(bytes);
                }
            }
            else if (spec.AllowSecondsOmissionInTime && TomlCodes.IsDot(Unsafe.Add(ref refBytes, 16)))
            {
                var index = 17;
                while (index < bytes.Length)
                {
                    ref var c = ref Unsafe.Add(ref refBytes, index++);
                    if (TomlCodes.IsPlusOrMinusSign(c)) break;
                }
                if (index < bytes.Length)
                {
                    return ReadOffsetDateTimeByNumberToOmitSeconds(bytes);
                }
                return ReadLocalDateTimeToOmitSeconds(bytes);
            }
            else if (TomlCodes.IsPlusOrMinusSign(Unsafe.Add(ref refBytes, 19)))
            {
                return ReadOffsetDateTimeByNumber(bytes);
            }
        }

        if (spec.AllowSecondsOmissionInTime && bytes.Length >= TomlCodes.DateTime.OffsetDateTimeZOptionFormatLength)
        {
            ref var refBytes = ref MemoryMarshal.GetReference(bytes);
            if (bytes[^1] == TomlCodes.Alphabet.Z || bytes[^1] == TomlCodes.Alphabet.z)
            {
                return ReadOffsetDateTimeToOmitSeconds(bytes);
            }
            else if (TomlCodes.IsDot(Unsafe.Add(ref refBytes, 16)))
            {
                var index = 17;
                while (index < bytes.Length)
                {
                    ref var c = ref Unsafe.Add(ref refBytes, index++);
                    if (TomlCodes.IsPlusOrMinusSign(c)) break;
                }
                if (index < bytes.Length)
                {
                    return ReadOffsetDateTimeByNumberToOmitSeconds(bytes);
                }
                return ReadLocalDateTimeToOmitSeconds(bytes);
            }
            else if (TomlCodes.IsPlusOrMinusSign(Unsafe.Add(ref refBytes, 16)))
            {
                return ReadOffsetDateTimeByNumberToOmitSeconds(bytes);
            }

        }

        if (bytes.Length < TomlCodes.DateTime.LocalDateTimeFormatLength)
        {
            // If there is no seconds omission option, always fail.
            if (!spec.AllowSecondsOmissionInTime)
            {
                ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
            }
            return ReadLocalDateTimeToOmitSeconds(bytes);
        }

        // local date time
        return ReadLocalDateTime(bytes);
    }


    private TomlLocalDateTime ReadLocalDateTime(ReadOnlySpan<byte> bytes)
    {
        ref var refBytes = ref MemoryMarshal.GetReference(bytes);
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 0))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 1))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 2))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 3))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsHyphen(Unsafe.Add(ref refBytes, 4))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 5))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 6))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsHyphen(Unsafe.Add(ref refBytes, 7))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 8))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 9))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();

        ref var delimiter2 = ref Unsafe.Add(ref refBytes, 10);
        if (!(TomlCodes.IsWhiteSpace(delimiter2) || delimiter2 == TomlCodes.Alphabet.T || delimiter2 == TomlCodes.Alphabet.t))
            ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 11))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 12))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsColon(Unsafe.Add(ref refBytes, 13))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 14))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 15))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsColon(Unsafe.Add(ref refBytes, 16))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 17))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 18))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();

        if (bytes.Length > TomlCodes.DateTime.LocalDateTimeFormatLength)
        {
            if (!TomlCodes.IsDot(Unsafe.Add(ref refBytes, 19))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
            var index = 20;
            while (index < bytes.Length)
            {
                if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, index++))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
            }
        }

        return TomlLocalDateTime.Parse(bytes);
    }

    private TomlLocalDateTime ReadLocalDateTimeToOmitSeconds(ReadOnlySpan<byte> bytes)
    {
        ref var refBytes = ref MemoryMarshal.GetReference(bytes);

        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 0))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 1))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 2))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 3))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsHyphen(Unsafe.Add(ref refBytes, 4))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 5))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 6))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsHyphen(Unsafe.Add(ref refBytes, 7))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 8))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 9))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();

        ref var delimiter = ref Unsafe.Add(ref refBytes, 10);
        if (!(TomlCodes.IsWhiteSpace(delimiter) || delimiter == TomlCodes.Alphabet.T || delimiter == TomlCodes.Alphabet.t))
            ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 11))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 12))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsColon(Unsafe.Add(ref refBytes, 13))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 14))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 15))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();

        if (bytes.Length > TomlCodes.DateTime.LocalDateTimeFormatLength)
        {
            if (!TomlCodes.IsDot(Unsafe.Add(ref refBytes, 16))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
            var index = 17;
            while (index < bytes.Length)
            {
                if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, index++))) ExceptionHelper.ThrowIncorrectTomlLocalDateTimeFormat();
            }
        }

        return TomlLocalDateTime.ParseToOmitSeconds(bytes);
    }

    private TomlLocalDate ReadLocalDate(ReadOnlySpan<byte> bytes)
    {
        ref var refBytes = ref MemoryMarshal.GetReference(bytes);
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,0))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,1))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,2))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,3))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsHyphen(Unsafe.Add(ref refBytes,4))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,5))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,6))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsHyphen(Unsafe.Add(ref refBytes,7))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,8))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,9))) ExceptionHelper.ThrowIncorrectTomlLocalDateFormat();

        return TomlLocalDate.Parse(bytes);
    }

    private TomlLocalTime ReadLocalTime(ReadOnlySpan<byte> bytes)
    {
        ref var refBytes = ref MemoryMarshal.GetReference(bytes);

        if (bytes.Length < TomlCodes.DateTime.LocalTimeFormatLength)
        {
            // allow seconds omission from TOML v1.1.0
            if (!spec.AllowSecondsOmissionInTime)
            {
                ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
            }

            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 0))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 1))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
            if (!TomlCodes.IsColon( Unsafe.Add(ref refBytes, 2))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 3))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 4))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();

            if (bytes.Length > TomlCodes.DateTime.LocalTimeOptionFormatLength)
            {
                if (!TomlCodes.IsDot(Unsafe.Add(ref refBytes, 5))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
                var index = 6;
                while (index < bytes.Length)
                {
                    if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, index++))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
                }
            }

            return TomlLocalTime.ParseToOmitSeconds(bytes);
        }
        
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,0))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,1))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!TomlCodes.IsColon( Unsafe.Add(ref refBytes,2))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,3))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,4))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();

        if (TomlCodes.IsDot(Unsafe.Add(ref refBytes, 5)))
        {            
            // allow seconds omission from TOML v1.1.0
            if (!spec.AllowSecondsOmissionInTime)
            {
                ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
            }
            var index = 6;
            while (index < bytes.Length)
            {
                if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, index++))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
            }
            return TomlLocalTime.ParseToOmitSeconds(bytes);
        }
        else
        {
            if (!TomlCodes.IsColon(Unsafe.Add(ref refBytes, 5))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 6))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 7))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();

            if (bytes.Length > TomlCodes.DateTime.LocalTimeFormatLength)
            {
                if (!TomlCodes.IsDot(Unsafe.Add(ref refBytes, 8))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
                var index = 9;
                while (index < bytes.Length)
                {
                    if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, index++))) ExceptionHelper.ThrowIncorrectTomlLocalTimeFormat();
                }
            }

            return TomlLocalTime.Parse(bytes);
        }
    }

    private TomlOffsetDateTime ReadOffsetDateTime(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < TomlCodes.DateTime.OffsetDateTimeZFormatLength) 
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        if (!(bytes[^1] == TomlCodes.Alphabet.Z || bytes[^1] == TomlCodes.Alphabet.z)) 
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        if (!TomlCodes.IsNumber(bytes[0])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[1])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[2])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[3])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsHyphen(bytes[4])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[5])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[6])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsHyphen(bytes[7])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[8])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[9])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!(  TomlCodes.IsWhiteSpace(bytes[10]) || 
                bytes[10] == TomlCodes.Alphabet.T || 
                bytes[10] == TomlCodes.Alphabet.t))
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[11])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[12])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsColon( bytes[13])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[14])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[15])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsColon( bytes[16])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[17])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[18])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        return TomlOffsetDateTime.Parse(bytes);
    }

    private TomlOffsetDateTime ReadOffsetDateTimeToOmitSeconds(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < TomlCodes.DateTime.OffsetDateTimeZOptionFormatLength)
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        if (!(bytes[^1] == TomlCodes.Alphabet.Z || bytes[^1] == TomlCodes.Alphabet.z))
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        if (!TomlCodes.IsNumber(bytes[0])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[1])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[2])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[3])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsHyphen(bytes[4])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[5])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[6])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsHyphen(bytes[7])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[8])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[9])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!(TomlCodes.IsWhiteSpace(bytes[10]) ||
                bytes[10] == TomlCodes.Alphabet.T ||
                bytes[10] == TomlCodes.Alphabet.t))
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[11])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[12])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsColon(bytes[13])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[14])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(bytes[15])) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        return TomlOffsetDateTime.ParseToOmitSeconds(bytes);
    }

    private TomlOffsetDateTime ReadOffsetDateTimeByNumber(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < TomlCodes.DateTime.OffsetDateTimeZFormatLength) 
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        ref var refBytes = ref MemoryMarshal.GetReference(bytes);
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,0))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,1))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,2))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,3))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsHyphen(Unsafe.Add(ref refBytes,4))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,5))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,6))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsHyphen(Unsafe.Add(ref refBytes,7))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,8))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,9))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        ref var delimiter = ref Unsafe.Add(ref refBytes, 10);
        if (!(TomlCodes.IsWhiteSpace(delimiter) || delimiter == TomlCodes.Alphabet.T || delimiter == TomlCodes.Alphabet.t)) 
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,11))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,12))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsColon( Unsafe.Add(ref refBytes,13))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,14))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,15))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsColon( Unsafe.Add(ref refBytes,16))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,17))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,18))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        if (TomlCodes.IsPlusOrMinusSign(Unsafe.Add(ref refBytes, 19)))
        {
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,20))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,21))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsColon( Unsafe.Add(ref refBytes,22))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,23))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,24))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        }
        else if (TomlCodes.IsDot(Unsafe.Add(ref refBytes, 19)))
        {
            var index = 20;
            while (index < bytes.Length)
            {
                ref var c = ref Unsafe.Add(ref refBytes, index++);
                if (!TomlCodes.IsNumber(c))
                {
                    if (TomlCodes.IsPlusOrMinusSign(c)) break;
                    ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
                }
            }
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsColon( Unsafe.Add(ref refBytes,index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes,index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        }

        return TomlOffsetDateTime.Parse(bytes);
    }

    private TomlOffsetDateTime ReadOffsetDateTimeByNumberToOmitSeconds(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < TomlCodes.DateTime.OffsetDateTimeZOptionFormatLength)
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        ref var refBytes = ref MemoryMarshal.GetReference(bytes);
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 0))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 1))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 2))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 3))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsHyphen(Unsafe.Add(ref refBytes, 4))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 5))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 6))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsHyphen(Unsafe.Add(ref refBytes, 7))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 8))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 9))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        ref var delimiter = ref Unsafe.Add(ref refBytes, 10);
        if (!(TomlCodes.IsWhiteSpace(delimiter) || delimiter == TomlCodes.Alphabet.T || delimiter == TomlCodes.Alphabet.t))
            ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 11))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 12))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsColon(Unsafe.Add(ref refBytes, 13))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 14))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 15))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();

        if (TomlCodes.IsPlusOrMinusSign(Unsafe.Add(ref refBytes, 16)))
        {
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 17))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 18))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsColon(Unsafe.Add(ref refBytes, 19))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 20))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, 21))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        }
        else if (TomlCodes.IsDot(Unsafe.Add(ref refBytes, 16)))
        {
            var index = 17;
            while (index < bytes.Length)
            {
                ref var c = ref Unsafe.Add(ref refBytes, index++);
                if (!TomlCodes.IsNumber(c))
                {
                    if (TomlCodes.IsPlusOrMinusSign(c)) break;
                    ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
                }
            }
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsColon(Unsafe.Add(ref refBytes, index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
            if (!TomlCodes.IsNumber(Unsafe.Add(ref refBytes, index++))) ExceptionHelper.ThrowIncorrectTomlOffsetDateTimeFormat();
        }

        return TomlOffsetDateTime.ParseToOmitSeconds(bytes);
    }

    private bool TryReadUntilWhiteSpaceOrNewLineOrCommaOrEndOfArrayForDateTime(int minimumLength, out ReadOnlySpan<byte> value)
    {
        var currentSpan = sequenceReader.UnreadSpan;
        var totalLength = 0;
        var delimiterSpace = false;

        ref var refSpan = ref MemoryMarshal.GetReference(currentSpan);
        for (var index = 0; index < currentSpan.Length; index++)
        {
            ref var ch = ref Unsafe.Add(ref refSpan, index);
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
                    if (index + 1 < currentSpan.Length && TomlCodes.IsLf(Unsafe.Add(ref refSpan, index + 1)))
                    {
                        goto BREAK;
                    }
                    ExceptionHelper.ThrowEscapeCharactersIncluded(ch);
                    value = [];
                    return false;
                default:
                    totalLength++;
                    continue;
            }
        }


    BREAK:
        if (minimumLength > totalLength)
        {
            value = [];
            return false;
        }
        sequenceReader.TryFullSpan(totalLength, out var bytes);
        value = bytes;
        return true;
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

