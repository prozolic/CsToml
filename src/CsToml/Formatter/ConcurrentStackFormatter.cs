using System.Collections.Concurrent;

namespace CsToml.Formatter;

public sealed class ConcurrentStackFormatter<T> : CollectionBaseFormatter<ConcurrentStack<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override ConcurrentStack<T> Complete(List<T> collection)
    {
        var stack = new ConcurrentStack<T>();
        for (var i = collection.Count - 1; i >= 0; i--)
        {
            stack.Push(collection[i]);
        }
        return stack;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ConcurrentStack<T> target, CsTomlSerializerOptions options)
    {
        IEnumerableSerializer<T>.Instance.Serialize(ref writer, new CollectionContent(target), options);
    }

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, ConcurrentStack<T> target, CsTomlSerializerOptions options)
    {
        return IEnumerableSerializer<T>.Instance.TrySerializeTomlArrayHeaderStyle(ref writer, header, new CollectionContent(target), options);
    }
}
