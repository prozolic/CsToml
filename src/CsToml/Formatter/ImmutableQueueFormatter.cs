using System.Collections.Immutable;

namespace CsToml.Formatter;

internal sealed class ImmutableQueueFormatter<T> : CollectionBaseFormatter<ImmutableQueue<T>, T, Queue<T>>
{
    protected override void AddValue(Queue<T> mediator, T element)
    {
        mediator.Enqueue(element);
    }

    protected override ImmutableQueue<T> Complete(Queue<T> collection)
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
}