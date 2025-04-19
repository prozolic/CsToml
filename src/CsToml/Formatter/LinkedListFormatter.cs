
namespace CsToml.Formatter;

public sealed class LinkedListFormatter<T> : CollectionBaseFormatter<LinkedList<T>, T, LinkedList<T>>
{
    protected override void AddValue(LinkedList<T> mediator, T element)
    {
        mediator.AddLast(element);
    }

    protected override LinkedList<T> Complete(LinkedList<T> collection)
    {
        return collection;
    }

    protected override LinkedList<T> CreateCollection(int capacity)
    {
        return new LinkedList<T>();
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, LinkedList<T> target, CsTomlSerializerOptions options)
    {
        if (target.Count == 0)
        {
            writer.BeginArray();
            writer.EndArray();
            return;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        writer.BeginArray();

        // Use LinkedList<T>.GetEnumerator directly instead of IEnumerable<T>.GetEnumerator.
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
