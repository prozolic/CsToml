using System.Collections.Frozen;
using System.Collections.Immutable;

namespace CsToml.Formatter;

public sealed class IImmutableSetFormatter<T> : CollectionBaseFormatter<IImmutableSet<T>, T, HashSet<T>>
{
    protected override void AddValue(HashSet<T> mediator, T element)
    {
        mediator.Add(element);
    }
    protected override IImmutableSet<T> Complete(HashSet<T> collection)
    {
        return collection.ToImmutableHashSet();
    }
    protected override HashSet<T> CreateCollection(int capacity)
    {
        return new HashSet<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, IImmutableSet<T> target, CsTomlSerializerOptions options)
    {
        if (target is ImmutableHashSet<T> immHashSetTarget)
        {
            var serializer = new EnumeratorStructSerializer<T, ImmutableHashSet<T>.Enumerator>(target.Count, immHashSetTarget.GetEnumerator());
            serializer.Serialize(ref writer, options);
        }
        else if (target is ImmutableSortedSet<T> immutableSortedSet)
        {
            var serializer = new EnumeratorStructSerializer<T, ImmutableSortedSet<T>.Enumerator>(target.Count, immutableSortedSet.GetEnumerator());
            serializer.Serialize(ref writer, options);
        }
        else
        {
            IEnumerableSerializer<T>.Serialize(ref writer, new CollectionContent(target), options);
        }
    }

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, IImmutableSet<T> target, CsTomlSerializerOptions options)
    {
        if (target is ImmutableHashSet<T> immutableHashSet)
        {
            var serializer = new EnumeratorStructSerializer<T, ImmutableHashSet<T>.Enumerator>(target.Count, immutableHashSet.GetEnumerator());
            return serializer.TrySerializeTomlArrayHeaderStyle(ref writer, header, options);
        }
        else if (target is ImmutableSortedSet<T> immutableSortedSet)
        {
            var serializer = new EnumeratorStructSerializer<T, ImmutableSortedSet<T>.Enumerator>(target.Count, immutableSortedSet.GetEnumerator());
            return serializer.TrySerializeTomlArrayHeaderStyle(ref writer, header, options);
        }

        return IEnumerableSerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, new CollectionContent(target), options);
    }
}
