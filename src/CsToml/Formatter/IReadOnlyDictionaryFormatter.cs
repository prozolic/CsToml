﻿
namespace CsToml.Formatter;

public sealed class IReadOnlyDictionaryFormatter<TKey, TValue> : DictionaryBaseFormatter<TKey, TValue, IReadOnlyDictionary<TKey, TValue>, Dictionary<TKey, TValue>>
    where TKey : notnull
{
    protected override void AddValue(Dictionary<TKey, TValue> mediator, TKey key, TValue value)
    {
        mediator.Add(key, value);
    }

    protected override IReadOnlyDictionary<TKey, TValue> Complete(Dictionary<TKey, TValue> dictionary)
    {
        return dictionary.AsReadOnly();
    }

    protected override Dictionary<TKey, TValue> CreateMediator(int capacity)
    {
        return new Dictionary<TKey, TValue>(capacity);
    }
}


