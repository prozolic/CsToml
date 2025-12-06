using System.Collections.Immutable;

namespace CsToml.Formatter;

public sealed class ImmutableListFormatter<T> : StructuralCollectionBaseFormatter<ImmutableList<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override ImmutableList<T> Complete(List<T> collection)
    {
        return collection.ToImmutableList();
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }

    public override void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ImmutableList<T> target, CsTomlSerializerOptions options)
    {
        IReadOnlyListSerializer<T>.Serialize(ref writer, new CollectionContent(target), options);
    }

    public override bool TrySerialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, ImmutableList<T> target, CsTomlSerializerOptions options)
    {
        return IReadOnlyListSerializer<T>.TrySerializeTomlArrayHeaderStyle(ref writer, header, new CollectionContent(target), options);
    }

}
