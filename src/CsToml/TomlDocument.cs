using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices; // InlineArray16<T>

namespace CsToml;

[DebuggerDisplay("Toml Document = {table.RootNode.NodeCount}")]
public partial class TomlDocument : ITomlValueFormatter<TomlDocument>
{
    private readonly TomlTable table;

    public TomlDocumentNode RootNode
        => new(table.RootNode, true);

    public long LineNumber { get; internal set; }

    public TomlDocument()
    {
        table = new();
        LineNumber = 0;
    }

    TomlDocument ITomlValueFormatter<TomlDocument>.Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        return this;
    }

    void ITomlValueFormatter<TomlDocument>.Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TomlDocument target, CsTomlSerializerOptions options)
    {
        target!.ToTomlString(ref writer);
    }

    public IDictionary<TKey, TValue> ToDictionary<TKey, TValue>() where TKey : notnull
        => RootNode.GetValue<IDictionary<TKey, TValue>>();

    internal bool ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        table.ToTomlString(ref writer);
        return true;
    }

    internal void Parse(ref Utf8SequenceReader reader, CsTomlSerializerOptions options)
    {
        var initialComments = default(InlineArray16<TomlString>);
        Span<TomlString> initialCommentsSpan = initialComments;
        var commentsBuilder = new InlineArrayBuilder<TomlString>(initialCommentsSpan);

        List<CsTomlParseException>? exceptions = default;
        TomlTableNode currentNode = table.RootNode;
        TomlTableNode commentNode = table.RootNode;

        var parser = new CsTomlParser(ref reader, options);

        try
        {
            while (parser.TryRead())
            {
                try
                {
                    switch (parser.CurrentState)
                    {
                        case ParserState.Comment:
                            commentsBuilder.Add(parser.GetComment());
                            continue;

                        case ParserState.KeyValue:
                            var node = currentNode!.AddKeyValue(parser.GetDottedKeySpan(), parser.GetValue()!);
                            commentNode = node;
                            break;

                        case ParserState.TableHeader:
                            currentNode = table.AddTableHeader(parser.GetDottedKeySpan());
                            commentNode = currentNode;
                            break;

                        case ParserState.ArrayOfTablesHeader:
                            currentNode = table.AddArrayOfTablesHeader(parser.GetDottedKeySpan(), out commentNode);
                            break;

                        case ParserState.ThrowException:
                            exceptions ??= new List<CsTomlParseException>();
                            exceptions?.Add(parser.GetException()!);
                            break;

                        default:
                            break;
                    }

                    if (commentsBuilder.Count > 0)
                    {
                        var commentsSpan = commentNode.SetCommentCount(commentsBuilder.Count);
                        commentsBuilder.CopyToAndReturn(commentsSpan);   
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
                "Exceptions were thrown while parsing TOML. See the 'ParseExceptions' property for details about any errors.",
                exceptions);
        }
    }
}

internal sealed class TempTomlDocumentFormatter : ITomlValueFormatter<TomlDocument>
{
    public static readonly TempTomlDocumentFormatter Instance = new();

    TomlDocument ITomlValueFormatter<TomlDocument>.Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    void ITomlValueFormatter<TomlDocument>.Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TomlDocument target, CsTomlSerializerOptions options)
    {
        target!.ToTomlString(ref writer);
    }
}