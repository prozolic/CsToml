using System.Collections.Immutable;

namespace CsToml.Formatter;

public sealed class ImmutableListFormatter<T> : CollectionBaseFormatter<ImmutableList<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override ImmutableList<T> Complete(List<T> collection)
    {
        return collection.ToImmutableList();
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }
}
