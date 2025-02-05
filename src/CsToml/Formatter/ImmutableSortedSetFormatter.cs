using System.Collections.Immutable;

namespace CsToml.Formatter;

public sealed class ImmutableSortedSetFormatter<T> : CollectionBaseFormatter<ImmutableSortedSet<T>, T, SortedSet<T>>
{
    protected override void AddValue(SortedSet<T> mediator, T element)
    {
        mediator.Add(element);
    }
    protected override ImmutableSortedSet<T> Complete(SortedSet<T> collection)
    {
        return collection.ToImmutableSortedSet();
    }
    protected override SortedSet<T> CreateCollection(int capacity)
    {
        return new SortedSet<T>();
    }
}
