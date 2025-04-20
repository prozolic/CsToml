

namespace CsToml.Formatter;

public sealed class QueueFormatter<T> : CollectionBaseFormatter<Queue<T>, T, Queue<T>>
{
    protected override void AddValue(Queue<T> mediator, T element)
    {
        mediator.Enqueue(element);
    }

    protected override Queue<T> Complete(Queue<T> collection)
    {
        return collection;
    }

    protected override Queue<T> CreateCollection(int capacity)
    {
        return new Queue<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Queue<T> target, CsTomlSerializerOptions options)
    {
        if (target.Count == 0)
        {
            writer.BeginArray();
            writer.EndArray();
            return;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        writer.BeginArray();

        // Use Queue<T>.GetEnumerator directly instead of IEnumerable<T>.GetEnumerator.
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