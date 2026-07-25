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

        List<CsTomlParseException>? exceptions = null;
        TomlTableNode currentNode = table.RootNode;
        TomlTableNode commentNode = table.RootNode;

        TomlTableNodeHolder nodeHolder = default;

        // Key-reuse sources: keys repeat because same-schema tables repeat, so the
        // previously completed sibling table (mirror) already holds identical key instances.
        TomlTableNode? tableKeyMirror = null;
        TomlTableNode? previousTableNode = null;
        TomlTableNode? aotPreviousElementRoot = null;
        TomlTableNode? aotCurrentElementRoot = null;

        var parser = new CsTomlParser(ref reader, options);
        currentNode.ReserveNodeCapacity(parser.reader.EstimateDirectKeyCount());

        while (parser.Read())
        {
            try
            {
                switch (parser.CurrentState)
                {
                    case ParserState.Comment:
                        if (!parser.IsEndComment)
                        {
                            commentsBuilder.Add(parser.ReadComment());
                        }
                        else
                        {
                            // Currently, only validation is performed for end comments.
                            parser.ReadEndComment();
                        }
                        continue;

                    case ParserState.KeyValue:
                        var node = currentNode;
                        nodeHolder.node = tableKeyMirror;
                        var readResult = parser.reader.ReadKey(true, nodeHolder, out var key);
                        while (readResult == ReadKeyResult.FoundDot)
                        {
                            node = node.GetOrAddKeyNode(key!);
                            nodeHolder.node = nodeHolder.node?.TryGetMirrorChild(key!);
                            readResult = parser.reader.ReadKey(false, nodeHolder, out key);
                        }

                        var value = parser.reader.ReadValue();
                        node = node.AddKeyValueNode(key!, value);
                        commentNode = node;
                        break;

                    case ParserState.TableHeader:
                        currentNode = table.RootNode;
                        nodeHolder.node = currentNode;
                        var readTableHeaderResult = parser.reader.ReadTableHeaderKey(true, nodeHolder, out var tableHeaderKey);
                        var tableMirror = default(TomlTableNode);
                        while (readTableHeaderResult == ReadKeyResult.FoundDot)
                        {
                            currentNode = currentNode.GetOrAddTableHeaderKeyNode(tableHeaderKey!, out bool newNode);
                            tableMirror = currentNode == aotCurrentElementRoot ? aotPreviousElementRoot : tableMirror?.TryGetMirrorChild(tableHeaderKey!);
                            nodeHolder.node = currentNode;
                            readTableHeaderResult = parser.reader.ReadTableHeaderKey(false, nodeHolder, out tableHeaderKey);
                        }

                        currentNode = currentNode.AddTableHeaderKeyLastNode(tableHeaderKey!);
                        currentNode.ReserveNodeCapacity(parser.reader.EstimateDirectKeyCount());
                        tableKeyMirror = tableMirror?.TryGetMirrorChild(tableHeaderKey!) ?? previousTableNode;
                        previousTableNode = currentNode;
                        commentNode = currentNode;
                        break;

                    case ParserState.ArrayOfTablesHeader:
                        currentNode = table.RootNode;
                        nodeHolder.node = currentNode;
                        var readArrayOfTableHeaderResult = parser.reader.ReadArrayOfTableHeaderKey(true, nodeHolder, out var arrayOfTableHeaderKey);
                        while (readArrayOfTableHeaderResult == ReadKeyResult.FoundDot)
                        {
                            currentNode = currentNode.GetOrAddArrayOfTableHeaderKeyNode(arrayOfTableHeaderKey!, false, out bool newNode);
                            nodeHolder.node = currentNode;
                            readArrayOfTableHeaderResult = parser.reader.ReadArrayOfTableHeaderKey(false, nodeHolder, out arrayOfTableHeaderKey);
                        }
                        currentNode = currentNode.AddArrayOfTableHeaderKeyLastNode(arrayOfTableHeaderKey!, out commentNode, out aotPreviousElementRoot);
                        currentNode.ReserveNodeCapacity(parser.reader.EstimateDirectKeyCount());
                        aotCurrentElementRoot = currentNode;
                        tableKeyMirror = aotPreviousElementRoot;
                        previousTableNode = currentNode;
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

                if (!parser.ReadEnd())
                {
                    break;
                }
                if (parser.CurrentState == ParserState.ThrowException)
                {
                    (exceptions ??= new()).Add(parser.GetException()!);
                }
            }
            catch (CsTomlException cte)
            {
                (exceptions ??= new()).Add(new CsTomlParseException(cte, parser.LineNumber));
                parser.reader.SkipOneLine();
            }
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