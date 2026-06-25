using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml;

internal enum ParserState : byte
{
    ParseStart = 0,
    Comment = 1,
    KeyValue = 2,
    TableHeader = 3,
    ArrayOfTablesHeader = 4,
    ThrowException = 5,
    ParseEnd = 6,
    EndComment = 7
}

[StructLayout(LayoutKind.Auto)]
internal ref struct CsTomlParser
{
    internal CsTomlReader reader;
    private CsTomlParseException? exception;
    private bool endComment;

    public long LineNumber => reader.LineNumber;

    public ParserState CurrentState
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private set;
    }

    public bool IsEndComment => endComment;

    [DebuggerStepThrough]
    internal CsTomlParser(ref Utf8SequenceReader reader, CsTomlSerializerOptions options)
    {
        this.reader = new CsTomlReader(ref reader, options.Spec);
        CurrentState = ParserState.ParseStart;
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly CsTomlParseException? GetException()
        => exception;

    public bool Read()
    {
        if (!reader.Peek())
        {
            CurrentState = ParserState.ParseEnd;
            return false;
        }

        // Skip spaces and newlines
        reader.SkipWhiteSpaceAndNewLine();
        try
        {
            // Reads building block of a TOML document.
            // Determine and read the components from the first byte.
            if (reader.TryPeek(out var ch))
            {
                switch (ch)
                {
                    case TomlCodes.Symbol.NUMBERSIGN:
                        CurrentState = ParserState.Comment;
                        return true;
                    case TomlCodes.Symbol.LEFTSQUAREBRACKET: // table or array of tables
                        if (reader.TryPeek(1, out var c) && TomlCodes.IsLeftSquareBrackets(c))
                        {
                            reader.Advance(2);
                            CurrentState = ParserState.ArrayOfTablesHeader;
                            return true;
                        }
                        else
                        {
                            reader.Advance(1);
                            CurrentState = ParserState.TableHeader;
                            return true;
                        }
                    default:
                        {
                            CurrentState = ParserState.KeyValue;
                            return true;
                        }
                }
            }
            else
            {
                CurrentState = ParserState.ParseEnd;
                return false;
            }
        }
        catch (CsTomlException ce)
        {
            CurrentState = ParserState.ThrowException;
            exception = new CsTomlParseException(ce, LineNumber);
            // Skip lines where an error is thrown.
            reader.SkipOneLine();
        }

        return false;
    }

    public bool ReadEnd()
    {
        try
        {
            reader.SkipWhiteSpace();
            if (reader.TryPeek(out var ch2))
            {
                // skip newline
                if (reader.TrySkipIfNewLine(ch2, true))
                    return true;

                if (CurrentState == ParserState.Comment)
                {
                    ExceptionHelper.ThrowException("There is a non-newline (or EOF) character after comment.");
                }
                else if (CurrentState == ParserState.KeyValue ||
                    CurrentState == ParserState.TableHeader ||
                    CurrentState == ParserState.ArrayOfTablesHeader)
                {
                    // end comment
                    if (TomlCodes.IsNumberSign(ch2))
                    {
                        endComment = true;
                    }
                    else
                    {
                        ExceptionHelper.ThrowException($"There is a non-newline (or EOF) character after {CurrentState}.");
                    }
                }
            }
            else
            {
                if (CurrentState != ParserState.ParseEnd)
                    return true;

                CurrentState = ParserState.ParseEnd;
                return false;
            }
        }
        catch (CsTomlException ce)
        {
            CurrentState = ParserState.ThrowException;
            exception = new CsTomlParseException(ce, LineNumber);
            // Skip lines where an error is thrown.
            reader.SkipOneLine();
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TomlString ReadComment()
    {
        CurrentState = endComment ? ParserState.EndComment : ParserState.Comment;
        var comment = reader.ReadComment();
        endComment = false;
        return comment;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ReadEndComment()
    {
        CurrentState = endComment ? ParserState.EndComment : ParserState.Comment;
        reader.ReadEndComment();
        endComment = false;
    }

}
