
namespace CsToml.Formatter;

internal sealed class LinkedListFormatter<T> : CollectionBaseFormatter<LinkedList<T>, T, LinkedList<T>>
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
}
