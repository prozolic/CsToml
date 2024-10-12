using System.Collections.Concurrent;

namespace CsToml.Formatter;

internal class ConcurrentDictionaryFormatter<TKey, TValue> : DictionaryBaseFormatter<TKey, TValue, ConcurrentDictionary<TKey, TValue>, Dictionary<TKey, TValue>>
    where TKey : notnull
{
    protected override void AddValue(Dictionary<TKey, TValue> mediator, TKey key, TValue value)
    {
        mediator.Add(key, value);
    }

    protected override ConcurrentDictionary<TKey, TValue> Complete(Dictionary<TKey, TValue> dictionary)
    {
        return new ConcurrentDictionary<TKey, TValue>(dictionary);;
    }

    protected override Dictionary<TKey, TValue> CreateMediator(int capacity)
    {
        return new Dictionary<TKey, TValue>(capacity);
    }
}
