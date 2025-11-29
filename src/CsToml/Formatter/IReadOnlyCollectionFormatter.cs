
using System.Runtime.InteropServices;

namespace CsToml.Formatter;

public sealed class IReadOnlyCollectionFormatter<T> : CollectionBaseFormatter<IReadOnlyCollection<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override IReadOnlyCollection<T> Complete(List<T> collection)
    {
        return collection;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, IReadOnlyCollection<T> target, CsTomlSerializerOptions options)
    {
        if (target is T[])
        {
            ArraySerializer<T>.Serialize(ref writer, new CollectionContent(target), options);
        }
        else if (target is List<T>)
        {
            ListSerializer<T>.Serialize(ref writer, new CollectionContent(target), options);
        }
        else if (target is IList<T>)
        {
            IListSerializer<T>.Serialize(ref writer, new CollectionContent(target), options);
        }
        else if (target is IReadOnlyList<T>)
        {
            IReadOnlyListSerializer<T>.Serialize(ref writer, new CollectionContent(target), options);
        }
        else
        {
            IEnumerableSerializer<T>.Serialize(ref writer, new CollectionContent(target), options);
        }
    }

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, IReadOnlyCollection<T> target, CsTomlSerializerOptions options)
    {
        if (target is T[])
        {
            return ArraySerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, new CollectionContent(target), options);
        }
        else if (target is List<T> stTarget)
        {
            return ListSerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, new CollectionContent(target), options);
        }
        else if (target is IList<T>)
        {
            return IListSerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, new CollectionContent(target), options);
        }
        else if (target is IReadOnlyList<T>)
        {
            return IReadOnlyListSerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, new CollectionContent(target), options);
        }
        else
        {
            return IEnumerableSerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, new CollectionContent(target), options);
        }
    }
}