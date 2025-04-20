using System.Collections.Frozen;

namespace CsToml.Formatter;

public sealed class FrozenSetFormatter<T> : CollectionBaseFormatter<FrozenSet<T>, T, HashSet<T>>
{
    protected override void AddValue(HashSet<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override FrozenSet<T> Complete(HashSet<T> collection)
    {
        return collection.ToFrozenSet();
    }

    protected override HashSet<T> CreateCollection(int capacity)
    {
        return new HashSet<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, FrozenSet<T> target, CsTomlSerializerOptions options)
    {
        writer.BeginArray();
        if (target.Count == 0)
        {
            writer.EndArray();
            return;
        }

        // Use FrozenSet<T>.GetEnumerator directly instead of IEnumerable<T>.GetEnumerator.
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
