using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace CsToml.Formatter.Resolver;

public sealed class GeneratedFormatterResolver : ITomlValueFormatterResolver
{
    internal static readonly GeneratedFormatterResolver Instance = new GeneratedFormatterResolver();

    private sealed class CacheCheck<T>
    {
        public static bool Registered;
    }

    private sealed class GeneratedFormatterCache<T>
    {
        public static ITomlValueFormatter<T>? Formatter = default!;

        static GeneratedFormatterCache()
        {
            if (CacheCheck<T>.Registered) return;

            if (typeof(T).IsArray)
            {
                CacheCheck<T>.Registered = true;
                Formatter = GetArrayFormatter<T>() as ITomlValueFormatter<T>;
                return;
            }

            if (typeof(T).IsGenericType)
            {
                CacheCheck<T>.Registered = true;
                Formatter = GetGenericTypeFormatter<T>() as ITomlValueFormatter<T>;
                return;
            }
        }
    }

    private static readonly Dictionary<Type, Type> formatterTypeTable = new()
    {
        { typeof(Nullable<>), typeof(NullableFormatter<>) },
        { typeof(Memory<>),  typeof(MemoryFormatter<>) },
        { typeof(ReadOnlyMemory<>),  typeof(ReadOnlyMemoryFormatter<>) },

        { typeof(List<>), typeof(ListFormatter<>) },
        { typeof(Stack<>), typeof(StackFormatter<>) },
        { typeof(HashSet<>), typeof(HashSetFormatter<>) },
        { typeof(SortedSet<>), typeof(SortedSetFormatter<>) },
        { typeof(Queue<>), typeof(QueueFormatter<>) },
        { typeof(LinkedList<>), typeof(LinkedListFormatter<>) },
        { typeof(SortedList<,>), typeof(SortedListFormatter<,>) },
        { typeof(ConcurrentQueue<>), typeof(ConcurrentQueueFormatter<>) },
        { typeof(ConcurrentStack<>), typeof(ConcurrentStackFormatter<>) },
        { typeof(ConcurrentBag<>), typeof(ConcurrentBagFormatter<>) },
        { typeof(ReadOnlyCollection<>), typeof(ReadOnlyCollectionFormatter<>) },
        { typeof(BlockingCollection<>), typeof(BlockingCollectionFormatter<>) },
        { typeof(PriorityQueue<,>), typeof(PriorityQueueFormatter<,>) },
        { typeof(Dictionary<,>), typeof(DictionaryFormatter<,>) },
        { typeof(ReadOnlyDictionary<,>), typeof(ReadOnlyDictionaryFormatter<,>) },
        { typeof(SortedDictionary<,>), typeof(SortedDictionaryFormatter<,>) },
        { typeof(ConcurrentDictionary<,>), typeof(ConcurrentDictionaryFormatter<,>) },

        { typeof(ImmutableArray<>), typeof(ImmutableArrayFormatter<>) },
        { typeof(ImmutableList<>), typeof(ImmutableListFormatter<>) },
        { typeof(ImmutableStack<>), typeof(ImmutableStackFormatter<>) },
        { typeof(ImmutableQueue<>), typeof(ImmutableQueueFormatter<>)},
        { typeof(ImmutableHashSet<>), typeof(ImmutableHashSetFormatter<>) },
        { typeof(ImmutableSortedSet<>), typeof(ImmutableSortedSetFormatter<>) },
        { typeof(ImmutableDictionary<,>), typeof(ImmutableDictionaryFormatter<,>) },
        { typeof(ImmutableSortedDictionary<,>), typeof(ImmutableSortedDictionaryFormatter<,>) },

        { typeof(IEnumerable<>), typeof(IEnumerableFormatter<>) },
        { typeof(ICollection<>),  typeof(ICollectionFormatter<>) },
        { typeof(IReadOnlyCollection<>),  typeof(IReadOnlyCollectionFormatter<>) },
        { typeof(IList<>),  typeof(IListFormatter<>) },
        { typeof(IReadOnlyList<>),  typeof(IReadOnlyListFormatter<>) },
        { typeof(ISet<>),  typeof(ISetFormatter<>) },
        { typeof(IReadOnlySet<>),  typeof(IReadOnlySetFormatter<>) },
        { typeof(IDictionary<,>),typeof(IDictionaryFormatter<,>) },
        { typeof(IReadOnlyDictionary<,>),typeof(IReadOnlyDictionaryFormatter<,>) },

        { typeof(KeyValuePair<,>),  typeof(KeyValuePairFormatter<,>) },
        { typeof(Tuple<>),  typeof(TupleFormatter<>) },
        { typeof(Tuple<,>),  typeof(TupleFormatter<,>) },
        { typeof(Tuple<,,>),  typeof(TupleFormatter<,,>) },
        { typeof(Tuple<,,,>),  typeof(TupleFormatter<,,,>) },
        { typeof(Tuple<,,,,>),  typeof(TupleFormatter<,,,,>) },
        { typeof(Tuple<,,,,,>),  typeof(TupleFormatter<,,,,,>) },
        { typeof(Tuple<,,,,,,>),  typeof(TupleFormatter<,,,,,,>) },
        { typeof(Tuple<,,,,,,,>),  typeof(TupleFormatter<,,,,,,,>) },
        { typeof(ValueTuple<>),  typeof(ValueTupleFormatter<>) },
        { typeof(ValueTuple<,>),  typeof(ValueTupleFormatter<,>) },
        { typeof(ValueTuple<,,>),  typeof(ValueTupleFormatter<,,>) },
        { typeof(ValueTuple<,,,>),  typeof(ValueTupleFormatter<,,,>) },
        { typeof(ValueTuple<,,,,>),  typeof(ValueTupleFormatter<,,,,>) },
        { typeof(ValueTuple<,,,,,>),  typeof(ValueTupleFormatter<,,,,,>) },
        { typeof(ValueTuple<,,,,,,>),  typeof(ValueTupleFormatter<,,,,,,>) },
        { typeof(ValueTuple<,,,,,,,>),  typeof(ValueTupleFormatter<,,,,,,,>) },

#if NET9_0_OR_GREATER
        { typeof(OrderedDictionary<,>), typeof(OrderedDictionaryFormatter<,>) },
        { typeof(ReadOnlySet<>), typeof(ReadOnlySetFormatter<>) },
#endif
    };

    public ITomlValueFormatter<T>? GetFormatter<T>()
    {
        var genericFormatter = GeneratedFormatterCache<T>.Formatter;
        if (genericFormatter != null)
            return genericFormatter;

        return null;
    }

    private static object? GetArrayFormatter<T>()
    {
        var arrayFormatterType = typeof(ArrayFormatter<>).MakeGenericType(typeof(T).GetElementType()!);
        return Activator.CreateInstance(arrayFormatterType);
    }

    private static object? GetGenericTypeFormatter<T>()
    {
        var type = typeof(T);
        var genericTypeDefinition = type.GetGenericTypeDefinition();

        if (formatterTypeTable.TryGetValue(genericTypeDefinition, out var formatterType))
        {
            var genericType = formatterType.MakeGenericType(type.GetGenericArguments());
            if (genericType != null)
            {
                return Activator.CreateInstance(genericType);
            }
        }
        return null;
    }

    public static bool IsRegistered<T>()
        => CacheCheck<T>.Registered;

    public static void Register<T>(ITomlValueFormatter<T> fomatter)
    {
        if (CacheCheck<T>.Registered) return;

        CacheCheck<T>.Registered = true;
        GeneratedFormatterCache<T>.Formatter = fomatter;
    }
}
