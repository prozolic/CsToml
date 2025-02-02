using System.Collections.ObjectModel;

namespace CsToml.Formatter;

public sealed class ReadOnlyCollectionFormatter<T> : CollectionBaseFormatter<ReadOnlyCollection<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override ReadOnlyCollection<T> Complete(List<T> collection)
    {
        return new ReadOnlyCollection<T>(collection);
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }
}