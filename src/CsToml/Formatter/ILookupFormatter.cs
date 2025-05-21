
using System.Collections;
using System.Diagnostics;

namespace CsToml.Formatter;

public sealed class ILookupFormatter<TKey, TValue> : CollectionBaseFormatter<ILookup<TKey, TValue>, IGrouping<TKey, TValue>, Dictionary<TKey, IGrouping<TKey, TValue>>>
    where TKey : notnull
{
    protected override void AddValue(Dictionary<TKey, IGrouping<TKey, TValue>> mediator, IGrouping<TKey, TValue> element)
    {
        mediator.Add(element.Key, element);
    }

    protected override ILookup<TKey, TValue> Complete(Dictionary<TKey, IGrouping<TKey, TValue>> collection)
    {
        return new Lookup(collection);
    }

    protected override Dictionary<TKey, IGrouping<TKey, TValue>> CreateCollection(int capacity)
    {
        return new(capacity);
    }

    [DebuggerDisplay("Count = {Count}")]
    private sealed class Lookup(Dictionary<TKey, IGrouping<TKey, TValue>> dictionary) : ILookup<TKey, TValue>
    {
        private readonly Dictionary<TKey, IGrouping<TKey, TValue>> dictionary = dictionary;

        IEnumerable<TValue> ILookup<TKey, TValue>.this[TKey key] => dictionary[key];

        int ILookup<TKey, TValue>.Count => dictionary.Count;

        internal int Count => dictionary.Count;

        bool ILookup<TKey, TValue>.Contains(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        IEnumerator<IGrouping<TKey, TValue>> IEnumerable<IGrouping<TKey, TValue>>.GetEnumerator()
        {
            return dictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.Values.GetEnumerator();
        }
    }
}
