using System.Collections.Immutable;

namespace CsToml.Formatter;

public sealed class ImmutableSortedSetFormatter<T> : CollectionBaseFormatter<ImmutableSortedSet<T>, T, SortedSet<T>>
{
    protected override void AddValue(SortedSet<T> mediator, T element)
    {
        mediator.Add(element);
    }
    protected override ImmutableSortedSet<T> Complete(SortedSet<T> collection)
    {
        return collection.ToImmutableSortedSet();
    }
    protected override SortedSet<T> CreateCollection(int capacity)
    {
        return new SortedSet<T>();
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ImmutableSortedSet<T> target, CsTomlSerializerOptions options)
    {
        writer.BeginArray();
        if (target.IsEmpty)
        {
            writer.EndArray();
            return;
        }

        // Use ImmutableSortedSet<T>.GetEnumerator directly instead of IEnumerable<T>.GetEnumerator.
        var en = target.GetEnumerator();
        en.MoveNext();

        var formatter = options.Resolver.GetFormatter<T>()!;
        formatter.Serialize(ref writer, en.Current!, options);
        if (!en.MoveNext())
        {
            writer.WriteSpace();
            writer.EndArray();
            return;
        }

        do
        {
            writer.Write(TomlCodes.Symbol.COMMA);
            writer.WriteSpace();
            formatter.Serialize(ref writer, en.Current!, options);

        } while (en.MoveNext());
        writer.WriteSpace();
        writer.EndArray();
    }
}
