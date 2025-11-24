
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
        if (target is T[])
        {
            ArraySerializer<T>.Instance.Serialize(ref writer, new CollectionContent(target), options);
        }
        else if (target is List<T>)
        {
            ListSerializer<T>.Instance.Serialize(ref writer, new CollectionContent(target), options);
        }
        else
        {
            IListSerializer<T>.Instance.Serialize(ref writer, new CollectionContent(target), options);
        }
    }

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, IList<T> target, CsTomlSerializerOptions options)
    {
        if (target is T[])
        {
            return ArraySerializer<T>.Instance.TrySerializeTomlArrayHeaderStyle(ref writer, header, new CollectionContent(target), options);
        }
        else if (target is List<T> stTarget)
        {
            return ListSerializer<T>.Instance.TrySerializeTomlArrayHeaderStyle(ref writer, header, new CollectionContent(target), options);
        }
        else
        {
            return IListSerializer<T>.Instance.TrySerializeTomlArrayHeaderStyle(ref writer, header, new CollectionContent(target), options);
        }
    }
}
