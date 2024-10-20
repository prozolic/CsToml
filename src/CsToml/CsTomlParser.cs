using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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

internal ref struct DottedKeyEnumerator
{
    private ReadOnlySpan<TomlDottedKey> keys;
    private int index;

    internal DottedKeyEnumerator(ReadOnlySpan<TomlDottedKey> keys)
    {
        this.keys = keys;
        index = 0;
    }

    public string Current { get; private set; } = string.Empty;

    public readonly DottedKeyEnumerator GetEnumerator() => this;

    public bool MoveNext()
    {
        var target = keys;
        if (target.Length == 0 || target.Length == index) return false;

        Current = target[index++].Utf16String;
        return true;
    }

}

internal ref struct CsTomlParser
{
    private CsTomlReader reader;
    private TomlValue? comment;
    private ExtendableArray<TomlDottedKey> dottedKeys;
    private TomlValue? value;
    private CsTomlLineNumberException? exception;

    public readonly long LineNumber => reader.LineNumber;

    public ParserState CurrentState { get; private set; }

    [DebuggerStepThrough]
    internal CsTomlParser(ref Utf8SequenceReader sequenceReader)
    {
        reader = new CsTomlReader(ref sequenceReader);
        dottedKeys = new ExtendableArray<TomlDottedKey>(16);
        CurrentState = ParserState.ParseStart;
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Return()
        => dottedKeys.Return();

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly CsTomlLineNumberException? GetException()
        => exception;

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly TomlValue? GetComment()
        => comment;

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly ReadOnlySpan<TomlDottedKey> GetDottedKeySpan()
        => dottedKeys.AsSpan();

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly TomlValue? GetValue()
        => value;

    public bool TryRead()
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
                        ReadComment();
                        break;
                    case TomlCodes.Symbol.LEFTSQUAREBRACKET: // table or array of tables
                        if (reader.TryPeek(1, out var c) && TomlCodes.IsLeftSquareBrackets(c))
                            ReadArrayOfTablesHeader();
                        else
                            ReadTableHeader();
                        break;
                    default:
                        ReadKeyValue();
                        break;
                }
            }
            else
            {
                CurrentState = ParserState.ParseEnd;
                return false;
            }

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
                    if (!TomlCodes.IsNumberSign(ch2))
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
            exception = new CsTomlLineNumberException(ce, LineNumber);
            // Skip lines where an error is thrown.
            reader.SkipOneLine();
        }


        return true;
    }

    private void ReadComment()
    {
        CurrentState = CurrentState != ParserState.KeyValue ? ParserState.Comment : ParserState.EndComment;
        comment = reader.ReadComment();
    }

    private void ReadKeyValue()
    {
        CurrentState = ParserState.KeyValue;
        dottedKeys.Clear();
        reader.ReadKey(ref dottedKeys);
        reader.ReadEqual();
        reader.SkipWhiteSpace();
        value = reader.ReadValue();
    }

    private void ReadTableHeader()
    {
        CurrentState = ParserState.TableHeader;
        dottedKeys.Clear();
        reader.ReadTableHeader(ref dottedKeys);
        value = default;
    }

    private void ReadArrayOfTablesHeader()
    {
        CurrentState = ParserState.ArrayOfTablesHeader;
        dottedKeys.Clear();
        reader.ReadArrayOfTablesHeader(ref dottedKeys);
        value = default;
    }
}

