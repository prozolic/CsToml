
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Formatter;

public sealed class IListFormatter<T> : CollectionBaseFormatter<IList<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override IList<T> Complete(List<T> collection)
    {
        return collection;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, IList<T> target, CsTomlSerializerOptions options)
    {
        if (target.GetType() == typeof(T[]))
        {
            ArraySerializer<T>.Serialize(ref writer, Unsafe.As<T[]>(target), options);
        }
        else if (target is List<T>)
        {
            ListSerializer<T>.Serialize(ref writer, Unsafe.As<List<T>>(target), options);
        }
        else
        {
            IListSerializer<T>.Serialize(ref writer, target, options);
        }
    }

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, IList<T> target, CsTomlSerializerOptions options)
    {
        if (target.GetType() == typeof(T[]))
        {
            return ArraySerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, Unsafe.As<T[]>(target), options);
        }
        else if (target is List<T>)
        {
            return ListSerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, Unsafe.As<List<T>>(target), options);
        }
        else
        {
            return IListSerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, target, options);
        }
    }
}
