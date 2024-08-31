using System.Collections.Concurrent;

namespace CsToml.Formatter;

internal sealed class ConcurrentStackFormatter<T> : CollectionBaseFormatter<ConcurrentStack<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override ConcurrentStack<T> Complete(List<T> collection)
    {
        var stack = new ConcurrentStack<T>();
        for (var i = collection.Count - 1; i >= 0; i--)
        {
            stack.Push(collection[i]);
        }
        return stack;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }
}
