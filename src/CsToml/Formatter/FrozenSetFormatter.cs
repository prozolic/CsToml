using System.Collections.Frozen;

namespace CsToml.Formatter;

public sealed class FrozenSetFormatter<T> : CollectionBaseFormatter<FrozenSet<T>, T, HashSet<T>>
{
    protected override void AddValue(HashSet<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override FrozenSet<T> Complete(HashSet<T> collection)
    {
        return collection.ToFrozenSet();
    }

    protected override HashSet<T> CreateCollection(int capacity)
    {
        return new HashSet<T>(capacity);
    }

    protected override void SerializeCollection(ref Utf8TomlDocumentWriter writer, FrozenSet<T> target, CsTomlSerializerOptions options)
    {
        var serializer = new EnumeratorStructSerializer<T, FrozenSet<T>.Enumerator>(target.Count, target.GetEnumerator());
        serializer.Serialize(ref writer, options);
    }

    protected override bool TrySerializeTomlArrayHeaderStyle(ref Utf8TomlDocumentWriter writer, ReadOnlySpan<byte> header, FrozenSet<T> target, CsTomlSerializerOptions options)
    {
        var serializer = new EnumeratorStructSerializer<T, FrozenSet<T>.Enumerator>(target.Count, target.GetEnumerator());
        return serializer.TrySerializeTomlArrayHeaderStyle(ref writer, header, options);
    }
}
