using System.Runtime.InteropServices;

namespace CsToml.Formatter;

public sealed class StackFormatter<T> : CollectionBaseFormatter<Stack<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override Stack<T> Complete(List<T> collection)
    {
        var stack = new Stack<T>(collection.Count);
        var collectionSpan = CollectionsMarshal.AsSpan(collection);
        for (var i = collectionSpan.Length - 1; i >= 0; i--)
        {
            stack.Push(collectionSpan[i]);
        }
        return stack;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Stack<T> target, CsTomlSerializerOptions options)
    {
        var serializer = new EnumeratorStructSerializer<T, Stack<T>.Enumerator>(target.Count, target.GetEnumerator());
        serializer.Serialize(ref writer, options);
    }

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, Stack<T> target, CsTomlSerializerOptions options)
    {
        var serializer = new EnumeratorStructSerializer<T, Stack<T>.Enumerator>(target.Count, target.GetEnumerator());
        return serializer.TrySerializeTomlArrayHeaderStyle(ref writer, header, options);
    }
}
