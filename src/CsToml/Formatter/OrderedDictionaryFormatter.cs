#if NET9_0_OR_GREATER

namespace CsToml.Formatter;

public sealed class OrderedDictionaryFormatter<TKey, TValue> : DictionaryBaseFormatter<TKey, TValue, OrderedDictionary<TKey, TValue>, OrderedDictionary<TKey, TValue>>
    where TKey : notnull
{
    protected override void AddValue(OrderedDictionary<TKey, TValue> mediator, TKey key, TValue value)
    {
        mediator.Add(key, value);
    }

    protected override OrderedDictionary<TKey, TValue> Complete(OrderedDictionary<TKey, TValue> dictionary)
    {
        return dictionary;
    }

    protected override OrderedDictionary<TKey, TValue> CreateMediator(int capacity)
    {
        return new OrderedDictionary<TKey, TValue>(capacity);
    }
}

#endif