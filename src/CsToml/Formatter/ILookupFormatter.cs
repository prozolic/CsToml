
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Xml.Linq;

namespace CsToml.Formatter;

public sealed class ILookupFormatter<TKey, TValue> : CollectionBaseFormatter<ILookup<TKey, TValue>, IGrouping<TKey, TValue>, Dictionary<TKey, IGrouping<TKey, TValue>>>
    where TKey : notnull
{
    protected override void AddValue(Dictionary<TKey, IGrouping<TKey, TValue>> mediator, IGrouping<TKey, TValue> element)
    {
        mediator.Add(element.Key, element);
    }

    protected override ILookup<TKey, TValue> Complete(Dictionary<TKey, IGrouping<TKey, TValue>> collection)
    {
        return new Lookup(collection);
    }

    protected override Dictionary<TKey, IGrouping<TKey, TValue>> CreateCollection(int capacity)
    {
        return new(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ILookup<TKey, TValue> target, CsTomlSerializerOptions options)
    {
        IEnumerableSerializer<IGrouping<TKey, TValue>>.Serialize(ref writer, new CollectionContent(target), options);
    }

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, ILookup<TKey, TValue> target, CsTomlSerializerOptions options)
    {
        return IEnumerableSerializer<IGrouping<TKey, TValue>>.TrySerializeTomlArrayHeaderStyle(ref writer, header, new CollectionContent(target), options);
    }

    [DebuggerDisplay("Count = {Count}")]
    private sealed class Lookup(Dictionary<TKey, IGrouping<TKey, TValue>> dictionary) : ILookup<TKey, TValue>
    {
        private readonly Dictionary<TKey, IGrouping<TKey, TValue>> dictionary = dictionary;

        IEnumerable<TValue> ILookup<TKey, TValue>.this[TKey key] => dictionary[key];

        int ILookup<TKey, TValue>.Count => dictionary.Count;

        internal int Count => dictionary.Count;

        bool ILookup<TKey, TValue>.Contains(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        IEnumerator<IGrouping<TKey, TValue>> IEnumerable<IGrouping<TKey, TValue>>.GetEnumerator()
        {
            return dictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.Values.GetEnumerator();
        }
    }
}
