using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Diagnostics;

namespace CsToml;

[DebuggerDisplay("Toml Document = {table.RootNode.NodeCount}")]
public partial class TomlDocument
{
    private readonly TomlTable table;

    public TomlDocumentNode RootNode
        => new(table.RootNode);

    public long LineNumber { get; internal set; }

    public TomlDocument()
    {
        table = new();
        LineNumber = 0;
    }

    public IDictionary<TKey, TValue> ToDictionary<TKey, TValue>() where TKey : notnull
        => RootNode.GetValue<IDictionary<TKey, TValue>>();

    internal bool ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        table.ToTomlString(ref writer);
        return true;
    }

    internal void Deserialize(ref Utf8SequenceReader reader, CsTomlSerializerOptions options)
    {
        var parser = new CsTomlParser(ref reader, options);

        List<TomlString>? comments = default;
        List<CsTomlParseException>? exceptions = default;
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
                            exceptions ??= new List<CsTomlParseException>();
                            exceptions?.Add(parser.GetException()!);
                            comments?.Clear();
                            break;

                        default:
                            break;
                    }
                }
                catch (CsTomlException cte)
                {
                    exceptions ??= new List<CsTomlParseException>();
                    exceptions?.Add(new CsTomlParseException(cte, parser.LineNumber));
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
                "Exceptions were thrown during the parsing TOML. Check 'Exceptions' property about exceptions thrown.",
                exceptions);
        }
    }
}
