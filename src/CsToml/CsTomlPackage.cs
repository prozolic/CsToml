using CsToml.Values;
using CsToml.Error;
using CsToml.Utility;
using System.Collections.ObjectModel;

namespace CsToml;

public sealed class CsTomlPackageFactory : ICsTomlPackageFactory
{
    public static CsTomlPackage GetPackage() => new();
}

public partial class CsTomlPackage
{
    private readonly CsTomlTable table;
    private readonly List<CsTomlException> exceptions;

    public long LineNumber { get; internal set; }

    public ReadOnlyCollection<CsTomlException>? Exceptions => exceptions.AsReadOnly();

    public bool IsThrowCsTomlException { get; set; }

    public CsTomlPackage()
    {
        table = new();
        exceptions = new();
        LineNumber = 0;
        IsThrowCsTomlException = true;
    }

    internal void Serialize(ref Utf8Writer writer)
        => table.ToTomlString(ref writer);

    internal void Deserialize(ref Utf8Reader utf8Reader)
    {
        var reader = new CsTomlReader(ref utf8Reader);
        CsTomlTableNode? currentNode = table.RootNode;

        var comments = new List<CsTomlString>();
        while (reader.Peek())
        {
            // comment
            DeserializeComment(ref reader, out var comment);
            if (comment != null) comments.Add(comment);
            if (!reader.Peek()) goto BREAK;

            if (DeserializeNewLine(ref reader))
            {
                if (!reader.Peek()) goto BREAK;
                continue;
            }
            if (!reader.Peek()) goto BREAK;

            // table or table array
            reader.SkipWhiteSpace();
            if (reader.TryPeek(out var leftSquareBracketsCh) && CsTomlSyntax.IsLeftSquareBrackets(leftSquareBracketsCh))
            {
                reader.Skip(1);
                if (!reader.TryPeek(out var tableArrayCh))
                {
                    if (IsThrowCsTomlException)
                        throw new CsTomlLineNumberException("The TOML file has been read up to EOF, so analysis of the TOML file has been completed.", reader.LineNumber);

                    exceptions.Add(new CsTomlLineNumberException("The TOML file has been read up to EOF, so analysis of the TOML file has been completed.", reader.LineNumber));
                    goto BREAK;
                }

                reader.Skip(-1);
                if (CsTomlSyntax.IsLeftSquareBrackets(tableArrayCh))
                {
                    if (DeserializeTableArray(ref reader, out currentNode, comments))
                    {
                        comments.Clear();
                        DeserializeComment(ref reader, out var arrayComment);
                        if (!reader.Peek()) goto BREAK;

                        DeserializeNewLine(ref reader);
                        continue;
                    }

                    reader.SkipOneLine();
                    continue;
                }

                if (DeserializeTableSection(ref reader, out currentNode, comments))
                {
                    comments.Clear();
                    DeserializeComment(ref reader, out var arrayTableComment);
                    if (!reader.Peek()) goto BREAK;

                    DeserializeNewLine(ref reader);
                    continue;
                }
                else
                {
                    reader.SkipOneLine();
                    continue;
                }
            }

            // key and value
            if (DeserializeKeyValue(ref reader, currentNode!, comments))
            {
                comments.Clear();
            }
            else
            {
                reader.SkipOneLine();
                continue;
            }

            DeserializeComment(ref reader, out var endComment);
            if (!reader.Peek()) goto BREAK;

            DeserializeNewLine(ref reader);
        }

    BREAK:
        LineNumber = reader.LineNumber;
        RecycleByteArrayPoolBufferWriter.Release();
    }

    private bool DeserializeComment(ref CsTomlReader reader, out CsTomlString? comment)
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
                if (IsThrowCsTomlException)
                    throw new CsTomlLineNumberException(e, reader.LineNumber);

                exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        return false;
    }

    private bool DeserializeNewLine(ref CsTomlReader reader)
    {
        reader.SkipWhiteSpace();
        if (!reader.TryPeek(out var newline)) return false;

        try
        {
            return reader.TrySkipToNewLine(newline, true);
        }
        catch (CsTomlException e)
        {
            if (IsThrowCsTomlException)
                throw new CsTomlLineNumberException(e, reader.LineNumber);

            exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
    }

    private bool DeserializeKeyValue(ref CsTomlReader reader, CsTomlTableNode? currentNode, IReadOnlyCollection<CsTomlString> comments)
    {
        try
        {
            var key = reader.ReadKey();

            reader.SkipWhiteSpace();
            if (!reader.TryPeek(out var equalCh)) ExceptionHelper.ThrowEndOfFileReached(); // = or value is nothing
            if (!CsTomlSyntax.IsEqual(equalCh)) ExceptionHelper.ThrowNoEqualAfterTheKey(); // = is nothing

            reader.Skip(1); // skip "="
            reader.SkipWhiteSpace();

            if (!reader.Peek()) ExceptionHelper.ThrowEndOfFileReached(); // value is nothing
            var value = reader.ReadValue();

            table.AddKeyValue(key, value, currentNode, comments);
            return true;
        }
        catch (CsTomlException e)
        {
            if (IsThrowCsTomlException)
                throw new CsTomlLineNumberException(e, reader.LineNumber);

            exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private bool DeserializeTableSection(ref CsTomlReader reader, out CsTomlTableNode? newNode, IReadOnlyCollection<CsTomlString> comments)
    {
        try
        {
            var tableKey = reader.ReadKey();
            table.AddTableHeader(tableKey, out newNode, comments);
            return true;
        }
        catch (CsTomlException e)
        {
            if (IsThrowCsTomlException)
                throw new CsTomlLineNumberException(e, reader.LineNumber);

            newNode = null;
            exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
        catch (Exception)
        {
            newNode = null;
            throw;
        }
    }

    private bool DeserializeTableArray(ref CsTomlReader reader, out CsTomlTableNode? currentNode, IReadOnlyCollection<CsTomlString> comments)
    {
        try
        {
            var tableKey = reader.ReadKey();
            table.AddTableArrayHeader(tableKey, out currentNode, comments);
            return true;
        }
        catch (CsTomlException e)
        {
            if (IsThrowCsTomlException)
                throw new CsTomlLineNumberException(e, reader.LineNumber);

            currentNode = null;
            exceptions.Add(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
        catch (Exception)
        {
            currentNode = null;
            throw;
        }
    }

}

