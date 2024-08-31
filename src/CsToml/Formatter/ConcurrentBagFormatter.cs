using System.Collections.Concurrent;

namespace CsToml.Formatter;

internal sealed class ConcurrentBagFormatter<T> : CollectionBaseFormatter<ConcurrentBag<T>, T, ConcurrentBag<T>>
{
    protected override void AddValue(ConcurrentBag<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override ConcurrentBag<T> Complete(ConcurrentBag<T> collection)
    {
        return collection;
    }

    protected override ConcurrentBag<T> CreateCollection(int capacity)
    {
        return new ConcurrentBag<T>();
    }
}