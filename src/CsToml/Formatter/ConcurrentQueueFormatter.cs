using System.Collections.Concurrent;

namespace CsToml.Formatter;

public sealed class ConcurrentQueueFormatter<T> : CollectionBaseFormatter<ConcurrentQueue<T>, T, ConcurrentQueue<T>>
{
    protected override void AddValue(ConcurrentQueue<T> mediator, T element)
    {
        mediator.Enqueue(element);
    }

    protected override ConcurrentQueue<T> Complete(ConcurrentQueue<T> collection)
    {
        return collection;
    }

    protected override ConcurrentQueue<T> CreateCollection(int capacity)
    {
        return new ConcurrentQueue<T>();
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ConcurrentQueue<T> target, CsTomlSerializerOptions options)
    {
        IEnumerableSerializer<T>.Instance.Serialize(ref writer, new CollectionContent(target), options);
    }

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, ConcurrentQueue<T> target, CsTomlSerializerOptions options)
    {
        return IEnumerableSerializer<T>.Instance.TrySerializeTomlArrayHeaderStyle(ref writer, header, new CollectionContent(target), options);
    }
}
