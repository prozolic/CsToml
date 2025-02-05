using System.Collections.Concurrent;

namespace CsToml.Formatter;

public sealed class BlockingCollectionFormatter<T> : CollectionBaseFormatter<BlockingCollection<T>, T, BlockingCollection<T>>
{
    protected override void AddValue(BlockingCollection<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override BlockingCollection<T> Complete(BlockingCollection<T> collection)
    {
        return collection;
    }

    protected override BlockingCollection<T> CreateCollection(int capacity)
    {
        return new BlockingCollection<T>(capacity);
    }
}
