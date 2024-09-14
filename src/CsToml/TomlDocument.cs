using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Diagnostics;

namespace CsToml;

[DebuggerDisplay("Toml Document = {table.RootNode.NodeCount}")]
public partial class TomlDocument : ITomlSerializedObject<TomlDocument>
{
    #region ITomlSerializedObject

    static void ITomlSerializedObject<TomlDocument>.Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TomlDocument? target, CsTomlSerializerOptions options)
    {
        // No registration required
    }

    static TomlDocument ITomlSerializedObject<TomlDocument>.Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        // No registration required
        return default!;
    }

    static void ITomlSerializedObjectRegister.Register()
    {
        // No registration required
    }

    #endregion

    private readonly TomlTable table;

    public TomlDocumentNode RootNode
        => new(table.RootNode);

    public long LineNumber { get; internal set; }

    public TomlDocument()
    {
        table = new();
        LineNumber = 0;
    }

    internal bool ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        table.ToTomlString(ref writer);
        return true;
    }

    internal void Deserialize(ref Utf8SequenceReader reader, CsTomlSerializerOptions? options)
    {
        options ??= CsTomlSerializerOptions.Default;
        var parser = new CsTomlParser(ref reader);

        List<TomlString>? comments = default;
        List<CsTomlException>? exceptions = default;
        TomlTableNode? currentNode = table.RootNode;

        try
        {
            while (parser.TryRead())
            {
                try
                {
                    switch (parser.CurrentState)
                    {
                        case ParserState.Comment:
                            comments ??= new List<TomlString>();
                            comments?.Add((TomlString)parser.GetComment()!);
                            break;

                        case ParserState.KeyValue:
                            currentNode!.AddKeyValue(parser.GetDottedKeySpan(), parser.GetValue()!, comments);
                            comments?.Clear();
                            break;

                        case ParserState.TableHeader:
                            table.AddTableHeader(parser.GetDottedKeySpan(), comments, out currentNode);
                            comments?.Clear();
                            break;

                        case ParserState.ArrayOfTablesHeader:
                            table.AddArrayOfTablesHeader(parser.GetDottedKeySpan(), comments, out currentNode);
                            comments?.Clear();
                            break;

                        case ParserState.ThrowException:
                            exceptions ??= new List<CsTomlException>();
                            exceptions?.Add(parser.GetException()!);
                            comments?.Clear();
                            break;

                        default:
                            break;
                    }
                }
                catch (CsTomlException cte)
                {
                    exceptions ??= new List<CsTomlException>();
                    exceptions?.Add(new CsTomlLineNumberException(cte, parser.LineNumber));
                }
            }

        }
        finally
        {
            parser.Return();
        }

        LineNumber = parser.LineNumber;
        if (exceptions?.Count > 0)
        {
            throw new CsTomlSerializeException(
                "An error occurred while parsing the TOML file. Check Exceptions for information on exceptions raised during parsing.",
                exceptions);
        }
    }
}
