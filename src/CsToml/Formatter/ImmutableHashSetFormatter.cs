using System.Collections.Immutable;

namespace CsToml.Formatter;

internal sealed class ImmutableHashSetFormatter<T> : CollectionBaseFormatter<ImmutableHashSet<T>, T, HashSet<T>>
{
    protected override void AddValue(HashSet<T> mediator, T element)
    {
        mediator.Add(element);
    }
    protected override ImmutableHashSet<T> Complete(HashSet<T> collection)
    {
        return collection.ToImmutableHashSet();
    }
    protected override HashSet<T> CreateCollection(int capacity)
    {
        return new HashSet<T>(capacity);
    }
}