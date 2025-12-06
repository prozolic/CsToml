
namespace CsToml.Formatter;

public sealed class SortedSetFormatter<T> : CollectionBaseFormatter<SortedSet<T>, T, SortedSet<T>>
{
    protected override void AddValue(SortedSet<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override SortedSet<T> Complete(SortedSet<T> collection)
    {
        return collection;
    }

    protected override SortedSet<T> CreateCollection(int capacity)
    {
        return new SortedSet<T>();
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, SortedSet<T> target, CsTomlSerializerOptions options)
    {
        var serializer = new EnumeratorStructSerializer<T, SortedSet<T>.Enumerator>(target.Count, target.GetEnumerator());
        serializer.Serialize(ref writer, options);
    }

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, SortedSet<T> target, CsTomlSerializerOptions options)
    {
        var serializer = new EnumeratorStructSerializer<T, SortedSet<T>.Enumerator>(target.Count, target.GetEnumerator());
        return serializer.TrySerializeTomlArrayHeaderStyle(ref writer, header, options);
    }
}
