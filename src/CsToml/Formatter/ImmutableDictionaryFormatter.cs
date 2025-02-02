using System.Collections.Immutable;

namespace CsToml.Formatter;

public sealed class ImmutableDictionaryFormatter<TKey, TValue> : DictionaryBaseFormatter<TKey, TValue, ImmutableDictionary<TKey, TValue>, Dictionary<TKey, TValue>>
    where TKey : notnull
{
    protected override void AddValue(Dictionary<TKey, TValue> mediator, TKey key, TValue value)
    {
        mediator.Add(key, value);
    }

    protected override ImmutableDictionary<TKey, TValue> Complete(Dictionary<TKey, TValue> dictionary)
    {
        return dictionary.ToImmutableDictionary();
    }

    protected override Dictionary<TKey, TValue> CreateMediator(int capacity)
    {
        return new Dictionary<TKey, TValue>(capacity);
    }
}
