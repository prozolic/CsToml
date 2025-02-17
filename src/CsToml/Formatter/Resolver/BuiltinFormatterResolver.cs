
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace CsToml.Formatter.Resolver;

internal sealed class BuiltinFormatterResolver : ITomlValueFormatterResolver
{
    internal static readonly BuiltinFormatterResolver Instance = new BuiltinFormatterResolver();

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

        { typeof(IImmutableList<>), typeof(IImmutableListFormatter<>) },
        { typeof(IImmutableStack<>), typeof(IImmutableStackFormatter<>) },
        { typeof(IImmutableQueue<>), typeof(IImmutableQueueFormatter<>) },
        { typeof(IImmutableSet<>), typeof(IImmutableSetFormatter<>) },
        { typeof(IImmutableDictionary<,>), typeof(IImmutableDictionaryFormatter<,>) },

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

    private sealed class CacheCheck<T>
    {
        public static bool Registered;
    }

    private sealed class DefaultFormatterCache<T>
    {
        public static ITomlValueFormatter<T>? Formatter;
    }

    private sealed class GeneratedFormatterCache<T>
    {
        public static ITomlValueFormatter<T>? Formatter = default!;

        static GeneratedFormatterCache()
        {
            if (CacheCheck<T>.Registered) return;

            if (typeof(T).IsEnum)
            {
                CacheCheck<T>.Registered = true;
                Formatter = GetEnumFormatter<T>() as ITomlValueFormatter<T>;
                return;
            }

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

    static BuiltinFormatterResolver()
    {
        DefaultFormatterCache<bool>.Formatter = BooleanFormatter.Instance;
        DefaultFormatterCache<bool?>.Formatter = NullableBooleanFormatter.Instance;
        DefaultFormatterCache<byte>.Formatter = ByteFormatter.Instance;
        DefaultFormatterCache<byte?>.Formatter = NullableByteFormatter.Instance;
        DefaultFormatterCache<sbyte>.Formatter = SByteFormatter.Instance;
        DefaultFormatterCache<sbyte?>.Formatter = NullableSByteFormatter.Instance;
        DefaultFormatterCache<char>.Formatter = CharFormatter.Instance;
        DefaultFormatterCache<char?>.Formatter = NullableCharFormatter.Instance;
        DefaultFormatterCache<float>.Formatter = FloatFormatter.Instance;
        DefaultFormatterCache<float?>.Formatter = NullableFloatFormatter.Instance;
        DefaultFormatterCache<double>.Formatter = DoubleFormatter.Instance;
        DefaultFormatterCache<double?>.Formatter = NullableDoubleFormatter.Instance;
        DefaultFormatterCache<short>.Formatter = Int16Formatter.Instance;
        DefaultFormatterCache<short?>.Formatter = NullableInt16Formatter.Instance;
        DefaultFormatterCache<ushort>.Formatter = UInt16Formatter.Instance;
        DefaultFormatterCache<ushort?>.Formatter = NullableUInt16Formatter.Instance;
        DefaultFormatterCache<int>.Formatter = Int32Formatter.Instance;
        DefaultFormatterCache<int?>.Formatter = NullableInt32Formatter.Instance;
        DefaultFormatterCache<uint>.Formatter = UInt32Formatter.Instance;
        DefaultFormatterCache<uint?>.Formatter = NullableUInt32Formatter.Instance;
        DefaultFormatterCache<long>.Formatter = Int64Formatter.Instance;
        DefaultFormatterCache<long?>.Formatter = NullableInt64Formatter.Instance;
        DefaultFormatterCache<ulong>.Formatter = UInt64Formatter.Instance;
        DefaultFormatterCache<ulong?>.Formatter = NullableUInt64Formatter.Instance;

        DefaultFormatterCache<DateTimeOffset>.Formatter = DateTimeOffsetFormatter.Instance;
        DefaultFormatterCache<DateTimeOffset?>.Formatter = NullableDateTimeOffsetFormatter.Instance;
        DefaultFormatterCache<DateTime>.Formatter = DateTimeFormatter.Instance;
        DefaultFormatterCache<DateTime?>.Formatter = NullableDateTimeFormatter.Instance;
        DefaultFormatterCache<DateOnly>.Formatter = DateOnlyFormatter.Instance;
        DefaultFormatterCache<DateOnly?>.Formatter = NullableDateOnlyFormatter.Instance;
        DefaultFormatterCache<TimeOnly>.Formatter = TimeOnlyFormatter.Instance;
        DefaultFormatterCache<TimeOnly?>.Formatter = NullableTimeOnlyFormatter.Instance;
        DefaultFormatterCache<string?>.Formatter = NullableStringFormatter.Instance;
        DefaultFormatterCache<decimal>.Formatter = DecimalFormatter.Instance;
        DefaultFormatterCache<decimal?>.Formatter = NullableDecimalFormatter.Instance;
        DefaultFormatterCache<Half>.Formatter = HalfFormatter.Instance;
        DefaultFormatterCache<Half?>.Formatter = NullableHalfFormatter.Instance;
        DefaultFormatterCache<Int128>.Formatter = Int128Formatter.Instance;
        DefaultFormatterCache<Int128?>.Formatter = NullableInt128Formatter.Instance;
        DefaultFormatterCache<UInt128>.Formatter = UInt128Formatter.Instance;
        DefaultFormatterCache<UInt128?>.Formatter = NullableUInt128Formatter.Instance;
        DefaultFormatterCache<BigInteger>.Formatter = BigIntegerFormatter.Instance;
        DefaultFormatterCache<BigInteger?>.Formatter = NullableBigIntegerFormatter.Instance;
        DefaultFormatterCache<TimeSpan>.Formatter = TimeSpanFormatter.Instance;
        DefaultFormatterCache<TimeSpan?>.Formatter = NullableTimeSpanFormatter.Instance;

        DefaultFormatterCache<Uri?>.Formatter = UriFormatter.Instance;
        DefaultFormatterCache<Guid>.Formatter = GuidFormatter.Instance;
        DefaultFormatterCache<Guid?>.Formatter = NullableGuidFormatter.Instance;
        DefaultFormatterCache<Version?>.Formatter = VersionFormatter.Instance;
        DefaultFormatterCache<StringBuilder?>.Formatter = StringBuilderFormatter.Instance;
        DefaultFormatterCache<BitArray?>.Formatter = BitArrayFormatter.Instance;
        DefaultFormatterCache<Type?>.Formatter = TypeFormatter.Instance;

        DefaultFormatterCache<bool[]?>.Formatter = new ArrayFormatter<bool>();
        DefaultFormatterCache<byte[]?>.Formatter = new ArrayFormatter<byte>();
        DefaultFormatterCache<sbyte[]?>.Formatter = new ArrayFormatter<sbyte>();
        DefaultFormatterCache<short[]?>.Formatter = new ArrayFormatter<short>();
        DefaultFormatterCache<ushort[]?>.Formatter = new ArrayFormatter<ushort>();
        DefaultFormatterCache<int[]?>.Formatter = new ArrayFormatter<int>();
        DefaultFormatterCache<uint[]?>.Formatter = new ArrayFormatter<uint>();
        DefaultFormatterCache<long[]?>.Formatter = new ArrayFormatter<long>();
        DefaultFormatterCache<ulong[]?>.Formatter = new ArrayFormatter<ulong>();
        DefaultFormatterCache<float[]?>.Formatter = new ArrayFormatter<float>();
        DefaultFormatterCache<double[]?>.Formatter = new ArrayFormatter<double>();
        DefaultFormatterCache<decimal[]?>.Formatter = new ArrayFormatter<decimal>();
        DefaultFormatterCache<DateTimeOffset[]?>.Formatter = new ArrayFormatter<DateTimeOffset>();
        DefaultFormatterCache<DateTime[]?>.Formatter = new ArrayFormatter<DateTime>();
        DefaultFormatterCache<DateOnly[]?>.Formatter = new ArrayFormatter<DateOnly>();
        DefaultFormatterCache<TimeOnly[]?>.Formatter = new ArrayFormatter<TimeOnly>();
        DefaultFormatterCache<char[]?>.Formatter = new ArrayFormatter<char>();
        DefaultFormatterCache<string[]?>.Formatter = new ArrayFormatter<string>();

        DefaultFormatterCache<List<bool>?>.Formatter = new ListFormatter<bool>();
        DefaultFormatterCache<List<byte>?>.Formatter = new ListFormatter<byte>();
        DefaultFormatterCache<List<sbyte>?>.Formatter = new ListFormatter<sbyte>();
        DefaultFormatterCache<List<short>?>.Formatter = new ListFormatter<short>();
        DefaultFormatterCache<List<ushort>?>.Formatter = new ListFormatter<ushort>();
        DefaultFormatterCache<List<int>?>.Formatter = new ListFormatter<int>();
        DefaultFormatterCache<List<uint>?>.Formatter = new ListFormatter<uint>();
        DefaultFormatterCache<List<long>?>.Formatter = new ListFormatter<long>();
        DefaultFormatterCache<List<ulong>?>.Formatter = new ListFormatter<ulong>();
        DefaultFormatterCache<List<float>?>.Formatter = new ListFormatter<float>();
        DefaultFormatterCache<List<double>?>.Formatter = new ListFormatter<double>();
        DefaultFormatterCache<List<decimal>?>.Formatter = new ListFormatter<decimal>();
        DefaultFormatterCache<List<DateTimeOffset>?>.Formatter = new ListFormatter<DateTimeOffset>();
        DefaultFormatterCache<List<DateTime>?>.Formatter = new ListFormatter<DateTime>();
        DefaultFormatterCache<List<DateOnly>?>.Formatter = new ListFormatter<DateOnly>();
        DefaultFormatterCache<List<TimeOnly>?>.Formatter = new ListFormatter<TimeOnly>();
        DefaultFormatterCache<List<char>?>.Formatter = new ListFormatter<char>();
        DefaultFormatterCache<List<string>?>.Formatter = new ListFormatter<string>();

        DefaultFormatterCache<Hashtable>.Formatter = HashtableFormatter.Instance;
        DefaultFormatterCache<ArrayList?>.Formatter = ArrayListFormatter.Instance;

        DefaultFormatterCache<IDictionary<string, object?>>.Formatter = new IDictionaryFormatter<string, object?>();
        DefaultFormatterCache<IDictionary<object, object?>>.Formatter = new IDictionaryFormatter<object, object?>();

        CacheCheck<bool>.Registered = true;
        CacheCheck<bool?>.Registered = true;
        CacheCheck<byte>.Registered = true;
        CacheCheck<byte?>.Registered = true;
        CacheCheck<sbyte>.Registered = true;
        CacheCheck<sbyte?>.Registered = true;
        CacheCheck<char>.Registered = true;
        CacheCheck<char?>.Registered = true;
        CacheCheck<float>.Registered = true;
        CacheCheck<float?>.Registered = true;
        CacheCheck<double>.Registered = true;
        CacheCheck<double?>.Registered = true;
        CacheCheck<short>.Registered = true;
        CacheCheck<short?>.Registered = true;
        CacheCheck<ushort>.Registered = true;
        CacheCheck<ushort?>.Registered = true;
        CacheCheck<int>.Registered = true;
        CacheCheck<int?>.Registered = true;
        CacheCheck<uint>.Registered = true;
        CacheCheck<uint?>.Registered = true;
        CacheCheck<long>.Registered = true;
        CacheCheck<long?>.Registered = true;
        CacheCheck<ulong>.Registered = true;
        CacheCheck<ulong?>.Registered = true;

        CacheCheck<DateTimeOffset>.Registered = true;
        CacheCheck<DateTimeOffset?>.Registered = true;
        CacheCheck<DateTime>.Registered = true;
        CacheCheck<DateTime?>.Registered = true;
        CacheCheck<DateOnly>.Registered = true;
        CacheCheck<DateOnly?>.Registered = true;
        CacheCheck<TimeOnly>.Registered = true;
        CacheCheck<TimeOnly?>.Registered = true;
        CacheCheck<string?>.Registered = true;
        CacheCheck<decimal>.Registered = true;
        CacheCheck<decimal?>.Registered = true;
        CacheCheck<Half>.Registered = true;
        CacheCheck<Half?>.Registered = true;
        CacheCheck<Int128>.Registered = true;
        CacheCheck<Int128?>.Registered = true;
        CacheCheck<UInt128>.Registered = true;
        CacheCheck<UInt128?>.Registered = true;
        CacheCheck<BigInteger>.Registered = true;
        CacheCheck<BigInteger?>.Registered = true;
        CacheCheck<TimeSpan>.Registered = true;
        CacheCheck<TimeSpan?>.Registered = true;

        CacheCheck<Uri?>.Registered = true;
        CacheCheck<Guid>.Registered = true;
        CacheCheck<Guid?>.Registered = true;
        CacheCheck<Version?>.Registered = true;
        CacheCheck<StringBuilder?>.Registered = true;
        CacheCheck<BitArray?>.Registered = true;
        CacheCheck<Type?>.Registered = true;

        CacheCheck<bool[]?>.Registered = true;
        CacheCheck<byte[]?>.Registered = true;
        CacheCheck<sbyte[]?>.Registered = true;
        CacheCheck<short[]?>.Registered = true;
        CacheCheck<ushort[]?>.Registered = true;
        CacheCheck<int[]?>.Registered = true;
        CacheCheck<uint[]?>.Registered = true;
        CacheCheck<long[]?>.Registered = true;
        CacheCheck<ulong[]?>.Registered = true;
        CacheCheck<float[]?>.Registered = true;
        CacheCheck<double[]?>.Registered = true;
        CacheCheck<decimal[]?>.Registered = true;
        CacheCheck<DateTimeOffset[]?>.Registered = true;
        CacheCheck<DateTime[]?>.Registered = true;
        CacheCheck<DateOnly[]?>.Registered = true;
        CacheCheck<TimeOnly[]?>.Registered = true;
        CacheCheck<char[]?>.Registered = true;
        CacheCheck<string[]?>.Registered = true;

        CacheCheck<List<bool>?>.Registered = true;
        CacheCheck<List<byte>?>.Registered = true;
        CacheCheck<List<sbyte>?>.Registered = true;
        CacheCheck<List<short>?>.Registered = true;
        CacheCheck<List<ushort>?>.Registered = true;
        CacheCheck<List<int>?>.Registered = true;
        CacheCheck<List<uint>?>.Registered = true;
        CacheCheck<List<long>?>.Registered = true;
        CacheCheck<List<ulong>?>.Registered = true;
        CacheCheck<List<float>?>.Registered = true;
        CacheCheck<List<double>?>.Registered = true;
        CacheCheck<List<decimal>?>.Registered = true;
        CacheCheck<List<DateTimeOffset>?>.Registered = true;
        CacheCheck<List<DateTime>?>.Registered = true;
        CacheCheck<List<DateOnly>?>.Registered = true;
        CacheCheck<List<TimeOnly>?>.Registered = true;
        CacheCheck<List<char>?>.Registered = true;
        CacheCheck<List<string>?>.Registered = true;

        CacheCheck<Hashtable>.Registered = true;
        CacheCheck<ArrayList?>.Registered = true;

        CacheCheck<IDictionary<string, object?>>.Registered = true;
        CacheCheck<IDictionary<object, object?>>.Registered = true;
    }

    private static object? GetEnumFormatter<T>()
    {
        var enumFormatterType = typeof(EnumFormatter<>).MakeGenericType(typeof(T));
        return Activator.CreateInstance(enumFormatterType);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ITomlValueFormatter<T>? GetFormatter<T>()
    {
        var formatter = DefaultFormatterCache<T>.Formatter;
        if (formatter != null)
            return formatter;

        var genericFormatter = GeneratedFormatterCache<T>.Formatter;
        if (genericFormatter != null)
            return genericFormatter;

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsRegistered<T>()
        => CacheCheck<T>.Registered;
}
