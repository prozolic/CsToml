using CsToml.Error;
using CsToml.Formatter;
using CsToml.Formatter.Resolver;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Diagnostics;

namespace CsToml;

[DebuggerDisplay("{table}")]
public partial class TomlDocument : ITomlSerializedObject<TomlDocument>
{
    #region ITomlSerializedObject

    static void ITomlSerializedObject<TomlDocument>.Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TomlDocument? target, CsTomlSerializerOptions options)
    {
        options ??= CsTomlSerializerOptions.Default;

        try
        {
            target!.ToTomlString(ref writer);
        }
        catch (CsTomlException cte)
        {
            throw new CsTomlSerializeException([cte]);
        }
    }

    static TomlDocument ITomlSerializedObject<TomlDocument>.Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        return rootNode.Document;
    }

    static void ITomlSerializedObjectRegister.Register()
    {
        TomlSerializedObjectFormatterResolver.Register(innerSerializer);
    }

    private static readonly TomlSerializedObjectFormatter<TomlDocument> innerSerializer = new TomlSerializedObjectFormatter<TomlDocument>();

    #endregion

    private readonly TomlTable table;

    public TomlDocumentNode RootNode
        => new(this, table.RootNode);

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
                            table.AddKeyValue(parser.GetDottedKeySpan(), parser.GetValue()!, currentNode, comments);
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
                            exceptions?.Add(new CsTomlLineNumberException(parser.GetException()!, parser.LineNumber));
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
            throw new CsTomlSerializeException(exceptions);
    }
}
