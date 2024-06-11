﻿using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;

namespace CsToml;

public partial class CsTomlPackage
{
    internal void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, CsTomlSerializerOptions? options)
        where TBufferWriter : IBufferWriter<byte>
    {
        options ??= CsTomlSerializerOptions.Default;

        try
        {
            table.ToTomlString(ref writer);
        }
        catch (CsTomlException) 
        {
            if (options.IsThrowCsTomlException)
                throw;
        }
    }

    internal void Deserialize(ref Utf8SequenceReader reader, CsTomlSerializerOptions? options)
    {
        options ??= CsTomlSerializerOptions.Default;
        var tomlReader = new CsTomlReader(ref reader);
        CsTomlTableNode? currentNode = table.RootNode;
        try
        {
            var comments = new List<CsTomlString>(4);
            while (tomlReader.Peek())
            {
                // comment
                if (DeserializeComment(ref tomlReader, options, out var comment))
                {
                    comments.Add(comment!);
                }

                if (TrySkipToNewLine(ref tomlReader, options))
                {
                    if (!tomlReader.Peek()) goto BREAK;
                    continue;
                }
                if (!tomlReader.Peek()) goto BREAK;

                // table or table array
                tomlReader.SkipWhiteSpace();
                if (tomlReader.TryPeek(out var leftSquareBracketsCh) && CsTomlSyntax.IsLeftSquareBrackets(leftSquareBracketsCh))
                {
                    tomlReader.Advance(1);
                    if (!tomlReader.TryPeek(out var arrayOfTablesCh))
                    {
                        if (options.IsThrowCsTomlException)
                            throw new CsTomlLineNumberException("The TOML file has been read up to EOF, so analysis of the TOML file has been completed.", tomlReader.LineNumber);

                        exceptions.Add(new CsTomlLineNumberException("The TOML file has been read up to EOF, so analysis of the TOML file has been completed.", tomlReader.LineNumber));
                        goto BREAK;
                    }

                    tomlReader.Rewind(1);
                    if (CsTomlSyntax.IsLeftSquareBrackets(arrayOfTablesCh))
                    {
                        if (DeserializeArrayOfTablesHeader(ref tomlReader, comments, options, out currentNode))
                        {
                            comments.Clear();
                            DeserializeComment(ref tomlReader, options, out var arrayComment);
                            if (!tomlReader.Peek()) goto BREAK;

                            if (!TrySkipToNewLine(ref tomlReader, options))
                            {
                                if (options.IsThrowCsTomlException)
                                    throw new CsTomlLineNumberException("There is a non-newline (or EOF) character after the table array header.", tomlReader.LineNumber);

                                exceptions.Add(new CsTomlLineNumberException("There is a non-newline (or EOF) character after the table array header.", tomlReader.LineNumber));
                                tomlReader.SkipOneLine();
                            }
                        }
                        else
                        {
                            tomlReader.SkipOneLine();
                        }
                        continue;
                    }

                    if (DeserializeTableHeader(ref tomlReader, comments, options, out currentNode))
                    {
                        comments.Clear();
                        DeserializeComment(ref tomlReader, options, out var arrayTableComment);
                        if (!tomlReader.Peek()) goto BREAK;

                        if (!TrySkipToNewLine(ref tomlReader, options))
                        {
                            if (options.IsThrowCsTomlException)
                                throw new CsTomlLineNumberException("There is a character other than a newline (or EOF) after the table header.", tomlReader.LineNumber);

                            exceptions.Add(new CsTomlLineNumberException("There is a character other than a newline (or EOF) after the table header.", tomlReader.LineNumber));
                            tomlReader.SkipOneLine();
                        }
                    }
                    else
                    {
                        tomlReader.SkipOneLine();
                    }
                    continue;
                }

                // key and value
                if (DeserializeKeyValue(ref tomlReader, currentNode!, options, comments))
                {
                    comments.Clear();

                    DeserializeComment(ref tomlReader, options, out var endComment);
                    if (!tomlReader.Peek()) goto BREAK;

                    if (!TrySkipToNewLine(ref tomlReader, options))
                    {
                        if (options.IsThrowCsTomlException)
                            throw new CsTomlLineNumberException("There is a non-newline (or EOF) character after the key/value pair.", tomlReader.LineNumber);

                        exceptions.Add(new CsTomlLineNumberException("There is a non-newline (or EOF) character after the key/value pair.", tomlReader.LineNumber));
                        tomlReader.SkipOneLine();
                    }
                }
                else
                {
                    tomlReader.SkipOneLine();
                }
            }

        BREAK:
            LineNumber = tomlReader.LineNumber;
        }
        finally
        {
            RecycleByteArrayPoolBufferWriter.Release();
        }
    }

    private bool DeserializeComment(ref CsTomlReader reader, CsTomlSerializerOptions options, out CsTomlString? comment)
    {
        comment = null;
        reader.SkipWhiteSpace();
        if (!reader.TryPeek(out var commentCh)) return false;

        if (CsTomlSyntax.IsNumberSign(commentCh))
        {
            try
            {
                comment = reader.ReadComment();
                return true;
            }
            catch (CsTomlException e)
            {
                if (options.IsThrowCsTomlException)
                    throw new CsTomlLineNumberException(e, reader.LineNumber);

                exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
                return false;
            }
            catch (Exception e)
            {
                if (options.IsThrowCsTomlException)
                    throw new CsTomlLineNumberException(e, reader.LineNumber);

                exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
                return false;
            }
        }

        return false;
    }

    private bool DeserializeKeyValue(ref CsTomlReader reader, CsTomlTableNode? currentNode, CsTomlSerializerOptions options, IReadOnlyCollection<CsTomlString> comments)
    {
        try
        {
            var key = reader.ReadKey();
            reader.Advance(1); // skip "="
            reader.SkipWhiteSpace();

            table.AddKeyValue(key, reader.ReadValue(), currentNode, comments);
            return true;
        }
        catch (CsTomlException ce)
        {
            if (options.IsThrowCsTomlException)
                throw new CsTomlLineNumberException(ce, reader.LineNumber);

            exceptions.Add(new CsTomlLineNumberException(ce, reader.LineNumber));
            return false;
        }
        catch (Exception e)
        {
            if (options.IsThrowCsTomlException)
                throw new CsTomlLineNumberException(e, reader.LineNumber);

            exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
    }

    private bool DeserializeTableHeader(ref CsTomlReader reader, IReadOnlyCollection<CsTomlString> comments, CsTomlSerializerOptions options, out CsTomlTableNode? newNode)
    {
        try
        {
            var tableKey = reader.ReadTableHeader();
            table.AddTableHeader(tableKey, comments, out newNode);
            return true;
        }
        catch (CsTomlException e)
        {
            if (options.IsThrowCsTomlException)
                throw new CsTomlLineNumberException(e, reader.LineNumber);

            newNode = null;
            exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
        catch (Exception e)
        {
            if (options.IsThrowCsTomlException)
                throw new CsTomlLineNumberException(e, reader.LineNumber);

            newNode = null;
            exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
    }

    private bool DeserializeArrayOfTablesHeader(ref CsTomlReader reader, IReadOnlyCollection<CsTomlString> comments, CsTomlSerializerOptions options, out CsTomlTableNode? currentNode)
    {
        try
        {
            var tableKey = reader.ReadArrayOfTablesHeader();
            table.AddArrayOfTablesHeader(tableKey, comments, out currentNode);
            return true;
        }
        catch (CsTomlException e)
        {
            if (options.IsThrowCsTomlException)
                throw new CsTomlLineNumberException(e, reader.LineNumber);

            currentNode = null;
            exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
        catch (Exception e)
        {
            if (options.IsThrowCsTomlException)
                throw new CsTomlLineNumberException(e, reader.LineNumber);

            currentNode = null;
            exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
    }

    private bool TrySkipToNewLine(ref CsTomlReader reader, CsTomlSerializerOptions options)
    {
        reader.SkipWhiteSpace();
        if (!reader.TryPeek(out var newline)) return false;

        try
        {
            return reader.TrySkipIfNewLine(newline, true);
        }
        catch (CsTomlException e)
        {
            if (options.IsThrowCsTomlException)
                throw new CsTomlLineNumberException(e, reader.LineNumber);

            exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
    }

}

