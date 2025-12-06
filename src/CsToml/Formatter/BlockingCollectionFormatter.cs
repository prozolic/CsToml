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

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, BlockingCollection<T> target, CsTomlSerializerOptions options)
    {
        IEnumerableSerializer<T>.Serialize(ref writer, new CollectionContent(target), options);
    }

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, BlockingCollection<T> target, CsTomlSerializerOptions options)
    {
        return IEnumerableSerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, new CollectionContent(target), options);
    }
}
