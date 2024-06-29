using CsToml.Error;
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
        catch (CsTomlException cte)
        {
            throw new CsTomlSerializeException([cte]);
        }
    }

    internal void Deserialize(ref Utf8SequenceReader reader, CsTomlSerializerOptions? options)
    {
        options ??= CsTomlSerializerOptions.Default;
        var tomlReader = new CsTomlReader(ref reader);
        CsTomlTableNode? currentNode = table.RootNode;
        try
        {
            List<CsTomlException> exceptions = [];
            var comments = new List<CsTomlString>();

            while (tomlReader.Peek())
            {
                // comment
                if (DeserializeComment(ref tomlReader, options, exceptions, out var comment))
                {
                    comments.Add(comment!);
                }

                if (TrySkipToNewLine(ref tomlReader, options, exceptions))
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
                        exceptions.Add(new CsTomlLineNumberException("The TOML file has been read up to EOF, so analysis of the TOML file has been completed.", tomlReader.LineNumber));
                        goto BREAK;
                    }

                    tomlReader.Rewind(1);
                    if (CsTomlSyntax.IsLeftSquareBrackets(arrayOfTablesCh))
                    {
                        if (DeserializeArrayOfTablesHeader(ref tomlReader, comments, options, exceptions, out currentNode))
                        {
                            comments.Clear();
                            DeserializeComment(ref tomlReader, options, exceptions, out var arrayComment);
                            if (!tomlReader.Peek()) goto BREAK;

                            if (!TrySkipToNewLine(ref tomlReader, options, exceptions))
                            {
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

                    if (DeserializeTableHeader(ref tomlReader, comments, options, exceptions, out currentNode))
                    {
                        comments.Clear();
                        DeserializeComment(ref tomlReader, options, exceptions, out var arrayTableComment);
                        if (!tomlReader.Peek()) goto BREAK;

                        if (!TrySkipToNewLine(ref tomlReader, options, exceptions))
                        {
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
                if (DeserializeKeyValue(ref tomlReader, currentNode!, options, exceptions, comments))
                {
                    comments.Clear();

                    DeserializeComment(ref tomlReader, options, exceptions, out var endComment);
                    if (!tomlReader.Peek()) goto BREAK;

                    if (!TrySkipToNewLine(ref tomlReader, options, exceptions))
                    {
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
            if (exceptions.Count > 0)
                throw new CsTomlSerializeException(exceptions);
        }
        finally
        {
            CsTomlReader.Release();
        }
    }

    private bool DeserializeComment(ref CsTomlReader reader, CsTomlSerializerOptions options, List<CsTomlException> exceptions, out CsTomlString? comment)
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
                exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
                return false;
            }
            catch (Exception e)
            {
                exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
                return false;
            }
        }

        return false;
    }

    private bool DeserializeKeyValue(ref CsTomlReader reader, CsTomlTableNode? currentNode, CsTomlSerializerOptions options, List<CsTomlException> exceptions, IReadOnlyCollection<CsTomlString> comments)
    {
        try
        {
            var key = reader.ReadKey();
            try
            {
                reader.Advance(1); // skip "="
                reader.SkipWhiteSpace();
                table.AddKeyValue(key, reader.ReadValue(), currentNode, comments);
            }
            finally
            {
                key.Recycle();
            }
            return true;
        }
        catch (CsTomlException ce)
        {
            exceptions.Add(new CsTomlLineNumberException(ce, reader.LineNumber));
            return false;
        }
        catch (Exception e)
        {
            exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
    }

    private bool DeserializeTableHeader(ref CsTomlReader reader, IReadOnlyCollection<CsTomlString> comments, CsTomlSerializerOptions options, List<CsTomlException> exceptions, out CsTomlTableNode? newNode)
    {
        try
        {
            var tableKey = reader.ReadTableHeader();
            try
            {
                table.AddTableHeader(tableKey, comments, out newNode);
            }
            finally
            {
                tableKey.Recycle();
            }
            return true;
        }
        catch (CsTomlException e)
        {
            newNode = null;
            exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
        catch (Exception e)
        {
            newNode = null;
            exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
    }

    private bool DeserializeArrayOfTablesHeader(ref CsTomlReader reader, IReadOnlyCollection<CsTomlString> comments, CsTomlSerializerOptions options, List<CsTomlException> exceptions, out CsTomlTableNode? currentNode)
    {
        try
        {
            var tableKey = reader.ReadArrayOfTablesHeader();
            try
            {
                table.AddArrayOfTablesHeader(tableKey, comments, out currentNode);
            }
            finally
            {
                tableKey.Recycle();
            }
            return true;
        }
        catch (CsTomlException e)
        {
            currentNode = null;
            exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
        catch (Exception e)
        {
            currentNode = null;
            exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
    }

    private bool TrySkipToNewLine(ref CsTomlReader reader, CsTomlSerializerOptions options, List<CsTomlException> exceptions)
    {
        reader.SkipWhiteSpace();
        if (!reader.TryPeek(out var newline)) return false;

        try
        {
            return reader.TrySkipIfNewLine(newline, true);
        }
        catch (CsTomlException e)
        {
            exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
    }

}

