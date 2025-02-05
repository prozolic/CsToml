using System.Collections.ObjectModel;

namespace CsToml.Formatter;

public sealed class ReadOnlyDictionaryFormatter<TKey, TValue> : DictionaryBaseFormatter<TKey, TValue, ReadOnlyDictionary<TKey, TValue>, Dictionary<TKey, TValue>>
    where TKey : notnull
{
    protected override void AddValue(Dictionary<TKey, TValue> mediator, TKey key, TValue value)
    {
        mediator.Add(key, value);
    }

    protected override ReadOnlyDictionary<TKey, TValue> Complete(Dictionary<TKey, TValue> dicitonary)
    {
        return dicitonary.AsReadOnly();
    }

    protected override Dictionary<TKey, TValue> CreateMediator(int capacity)
    {
        return new Dictionary<TKey, TValue>(capacity);
    }
}

