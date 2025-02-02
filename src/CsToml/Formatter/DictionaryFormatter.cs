
namespace CsToml.Formatter;

public sealed class DictionaryFormatter<TKey, TValue> : DictionaryBaseFormatter<TKey, TValue, Dictionary<TKey, TValue>, Dictionary<TKey, TValue>>
    where TKey : notnull
{
    protected override void AddValue(Dictionary<TKey, TValue> mediator, TKey key, TValue value)
    {
        mediator.Add(key, value);
    }

    protected override Dictionary<TKey, TValue> Complete(Dictionary<TKey, TValue> dictionary)
    {
        return dictionary;
    }

    protected override Dictionary<TKey, TValue> CreateMediator(int capacity)
    {
        return new Dictionary<TKey, TValue>(capacity);
    }
}
