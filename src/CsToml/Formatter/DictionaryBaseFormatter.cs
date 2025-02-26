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

        var headerStyle = options.SerializeOptions.TableStyle == TomlTableStyle.Header && (writer.State == TomlValueState.Default || writer.State == TomlValueState.Table);
        writer.BeginCurrentState(headerStyle ? TomlValueState.Table : TomlValueState.ArrayOfTable);
        
        var enumerator = target.GetEnumerator();
        using (IEnumerator<KeyValuePair<TKey, TValue>> en = target.GetEnumerator())
        {
            writer.BeginScope();
            if (!en.MoveNext())
            {
                goto END;
            }

            SerializeKeyValue(ref writer, headerStyle, en.Current, options);
            if (!en.MoveNext())
            {
                goto END;
            }

            do
            {
                writer.EndKeyValue();
                SerializeKeyValue(ref writer, headerStyle, en.Current, options);
            } while (en.MoveNext());
        }
    END:
        writer.EndKeyValue(true);
        writer.EndScope();
        writer.EndCurrentState();

        static void SerializeKeyValue(ref Utf8TomlDocumentWriter<TBufferWriter> writer, bool style, KeyValuePair<TKey, TValue> pair, CsTomlSerializerOptions options)
        {
            var (key, value) = pair;

            if (style && value is IDictionary dict && dict.Count > 0)
            {
                writer.WriteTableHeaderForPrimitive(key);
                writer.WriteNewLine();
                writer.PushKeyForPrimitive(key);
            }
            else
            {
                writer.WriteKeyForPrimitive(key);
                writer.WriteEqual();
            }
            options.Resolver.GetFormatter<TValue>()!.Serialize(ref writer, value!, options);

            if (style && value is IDictionary dict2 && dict2.Count > 0)
            {
                writer.PopKey();
            }
        }
    }

    protected abstract void AddValue(TMediator mediator, TKey key, TValue value);

    protected abstract TDicitonary Complete(TMediator dictionary);

    protected abstract TMediator CreateMediator(int capacity);
}

