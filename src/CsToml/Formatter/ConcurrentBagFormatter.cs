using System.Collections.Concurrent;

namespace CsToml.Formatter;

public sealed class ConcurrentBagFormatter<T> : CollectionBaseFormatter<ConcurrentBag<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override ConcurrentBag<T> Complete(List<T> collection)
    {
        var bag = new ConcurrentBag<T>();
        for (var i = collection.Count - 1; i >= 0; i--)
        {
            bag.Add(collection[i]);
        }
        return bag;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }
}