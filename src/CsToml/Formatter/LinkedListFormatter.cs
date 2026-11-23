

namespace CsToml.Formatter;

public sealed class LinkedListFormatter<T> : CollectionBaseFormatter<LinkedList<T>, T, LinkedList<T>>
{
    protected override void AddValue(LinkedList<T> mediator, T element)
    {
        mediator.AddLast(element);
    }

    protected override LinkedList<T> Complete(LinkedList<T> collection)
    {
        return collection;
    }

    protected override LinkedList<T> CreateCollection(int capacity)
    {
        return new LinkedList<T>();
    }

    protected override void SerializeCollection(ref Utf8TomlDocumentWriter writer, LinkedList<T> target, CsTomlSerializerOptions options)
    {
        var serializer = new EnumeratorStructSerializer<T, LinkedList<T>.Enumerator>(target.Count, target.GetEnumerator());
        serializer.Serialize(ref writer, options);
    }

    protected override bool TrySerializeTomlArrayHeaderStyle(ref Utf8TomlDocumentWriter writer, ReadOnlySpan<byte> header, LinkedList<T> target, CsTomlSerializerOptions options)
    {
        var serializer = new EnumeratorStructSerializer<T, LinkedList<T>.Enumerator>(target.Count, target.GetEnumerator());
        return serializer.TrySerializeTomlArrayHeaderStyle(ref writer, header, options);
    }
}
