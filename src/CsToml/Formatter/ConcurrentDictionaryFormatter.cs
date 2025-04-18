using System.Collections.Concurrent;

namespace CsToml.Formatter;

public class ConcurrentDictionaryFormatter<TKey, TValue> : DictionaryBaseFormatter<TKey, TValue, ConcurrentDictionary<TKey, TValue>, ConcurrentDictionary<TKey, TValue>>
    where TKey : notnull
{
    protected override void AddValue(ConcurrentDictionary<TKey, TValue> mediator, TKey key, TValue value)
    {
        mediator.TryAdd(key, value);
    }

    protected override ConcurrentDictionary<TKey, TValue> Complete(ConcurrentDictionary<TKey, TValue> dictionary)
    {
        return dictionary;
    }

    protected override ConcurrentDictionary<TKey, TValue> CreateMediator(int capacity)
    {
        // Set the number of processors to concurrencyLevel.
        return new ConcurrentDictionary<TKey, TValue>(Environment.ProcessorCount, capacity);
    }
}
