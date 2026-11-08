
using System.Runtime.CompilerServices;

namespace CsToml.Formatter;

public sealed class ICollectionFormatter<T> : CollectionBaseFormatter<ICollection<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override ICollection<T> Complete(List<T> collection)
    {
        return collection;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ICollection<T> target, CsTomlSerializerOptions options)
    {
        if (target.GetType() == typeof(T[]))
        {
            ArraySerializer<T>.Serialize(ref writer, Unsafe.As<T[]>(target), options);
        }
        else if (target is List<T>)
        {
            ListSerializer<T>.Serialize(ref writer, Unsafe.As<List<T>>(target), options);
        }
        else if (target is IList<T>)
        {
            IListSerializer<T>.Serialize(ref writer, Unsafe.As<IList<T>>(target), options);
        }
            else if (target is IReadOnlyList<T>)
        {
            IReadOnlyListSerializer<T>.Serialize(ref writer, Unsafe.As<IReadOnlyList<T>>(target), options);
        }
        else
        {
            IEnumerableSerializer<T>.Serialize(ref writer, target, options);
        }
    }

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, ICollection<T> target, CsTomlSerializerOptions options)
    {
        if (target.GetType() == typeof(T[]))
        {
            return ArraySerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, Unsafe.As<T[]>(target), options);
        }
        else if (target is List<T>)
        {
            return ListSerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, Unsafe.As<List<T>>(target), options);
        }
        else if (target is IList<T>)
        {
            return IListSerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, Unsafe.As<IList<T>>(target), options);
        }
        else if (target is IReadOnlyList<T>)
        {
            return IReadOnlyListSerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, Unsafe.As<IReadOnlyList<T>>(target), options);
        }
        else
        {
            return IEnumerableSerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, target, options);
        }
    }
}
