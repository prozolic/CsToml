﻿
namespace CsToml.Generator;

internal static class FormatterTypeMetaData
{
    private static readonly Dictionary<string, string> builtInGeneratedFormatterTypes = new()
    {
        { "global::System.Collections.Generic.List<>", "ListFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.Stack<>", "StackFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.HashSet<>", "HashSetFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.SortedSet<>", "SortedSetFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.Queue<>", "QueueFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.LinkedList<>", "LinkedListFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.SortedList<,>", "SortedListFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.Dictionary<,>", "DictionaryFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.PriorityQueue<,>", "PriorityQueueFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.SortedDictionary<,>", "SortedDictionaryFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Concurrent.ConcurrentBag<>", "ConcurrentBagFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Concurrent.ConcurrentQueue<>", "ConcurrentQueueFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Concurrent.ConcurrentStack<>", "ConcurrentStackFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Concurrent.BlockingCollection<>", "BlockingCollectionFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Concurrent.ConcurrentDictionary<,>", "ConcurrentDictionaryFormatter<TYPEPARAMETER>" },
        { "System.Collections.ObjectModel.ReadOnlyCollection<>", "ReadOnlyCollectionFormatter" },
        { "System.Collections.ObjectModel.ReadOnlyDictionary<,>", "ReadOnlyDictionaryFormatter" },

        { "System.Collections.Immutable.ImmutableArray<>", "ImmutableArrayFormatter" },
        { "System.Collections.Immutable.ImmutableList<>", "ImmutableListFormatter" },
        { "System.Collections.Immutable.ImmutableStack<>", "ImmutableStackFormatter" },
        { "System.Collections.Immutable.ImmutableQueue<>", "ImmutableQueueFormatter" },
        { "System.Collections.Immutable.ImmutableHashSet<>", "ImmutableHashSetFormatter" },
        { "System.Collections.Immutable.ImmutableSortedSet<>", "ImmutableSortedSetFormatter" },
        { "System.Collections.Immutable.ImmutableDictionary<,>", "ImmutableDictionaryFormatter" },
        { "System.Collections.Immutable.ImmutableSortedDictionary<,>", "ImmutableSortedDictionaryFormatter" },

        { "global::System.Collections.Generic.IEnumerable<>", "IEnumerableFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.ICollection<>", "ICollectionFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.IReadOnlyCollection<>", "IReadOnlyCollectionFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.IList<>", "IListFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.IReadOnlyList<>", "IReadOnlyListFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.ISet<>", "ISetFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.IReadOnlySet<>", "IReadOnlySetFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.IDictionary<,>", "IDictionaryFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.IReadOnlyDictionary<,>", "IReadOnlyDictionaryFormatter<TYPEPARAMETER>" },

        { "global::System.Collections.Generic.KeyValuePair<,>", "KeyValuePairFormatter<TYPEPARAMETER>" },
        { "global::System.Tuple<,>", "TupleFormatter<TYPEPARAMETER>" },
        { "global::System.Tuple<,,>", "TupleFormatter<TYPEPARAMETER>" },
        { "global::System.Tuple<,,,>", "TupleFormatter<TYPEPARAMETER>" },
        { "global::System.Tuple<,,,,>", "TupleFormatter<TYPEPARAMETER>" },
        { "global::System.Tuple<,,,,,>", "TupleFormatter<TYPEPARAMETER>" },
        { "global::System.Tuple<,,,,,,>", "TupleFormatter<TYPEPARAMETER>" },
        { "global::System.Tuple<,,,,,,,>", "TupleFormatter<TYPEPARAMETER>" },
        { "global::System.ValueTuple<,>", "ValueTupleFormatter<TYPEPARAMETER>" },
        { "global::System.ValueTuple<,,>", "ValueTupleFormatter<TYPEPARAMETER>" },
        { "global::System.ValueTuple<,,,>", "ValueTupleFormatter<TYPEPARAMETER>" },
        { "global::System.ValueTuple<,,,,>", "ValueTupleFormatter<TYPEPARAMETER>" },
        { "global::System.ValueTuple<,,,,,>", "ValueTupleFormatter<TYPEPARAMETER>" },
        { "global::System.ValueTuple<,,,,,,>", "ValueTupleFormatter<TYPEPARAMETER>" },
        { "global::System.ValueTuple<,,,,,,,>", "ValueTupleFormatter<TYPEPARAMETER>" },

        { "global::System.Nullable<,>", "NullableStructFormatter<TYPEPARAMETER>" },
    };

    private static readonly HashSet<string> builtInFormatterTypeMap = new()
    {
        "global::System.Half",
        "global::System.Int128",
        "global::System.UInt128" ,
        "global::System.Numerics.BigInteger",
        "global::System.TimeSpan",
        "global::System.Uri",
        "global::System.Guid", 
        "global::System.Version",
        "global::System.Text.StringBuilder",
        "global::System.Collections.BitArray",
        "global::System.Type",
    };

    public static bool TryGetGeneratedFormatterType(string formatterType, out string formatter)
    {
        return builtInGeneratedFormatterTypes.TryGetValue(formatterType, out formatter);
    }

    public static bool ContainsBuiltInFormatterType(string typeMetaName)
        => builtInFormatterTypeMap.Contains(typeMetaName);
}