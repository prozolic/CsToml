using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Diagnostics;

namespace CsToml;

[DebuggerDisplay("{table}")]
public partial class TomlDocument : ITomlSerializedObject<TomlDocument>
{
    #region ICsTomlPackagePart

    static void ITomlSerializedObject<TomlDocument>.Serialize<TBufferWriter, TSerializer>(ref TBufferWriter writer, TomlDocument? target, CsTomlSerializerOptions? options)
    {
        options ??= CsTomlSerializerOptions.Default;

        try
        {
            var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
            target?.Serialize(ref utf8Writer, options);
        }
        catch (CsTomlException cte)
        {
            throw new CsTomlSerializeException([cte]);
        }
    }

    static TomlDocument ITomlSerializedObject<TomlDocument>.Deserialize<TSerializer>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options)
    {
        options ??= CsTomlSerializerOptions.Default;

        var package = new TomlDocument();
        var reader = new Utf8SequenceReader(tomlText);
        package.Deserialize(ref reader, options);
        return package;
    }

    static TomlDocument ITomlSerializedObject<TomlDocument>.Deserialize<TSerializer>(in ReadOnlySequence<byte> tomlTextSequence, CsTomlSerializerOptions? options)
    {
        options ??= CsTomlSerializerOptions.Default;

        var package = new TomlDocument();
        var reader = new Utf8SequenceReader(tomlTextSequence);
        package.Deserialize(ref reader, options);
        return package;
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
        var parser = new CsTomlParser(ref reader);

        List<TomlString>? comments = default;
        List<CsTomlException>? exceptions = default;
        TomlTableNode? currentNode = table.RootNode;
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

        LineNumber = parser.LineNumber;
        if (exceptions?.Count > 0)
            throw new CsTomlSerializeException(exceptions);
    }

}
