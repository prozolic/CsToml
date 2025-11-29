

namespace CsToml.Formatter;

public sealed class QueueFormatter<T> : CollectionBaseFormatter<Queue<T>, T, Queue<T>>
{
    protected override void AddValue(Queue<T> mediator, T element)
    {
        mediator.Enqueue(element);
    }

    protected override Queue<T> Complete(Queue<T> collection)
    {
        return collection;
    }

    protected override Queue<T> CreateCollection(int capacity)
    {
        return new Queue<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Queue<T> target, CsTomlSerializerOptions options)
    {
        var serializer = new EnumeratorStructSerializer<T, Queue<T>.Enumerator>(target.Count, target.GetEnumerator());
        serializer.Serialize(ref writer, options);
    }

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, Queue<T> target, CsTomlSerializerOptions options)
    {
        var serializer = new EnumeratorStructSerializer<T, Queue<T>.Enumerator>(target.Count, target.GetEnumerator());
        return serializer.TrySerializeTomlArrayHeaderStyle(ref writer, header, options);
    }
}