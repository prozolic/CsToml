
namespace CsToml.Formatter;

public sealed class HashSetFormatter<T> : CollectionBaseFormatter<HashSet<T>, T, HashSet<T>>
{
    protected override void AddValue(HashSet<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override HashSet<T> Complete(HashSet<T> collection)
    {
        return collection;
    }

    protected override HashSet<T> CreateCollection(int capacity)
    {
        return new HashSet<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, HashSet<T> target, CsTomlSerializerOptions options)
    {
        if (target.Count == 0)
        {
            writer.BeginArray();
            writer.EndArray();
            return;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        writer.BeginArray();

        // Use HashSet<T>.GetEnumerator directly instead of IEnumerable<T>.GetEnumerator.
        var en = target.GetEnumerator();
        if (!en.MoveNext())
        {
            writer.EndArray();
            return;
        }

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