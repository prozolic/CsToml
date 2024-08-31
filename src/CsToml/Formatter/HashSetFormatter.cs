
namespace CsToml.Formatter;

internal sealed class HashSetFormatter<T> : CollectionBaseFormatter<HashSet<T>, T, HashSet<T>>
{
    protected override void AddValue(HashSet<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override HashSet<T> Complete(HashSet<T> collection)
    {
        return collection;
    }

    protected override HashSet<T> CreateCollection(int capacity)
    {
        return new HashSet<T>(capacity);
    }
}