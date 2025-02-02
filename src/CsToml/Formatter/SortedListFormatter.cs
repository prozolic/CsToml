
namespace CsToml.Formatter;

public sealed class SortedListFormatter<TKey, TValue> : CollectionBaseFormatter<SortedList<TKey, TValue>, KeyValuePair<TKey, TValue>, SortedList<TKey, TValue>>
    where TKey : notnull
{
    protected override void AddValue(SortedList<TKey, TValue> mediator, KeyValuePair<TKey, TValue> element)
    {
        mediator.Add(element.Key, element.Value);
    }

    protected override SortedList<TKey, TValue> Complete(SortedList<TKey, TValue> collection)
    {
        return collection;
    }

    protected override SortedList<TKey, TValue> CreateCollection(int capacity)
    {
        return new SortedList<TKey, TValue>(capacity);
    }
}
