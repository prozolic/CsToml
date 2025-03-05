using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;
using System.Collections;
using System.Buffers;

namespace CsToml.Formatter;

public abstract class DictionaryBaseFormatter<TKey, TValue, TDicitonary, TMediator> : ITomlValueFormatter<TDicitonary>
    where TDicitonary : IEnumerable<KeyValuePair<TKey, TValue>>
{
    public TDicitonary Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.NodeCount == 0)
        {
            if (rootNode.Value is TomlTable table)
            {
                var tableDocumentNode = new TomlDocumentNode(table.RootNode);
                return options.Resolver.GetFormatter<TDicitonary>()!.Deserialize(ref tableDocumentNode, options);
            }
            else if (rootNode.Value is TomlInlineTable inlineTable)
            {
                var tableDocumentNode = new TomlDocumentNode(inlineTable.RootNode);
                return options.Resolver.GetFormatter<TDicitonary>()!.Deserialize(ref tableDocumentNode, options);
            }
            return Complete(CreateMediator(0));
        }
        else
        {
            var dictionary = CreateMediator(rootNode.NodeCount);
            var keyFormatter = options.Resolver.GetFormatter<TKey>();

            foreach ((var key, var node) in rootNode.Node.KeyValuePairs)
            {
                if (node.Value!.HasValue)
                {
                    var keyNode = new TomlDocumentNode(key);
                    var valueNode = new TomlDocumentNode(node);
                    AddValue(dictionary,
                        keyFormatter!.Deserialize(ref keyNode, options),
                        options.Resolver.GetFormatter<TValue>()!.Deserialize(ref valueNode, options));
                }
                else
                {
                    if (rootNode.Node.TryGetChildNode(key.Value, out var childNode))
                    {
                        var keyNode = new TomlDocumentNode(key);
                        var valueNode = new TomlDocumentNode(childNode!);
                        AddValue(dictionary,
                            keyFormatter!.Deserialize(ref keyNode, options),
                            (TValue)options.Resolver.GetFormatter<IDictionary<TKey, TValue>>()!.Deserialize(ref valueNode, options));
                    }
                }
            }

            return Complete(dictionary);
        }
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TDicitonary target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target == null)
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(TDicitonary));
            return;
        }

        var applyHeaderStyle = options.SerializeOptions.TableStyle == TomlTableStyle.Header && (writer.State == TomlValueState.Default || writer.State == TomlValueState.Table);
        if (!writer.IsRoot)
            writer.BeginCurrentState(applyHeaderStyle ? TomlValueState.Table : TomlValueState.ArrayOfTable);

        KeyValuePair<TKey, TValue> current;
        var enumerator = applyHeaderStyle ? target.OrderBy(x => x.Value is IDictionary).GetEnumerator() : target.GetEnumerator();
        using (IEnumerator<KeyValuePair<TKey, TValue>> en = enumerator)
        {
            if (!writer.IsRoot)
            {
                writer.BeginScope();
            }
            if (!en.MoveNext())
            {
                goto END;
            }

            current = en.Current;
            SerializeKeyValue(ref writer, applyHeaderStyle, current, options);
            if (!en.MoveNext())
            {
                goto ENDKEYVALUE;
            }

            do
            {
                if (!(applyHeaderStyle && current.Value is IDictionary))
                    writer.EndKeyValue();
                current = en.Current;
                SerializeKeyValue(ref writer, applyHeaderStyle, en.Current, options);
            } while (en.MoveNext());
        }
    ENDKEYVALUE:
        if (!(applyHeaderStyle && current.Value is IDictionary))
            writer.EndKeyValue(true);
    END:
        writer.EndScope();
        if (!writer.IsRoot)
            writer.EndCurrentState();

        static void SerializeKeyValue(ref Utf8TomlDocumentWriter<TBufferWriter> writer, bool applyHeaderStyle, KeyValuePair<TKey, TValue> pair, CsTomlSerializerOptions options)
        {
            var (key, value) = pair;

            if (value is IDictionary dict)
            {
                if (applyHeaderStyle)
                {
                    writer.WriteTableHeaderForPrimitive(key);
                    writer.WriteNewLine();
                    if (dict.Count > 0)
                    {
                        writer.BeginCurrentState(TomlValueState.Table);
                        writer.PushKeyForPrimitive(key);
                        options.Resolver.GetFormatter<TValue>()!.Serialize(ref writer, value!, options);
                        writer.PopKey();
                        writer.EndCurrentState();
                    }
                }
                else
                {
                    writer.WriteKeyForPrimitive(key);
                    writer.WriteEqual();
                    writer.BeginCurrentState(TomlValueState.ArrayOfTable);
                    options.Resolver.GetFormatter<TValue>()!.Serialize(ref writer, value!, options);
                    writer.EndCurrentState();
                }
            }
            else
            {
                writer.WriteKeyForPrimitive(key);
                writer.WriteEqual();
                options.Resolver.GetFormatter<TValue>()!.Serialize(ref writer, value!, options);
            }

        }
    }

    protected abstract void AddValue(TMediator mediator, TKey key, TValue value);

    protected abstract TDicitonary Complete(TMediator dictionary);

    protected abstract TMediator CreateMediator(int capacity);
}

