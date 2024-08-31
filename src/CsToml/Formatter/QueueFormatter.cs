
namespace CsToml.Formatter;

internal sealed class QueueFormatter<T> : CollectionBaseFormatter<Queue<T>, T, Queue<T>>
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
}