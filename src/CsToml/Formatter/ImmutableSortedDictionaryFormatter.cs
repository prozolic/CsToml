using System.Collections.Immutable;

namespace CsToml.Formatter;

public sealed class ImmutableSortedDictionaryFormatter<TKey, TValue> : DictionaryBaseFormatter<TKey, TValue, ImmutableSortedDictionary<TKey, TValue>, Dictionary<TKey, TValue>>
    where TKey : notnull
{
    protected override void AddValue(Dictionary<TKey, TValue> mediator, TKey key, TValue value)
    {
        mediator.Add(key, value);
    }

    protected override ImmutableSortedDictionary<TKey, TValue> Complete(Dictionary<TKey, TValue> dictionary)
    {
        return dictionary.ToImmutableSortedDictionary();
    }

    protected override Dictionary<TKey, TValue> CreateMediator(int capacity)
    {
        return new Dictionary<TKey, TValue>(capacity);
    }
}