using System.Collections.Concurrent;
using System.Xml.Linq;

namespace CsToml.Formatter;

public sealed class BlockingCollectionFormatter<T> : CollectionBaseFormatter<BlockingCollection<T>, T, BlockingCollection<T>>
{
    protected override void AddValue(BlockingCollection<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override BlockingCollection<T> Complete(BlockingCollection<T> collection)
    {
        return collection;
    }

    protected override BlockingCollection<T> CreateCollection(int capacity)
    {
        return new BlockingCollection<T>(capacity);
    }

    protected override void SerializeCollection(ref Utf8TomlDocumentWriter writer, BlockingCollection<T> target, CsTomlSerializerOptions options)
    {
        IEnumerableSerializer<T>.Serialize(ref writer, target, options);
    }

    protected override bool TrySerializeTomlArrayHeaderStyle(ref Utf8TomlDocumentWriter writer, ReadOnlySpan<byte> header, BlockingCollection<T> target, CsTomlSerializerOptions options)
    {
        return IEnumerableSerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, target, options);
    }
}
