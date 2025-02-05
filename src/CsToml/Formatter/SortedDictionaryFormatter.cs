
namespace CsToml.Formatter;

public sealed class SortedDictionaryFormatter<TKey, TValue> : DictionaryBaseFormatter<TKey, TValue, SortedDictionary<TKey, TValue>, Dictionary<TKey, TValue>>
    where TKey : notnull
{
    protected override void AddValue(Dictionary<TKey, TValue> mediator, TKey key, TValue value)
    {
        mediator.Add(key, value);
    }

    protected override SortedDictionary<TKey, TValue> Complete(Dictionary<TKey, TValue> dictionary)
    {
        return new SortedDictionary<TKey, TValue>(dictionary);
    }

    protected override Dictionary<TKey, TValue> CreateMediator(int capacity)
    {
        return new Dictionary<TKey, TValue>(capacity);
    }
}

