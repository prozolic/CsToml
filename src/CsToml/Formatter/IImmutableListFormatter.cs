using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace CsToml.Formatter;

public sealed class IImmutableListFormatter<T> : CollectionBaseFormatter<IImmutableList<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override IImmutableList<T> Complete(List<T> collection)
    {
        return collection.ToImmutableList();
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, IImmutableList<T> target, CsTomlSerializerOptions options)
    {
        if (target is ImmutableArray<T> immArrayTarget)
        {
            var array = ImmutableCollectionsMarshal.AsArray(immArrayTarget);
            ArraySerializer<T>.Serialize(ref writer, new CollectionContent(array!), options);
        }
        else
        {
            IReadOnlyListSerializer<T>.Serialize(ref writer, new CollectionContent(target), options);
        }
    }

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, IImmutableList<T> target, CsTomlSerializerOptions options)
    {
        if (target is ImmutableArray<T> immArrayTarget)
        {
            var array = ImmutableCollectionsMarshal.AsArray(immArrayTarget);
            return ArraySerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, new CollectionContent(target), options);
        }
        else
        {
            return IReadOnlyListSerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, new CollectionContent(target), options);
        }
    }
}
