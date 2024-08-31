using System.Collections.Concurrent;

namespace CsToml.Formatter;

internal sealed class ConcurrentQueueFormatter<T> : CollectionBaseFormatter<ConcurrentQueue<T>, T, ConcurrentQueue<T>>
{
    protected override void AddValue(ConcurrentQueue<T> mediator, T element)
    {
        mediator.Enqueue(element);
    }

    protected override ConcurrentQueue<T> Complete(ConcurrentQueue<T> collection)
    {
        return collection;
    }

    protected override ConcurrentQueue<T> CreateCollection(int capacity)
    {
        return new ConcurrentQueue<T>();
    }
}
