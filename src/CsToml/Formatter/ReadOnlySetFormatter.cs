#if NET9_0_OR_GREATER

using System.Collections.ObjectModel;

namespace CsToml.Formatter;

public class ReadOnlySetFormatter<T> : CollectionBaseFormatter<ReadOnlySet<T>, T, HashSet<T>>
{
    protected override void AddValue(HashSet<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override ReadOnlySet<T> Complete(HashSet<T> collection)
    {
        return new ReadOnlySet<T>(collection);
    }

    protected override HashSet<T> CreateCollection(int capacity)
    {
        return new HashSet<T>(capacity);
    }
}

#endif