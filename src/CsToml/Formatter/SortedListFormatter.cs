
namespace CsToml.Formatter;

public sealed class SortedListFormatter<TKey, TValue> : DictionaryBaseFormatter<TKey, TValue, SortedList<TKey, TValue>, SortedList<TKey, TValue>>
    where TKey : notnull
{
    protected override void AddValue(SortedList<TKey, TValue> mediator, TKey key, TValue value)
    {
        mediator.Add(key, value);
    }

    protected override SortedList<TKey, TValue> Complete(SortedList<TKey, TValue> dictionary)
    {
        return dictionary;
    }

    protected override SortedList<TKey, TValue> CreateMediator(int capacity)
    {
        return new SortedList<TKey, TValue>(capacity);
    }
}
