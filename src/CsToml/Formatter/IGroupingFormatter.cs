using CsToml.Error;
using CsToml.Values;
using System.Buffers;
using System.Collections;
using System.Diagnostics;

namespace CsToml.Formatter;

public sealed class IGroupingFormatter<TKey, TValue> : ITomlValueFormatter<IGrouping<TKey, TValue>>
{
    public IGrouping<TKey, TValue> Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    { 
        if (rootNode.NodeCount == 0)
        {
            if (rootNode.Value is TomlTable table)
            {
                var tableDocumentNode = new TomlDocumentNode(table.RootNode);
                return Deserialize(ref tableDocumentNode, options);
            }
            else if (rootNode.Value is TomlInlineTable inlineTable)
            {
                var tableDocumentNode = new TomlDocumentNode(inlineTable.RootNode);
                return Deserialize(ref tableDocumentNode, options);
            }
            return Grouping.Empty;
        }

        var keyFormatter = options.Resolver.GetFormatter<TKey>();
        var valueFormatter = options.Resolver.GetFormatter<IEnumerable<TValue>>();

        foreach ((var key, var node) in rootNode.Node.KeyValuePairs)
        {
            if (node.Value!.HasValue)
            {
                var keyNode = new TomlDocumentNode(key);
                var valueNode = new TomlDocumentNode(node);

                return new Grouping(
                    keyFormatter!.Deserialize(ref keyNode, options),
                    valueFormatter!.Deserialize(ref valueNode, options));
            }
            else
            {
                return Grouping.Empty;
            }
        }
        return Grouping.Empty;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, IGrouping<TKey, TValue> target, CsTomlSerializerOptions options) where TBufferWriter : IBufferWriter<byte>
    {
        if (target == null)
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(IGrouping<TKey, TValue>));
            return;
        }

        var keyFormatter = options.Resolver.GetFormatter<TKey>();
        var valueFormatter = options.Resolver.GetFormatter<IEnumerable<TValue>>();

        if (!writer.IsRoot)
            writer.BeginCurrentState(TomlValueState.ArrayOfTable);

        writer.BeginScope();
        keyFormatter!.Serialize(ref writer, target.Key, options);
        writer.WriteEqual();
        valueFormatter!.Serialize(ref writer, target, options);
        writer.EndScope();

        if (!writer.IsRoot)
            writer.EndCurrentState();

    }

    [DebuggerDisplay("Key = {Key}")]
    private sealed class Grouping : IGrouping<TKey, TValue>
    {
        public static readonly Grouping Empty = new(default!, Array.Empty<TValue>());
        private readonly TKey key;
        private readonly IEnumerable<TValue> values;

        public Grouping(TKey key, IEnumerable<TValue> values)
        {
            this.key = key;
            this.values = values;
        }

        public TKey Key => key;

        public IEnumerator<TValue> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }
}
