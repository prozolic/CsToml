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

    protected override void SerializeCollection(ref Utf8TomlDocumentWriter writer, ConcurrentQueue<T> target, CsTomlSerializerOptions options)
    {
        IEnumerableSerializer<T>.Serialize(ref writer, target, options);
    }

    protected override bool TrySerializeTomlArrayHeaderStyle(ref Utf8TomlDocumentWriter writer, ReadOnlySpan<byte> header, ConcurrentQueue<T> target, CsTomlSerializerOptions options)
    {
        return IEnumerableSerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, target, options);
    }
}
