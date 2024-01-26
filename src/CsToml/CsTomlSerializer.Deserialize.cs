using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;

namespace CsToml;

public partial class CsTomlSerializer
{
    public static void ReadAndDeserialize(ref CsTomlPackage package, string? path)
    {
        if (Path.GetExtension(path) != ".toml") throw new FormatException($"TOML files should use the extension .toml");
        if (!File.Exists(path)) throw new FileNotFoundException(nameof(path));

        using var handle = File.OpenHandle(path, FileMode.Open, FileAccess.Read, options: FileOptions.Asynchronous);
        var length = RandomAccess.GetLength(handle);
        var bytes = new byte[length];
        RandomAccess.Read(handle, bytes.AsSpan(), 0);

        var startIndex = Utf8Helper.ContainBOM(bytes) ? 3 : 0;
        var byteSpan = bytes.AsSpan(startIndex);
        Deserialize(ref package, byteSpan);
    }

    public static async ValueTask<CsTomlPackage> ReadAndDeserializeAsync<TFactory>(string? path, CancellationToken cancellationToken = default)
        where TFactory : ICsTomlPackageFactory
    {
        if (!File.Exists(path)) throw new FileNotFoundException(nameof(path));

        using var handle = File.OpenHandle(path, FileMode.Open, FileAccess.Read, options: FileOptions.Asynchronous);
        var length = RandomAccess.GetLength(handle);
        var bytes = new byte[length];
        await RandomAccess.ReadAsync(handle, bytes.AsMemory(), 0, cancellationToken).ConfigureAwait(false);

        var startIndex = Utf8Helper.ContainBOM(bytes) ? 3 : 0;
        var package = TFactory.GetPackage();
        Deserialize(ref package, bytes.AsSpan(startIndex));
        return package;
    }

    public static void Deserialize(ref CsTomlPackage package, ReadOnlySpan<byte> tomlUtf8Text)
    {
        var reader = new CsTomlReader(tomlUtf8Text);
        CsTomlTableNode? currentNode = package.Node;

        var comments = new List<CsTomlString>();
        while (reader.Peek())
        {
            // comment
            DeserializeComment(ref package, ref reader, out var comment);
            if (comment != null) comments.Add(comment);
            if (!reader.Peek()) goto BREAK;

            if (DeserializeNewLine(ref package, ref reader))
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
                    if (package.IsThrowCsTomlException)
                        throw new CsTomlLineNumberException("The TOML file has been read up to EOF, so analysis of the TOML file has been completed.", reader.LineNumber);

                    package.AddException(new CsTomlLineNumberException("The TOML file has been read up to EOF, so analysis of the TOML file has been completed.", reader.LineNumber));
                    goto BREAK;
                }

                reader.Skip(-1);
                if (CsTomlSyntax.IsLeftSquareBrackets(tableArrayCh))
                {
                    if (DeserializeTableArray(ref package, ref reader, out currentNode, comments))
                    {
                        comments.Clear();
                        DeserializeComment(ref package, ref reader, out var arrayComment);
                        if (!reader.Peek()) goto BREAK;

                        DeserializeNewLine(ref package, ref reader);
                        continue;
                    }

                    reader.SkipOneLine();
                    continue;
                }

                if (DeserializeTableSection(ref package, ref reader, out currentNode, comments))
                {
                    comments.Clear();
                    DeserializeComment(ref package, ref reader, out var arrayTableComment);
                    if (!reader.Peek()) goto BREAK;

                    DeserializeNewLine(ref package, ref reader);
                    continue;
                }
                else
                {
                    reader.SkipOneLine();
                    continue;
                }
            }

            // key and value
            if (DeserializeKeyValue(ref package, ref reader, currentNode!, comments))
            {
                comments.Clear();
            }
            else
            {
                reader.SkipOneLine();
                continue;
            }

            DeserializeComment(ref package, ref reader, out var endComment);
            if (!reader.Peek()) goto BREAK;

            DeserializeNewLine(ref package, ref reader);
        }

    BREAK:
        package.LineNumber = reader.LineNumber;
        RecycleByteArrayPoolBufferWriter.Release();
    }

    private static bool DeserializeComment(ref CsTomlPackage package, ref CsTomlReader reader, out CsTomlString? comment)
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
            catch(CsTomlException e)
            {
                if (package.IsThrowCsTomlException)
                    throw new CsTomlLineNumberException(e, reader.LineNumber);

                package.AddException(new CsTomlLineNumberException(e, reader.LineNumber));
                return false;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        return false;
    }

    private static bool DeserializeNewLine(ref CsTomlPackage package, ref CsTomlReader reader)
    {
        reader.SkipWhiteSpace();
        if (!reader.TryPeek(out var newline)) return false;
        
        try
        {
            if (reader.TrySkipToNewLine(newline, true))
            {
                return true;
            }
        }
        catch(CsTomlException e)
        {
            if (package.IsThrowCsTomlException)
                throw new CsTomlLineNumberException(e, reader.LineNumber);

            package.AddException(new CsTomlLineNumberException(e, reader.LineNumber));
        }
        return false;

    }

    private static bool DeserializeKeyValue(ref CsTomlPackage package, ref CsTomlReader reader, CsTomlTableNode? currentNode, IEnumerable<CsTomlString>? comments)
    {
        try
        {
            reader.SkipWhiteSpace();
            var key = reader.ReadKey();

            reader.SkipWhiteSpace();
            if (!reader.TryPeek(out var equalCh)) return false; // = or value is nothing
            if (!CsTomlSyntax.IsEqual(equalCh)) return false; // = is nothing

            reader.Skip(1); // skip "="
            reader.SkipWhiteSpace();

            if (!reader.Peek()) return false; // value is nothing
            var value = reader.ReadValue();

            if (package.TryAddKeyValue(key, value, currentNode, comments))
            {
                // add key and value
                return true;
            }
            return false;
        }
        catch(CsTomlException e)
        {
            if (package.IsThrowCsTomlException)
                throw new CsTomlLineNumberException(e, reader.LineNumber);

            package.AddException(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
        catch(Exception e)
        {
            throw;
        }
    }

    private static bool DeserializeTableSection(ref CsTomlPackage package, ref CsTomlReader reader, out CsTomlTableNode? currentNode, IEnumerable<CsTomlString>? comments)
    {
        try
        {
            var tableKey = reader.ReadKey();

            return package.TryAddTableHeader(tableKey, out currentNode, comments);
        }
        catch (CsTomlException e)
        {
            if (package.IsThrowCsTomlException)
                throw new CsTomlLineNumberException(e, reader.LineNumber);

            currentNode = null;
            package.AddException(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
        catch (Exception e)
        {
            currentNode = null;
            throw;
        }
    }

    private static bool DeserializeTableArray(ref CsTomlPackage package, ref CsTomlReader reader, out CsTomlTableNode? currentNode, IEnumerable<CsTomlString>? comments)
    {
        try
        {
            var tableKey = reader.ReadKey();

            if (!package.TryAddTableArrayHeader(tableKey, out currentNode, comments))
            {
                return false;
            }
            return true;
        }
        catch (CsTomlException e)
        {
            if (package.IsThrowCsTomlException)
                throw new CsTomlLineNumberException(e, reader.LineNumber);

            currentNode = null;
            package.AddException(new CsTomlLineNumberException(e, reader.LineNumber));
            return false;
        }
        catch (Exception e)
        {
            currentNode = null;
            throw;
        }
    }

}


