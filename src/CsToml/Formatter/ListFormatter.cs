

namespace CsToml.Formatter;

public sealed class ListFormatter<T> : CollectionBaseFormatter<List<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override List<T> Complete(List<T> collection)
    {
        return collection;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }

    protected override void SerializeCollection(ref Utf8TomlDocumentWriter writer, List<T> target, CsTomlSerializerOptions options)
    {
        ListSerializer<T>.Serialize(ref writer, target, options);
    }

    protected override bool TrySerializeTomlArrayHeaderStyle(ref Utf8TomlDocumentWriter writer, ReadOnlySpan<byte> header, List<T> target, CsTomlSerializerOptions options)
    {
        return ListSerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, target, options);
    }
}
