﻿using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Diagnostics;

namespace CsToml;

public enum ParserState : byte
{
    ParseStart = 0,
    Comment = 1,
    KeyValue = 2,
    TableHeader = 3,
    ArrayOfTablesHeader = 4,
    ThrowException = 5,
    ParseEnd = 6,
}

public ref struct DottedKeyEnumerator
{
    private ReadOnlySpan<TomlDotKey> keys;
    private int index;

    internal DottedKeyEnumerator(ReadOnlySpan<TomlDotKey> keys)
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

public ref struct CsTomlParser
{
    private CsTomlReader reader;
    private TomlValue? comment;
    private ExtendableArray<TomlDotKey> dottedKeys;
    private TomlValue? value;
    private CsTomlException? exception;

    public readonly long LineNumber => reader.LineNumber;

    public ParserState CurrentState { get; private set; }

    public CsTomlParser(ReadOnlySpan<byte> tomlText)
    {
        reader = new CsTomlReader(tomlText);
        dottedKeys = new ExtendableArray<TomlDotKey>(16);
        CurrentState = ParserState.ParseStart;
    }

    public CsTomlParser(in ReadOnlySequence<byte> tomlText)
    {
        reader = new CsTomlReader(tomlText);
        dottedKeys = new ExtendableArray<TomlDotKey>(16);
        CurrentState = ParserState.ParseStart;
    }

    [DebuggerStepThrough]
    internal CsTomlParser(ref Utf8SequenceReader sequenceReader)
    {
        reader = new CsTomlReader(ref sequenceReader);
        dottedKeys = new ExtendableArray<TomlDotKey>(16);
        CurrentState = ParserState.ParseStart;
    }

    public readonly CsTomlException? GetException()
        => exception;

    public readonly TomlValue? GetComment()
        => comment;

    public DottedKeyEnumerator GetKeys()
        => new(dottedKeys.AsSpan());

    internal readonly ReadOnlySpan<TomlDotKey> GetDottedKeySpan()
        => dottedKeys.AsSpan();

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
                    throw new CsTomlLineNumberException("There is a non-newline (or EOF) character after comment.", LineNumber);
                }
                else if (CurrentState == ParserState.KeyValue ||
                    CurrentState == ParserState.TableHeader ||
                    CurrentState == ParserState.ArrayOfTablesHeader)
                {
                    // end comment
                    if (!TomlCodes.IsNumberSign(ch2))
                    {
                        throw new CsTomlLineNumberException($"There is a non-newline (or EOF) character after {CurrentState}.", LineNumber);
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
            exception = ce;
            // Skip lines where an error is thrown.
            reader.SkipOneLine();
        }

        return true;
    }

    private void ReadComment()
    {
        CurrentState = ParserState.Comment;
        comment = reader.ReadComment();
    }

    private void ReadKeyValue()
    {
        CurrentState = ParserState.KeyValue;
        dottedKeys.Clear();
        reader.ReadKey(ref dottedKeys);
        reader.Advance(1); // skip "="
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
