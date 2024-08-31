﻿
namespace CsToml.Formatter;

internal sealed class ICollectionFormatter<T> : CollectionBaseFormatter<ICollection<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override ICollection<T> Complete(List<T> collection)
    {
        return collection;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }
}
