using System.Collections.Immutable;

namespace CsToml.Formatter;

public sealed class IImmutableSetFormatter<T> : CollectionBaseFormatter<IImmutableSet<T>, T, HashSet<T>>
{
    protected override void AddValue(HashSet<T> mediator, T element)
    {
        mediator.Add(element);
    }
    protected override IImmutableSet<T> Complete(HashSet<T> collection)
    {
        return collection.ToImmutableHashSet();
    }
    protected override HashSet<T> CreateCollection(int capacity)
    {
        return new HashSet<T>(capacity);
    }

}
