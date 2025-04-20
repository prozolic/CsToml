
using System.Collections.Frozen;

namespace CsToml.Formatter;

public sealed class FrozenDictionaryFormatter<TKey, TValue> : DictionaryBaseFormatter<TKey, TValue, FrozenDictionary<TKey, TValue>, Dictionary<TKey, TValue>>
    where TKey : notnull
{
    protected override void AddValue(Dictionary<TKey, TValue> mediator, TKey key, TValue value)
    {
        mediator.Add(key, value);
    }

    protected override FrozenDictionary<TKey, TValue> Complete(Dictionary<TKey, TValue> dictionary)
    {
        return dictionary.ToFrozenDictionary();
    }

    protected override Dictionary<TKey, TValue> CreateMediator(int capacity)
    {
        return new Dictionary<TKey, TValue>(capacity);
    }
}
