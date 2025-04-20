using System.Collections.Immutable;

namespace CsToml.Formatter;

public sealed class IImmutableQueueFormatter<T> : CollectionBaseFormatter<IImmutableQueue<T>, T, Queue<T>>
{
    protected override void AddValue(Queue<T> mediator, T element)
    {
        mediator.Enqueue(element);
    }

    protected override IImmutableQueue<T> Complete(Queue<T> collection)
    {
        var queue = ImmutableQueue<T>.Empty;
        for (var i = collection.Count - 1; i >= 0; i--)
        {
            queue = queue.Enqueue(collection.Dequeue());
        }
        return queue;
    }

    protected override Queue<T> CreateCollection(int capacity)
    {
        return new Queue<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, IImmutableQueue<T> target, CsTomlSerializerOptions options)
    {
        if (target is ImmutableQueue<T> immQueueTarget)
        {
            writer.BeginArray();
            if (immQueueTarget.IsEmpty)
            {
                writer.EndArray();
                return;
            }

            // Use ImmutableHashSet<T>.GetEnumerator directly instead of IEnumerable<T>.GetEnumerator.
            var en = immQueueTarget.GetEnumerator();
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
            return;
        }

        base.SerializeCollection(ref writer, target, options);
    }
}