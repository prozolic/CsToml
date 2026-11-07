using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices; // InlineArray16<T>

namespace CsToml;

[DebuggerDisplay("Toml Document = {table.Count}")]
public partial class TomlDocument
{
    private readonly TomlTable table;

    public TomlDocumentNode RootNode
        => new(table, true);

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

    internal void Parse(ref Utf8SequenceReader reader, CsTomlSerializerOptions options)
    {
        var initialComments = default(InlineArray16<TomlString>);
        Span<TomlString> initialCommentsSpan = initialComments;
        var commentsBuilder = new InlineArrayBuilder<TomlString>(initialCommentsSpan);

        List<CsTomlParseException>? exceptions = null;
        TomlTable currentTable = table;

        // Comments are attached to the table / array of tables in commentOwner,
        // or to its key/value at commentEntryIndex when the index is not negative.
        TomlValue commentOwner = table;
        var commentEntryIndex = -1;

        // Key-reuse sources: keys repeat because same-schema tables repeat, so the
        // previously completed sibling table (mirror) already holds identical key instances.
        TomlTable? tableKeyMirror = null;
        TomlTable? previousTable = null;
        TomlTable? aotPreviousElement = null;
        TomlTable? aotCurrentElement = null;

        var parser = new CsTomlParser(ref reader, options);
        currentTable.EnsureCapacity(parser.reader.EstimateDirectKeyCount());

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
                        var keyValueTable = currentTable;
                        var keyMirror = tableKeyMirror;
                        var readResult = parser.reader.ReadKey(true, keyMirror, out var key);
                        while (readResult == ReadKeyResult.FoundDot)
                        {
                            keyValueTable = keyValueTable.GetOrAddKeyTable(key!);
                            keyMirror = keyMirror?.TryGetMirrorChild(key!);
                            readResult = parser.reader.ReadKey(false, keyMirror, out key);
                        }

                        var value = parser.reader.ReadValue();
                        var entryIndex = keyValueTable.AddKeyValue(key!, value);
                        commentOwner = keyValueTable;
                        commentEntryIndex = entryIndex;
                        break;

                    case ParserState.TableHeader:
                        currentTable = table;
                        var readTableHeaderResult = parser.reader.ReadTableHeaderKey(true, currentTable, out var tableHeaderKey);
                        var tableMirror = default(TomlTable);
                        while (readTableHeaderResult == ReadKeyResult.FoundDot)
                        {
                            currentTable = currentTable.GetOrAddTableHeaderKeyTable(tableHeaderKey!, out _);
                            tableMirror = currentTable == aotCurrentElement ? aotPreviousElement : tableMirror?.TryGetMirrorChild(tableHeaderKey!);
                            readTableHeaderResult = parser.reader.ReadTableHeaderKey(false, currentTable, out tableHeaderKey);
                        }

                        currentTable = currentTable.AddTableHeaderKeyLastTable(tableHeaderKey!);
                        var structuralMirror = tableMirror?.TryGetMirrorChild(tableHeaderKey!);
                        currentTable.EnsureCapacity(structuralMirror is { Count: > 0 } ? structuralMirror.Count : parser.reader.EstimateDirectKeyCount());
                        tableKeyMirror = structuralMirror ?? previousTable;
                        previousTable = currentTable;
                        commentOwner = currentTable;
                        commentEntryIndex = -1;
                        break;

                    case ParserState.ArrayOfTablesHeader:
                        currentTable = table;
                        var readArrayOfTableHeaderResult = parser.reader.ReadArrayOfTableHeaderKey(true, currentTable, out var arrayOfTableHeaderKey);
                        while (readArrayOfTableHeaderResult == ReadKeyResult.FoundDot)
                        {
                            currentTable = currentTable.GetOrAddArrayOfTableHeaderKeyTable(arrayOfTableHeaderKey!);
                            readArrayOfTableHeaderResult = parser.reader.ReadArrayOfTableHeaderKey(false, currentTable, out arrayOfTableHeaderKey);
                        }
                        currentTable = currentTable.AddArrayOfTableHeaderKeyLastTable(arrayOfTableHeaderKey!, out var tableArray, out aotPreviousElement);
                        currentTable.EnsureCapacity(aotPreviousElement is { Count: > 0 } ? aotPreviousElement.Count : parser.reader.EstimateDirectKeyCount());
                        aotCurrentElement = currentTable;
                        tableKeyMirror = aotPreviousElement;
                        previousTable = currentTable;
                        commentOwner = tableArray;
                        commentEntryIndex = -1;
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
                    Span<TomlString> commentsSpan;
                    if (commentOwner is TomlTable commentTable)
                    {
                        commentsSpan = commentEntryIndex >= 0
                            ? commentTable.AddKeyValueComments(commentEntryIndex, commentsBuilder.Count)
                            : commentTable.AddComments(commentsBuilder.Count);
                    }
                    else
                    {
                        commentsSpan = ((TomlTableArray)commentOwner).AddComments(commentsBuilder.Count);
                    }
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
