
namespace CsToml.Formatter;

public sealed class SortedSetFormatter<T> : CollectionBaseFormatter<SortedSet<T>, T, SortedSet<T>>
{
    protected override void AddValue(SortedSet<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override SortedSet<T> Complete(SortedSet<T> collection)
    {
        return collection;
    }

    protected override SortedSet<T> CreateCollection(int capacity)
    {
        return new SortedSet<T>();
    }
}
