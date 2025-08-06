
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Frozen;
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

        { typeof(FrozenSet<>), typeof(FrozenSetFormatter<>) },
        { typeof(FrozenDictionary<,>), typeof(FrozenDictionaryFormatter<,>) },

        { typeof(IEnumerable<>), typeof(IEnumerableFormatter<>) },
        { typeof(ICollection<>),  typeof(ICollectionFormatter<>) },
        { typeof(IReadOnlyCollection<>),  typeof(IReadOnlyCollectionFormatter<>) },
        { typeof(IList<>),  typeof(IListFormatter<>) },
        { typeof(IReadOnlyList<>),  typeof(IReadOnlyListFormatter<>) },
        { typeof(ISet<>),  typeof(ISetFormatter<>) },
        { typeof(IReadOnlySet<>),  typeof(IReadOnlySetFormatter<>) },
        { typeof(IDictionary<,>),typeof(IDictionaryFormatter<,>) },
        { typeof(IReadOnlyDictionary<,>),typeof(IReadOnlyDictionaryFormatter<,>) },
        { typeof(ILookup<,>), typeof(ILookupFormatter<,>) },
        { typeof(IGrouping<,>), typeof(IGroupingFormatter<,>) },

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
        { typeof(Lazy<>), typeof(LazyFormatter<>) },

#if NET9_0_OR_GREATER
        { typeof(OrderedDictionary<,>), typeof(OrderedDictionaryFormatter<,>) },
        { typeof(ReadOnlySet<>), typeof(ReadOnlySetFormatter<>) },
#endif
    };

    private sealed class CacheCheck<T>
    {
        public static bool Registered;
    }

    private sealed class BuiltinFormatterCache<T>
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
                var formatter = GetEnumFormatter<T>() as ITomlValueFormatter<T>;
                if (formatter != null)
                {
                    Formatter = formatter;
                    CacheCheck<T>.Registered = true;
                }
                return;
            }

            if (typeof(T).IsArray)
            {
                var formatter = GetArrayFormatter<T>() as ITomlValueFormatter<T>;
                if (formatter != null)
                {
                    Formatter = formatter;
                    CacheCheck<T>.Registered = true;
                }
                return;
            }

            if (typeof(T).IsGenericType)
            {
                var formatter = GetGenericTypeFormatter<T>() as ITomlValueFormatter<T>;
                if (formatter != null)
                {
                    Formatter = formatter;
                    CacheCheck<T>.Registered = true;
                }
                return;
            }
        }
    }

    static BuiltinFormatterResolver()
    {
        BuiltinFormatterCache<bool>.Formatter = BooleanFormatter.Instance;
        BuiltinFormatterCache<bool?>.Formatter = NullableBooleanFormatter.Instance;
        BuiltinFormatterCache<byte>.Formatter = ByteFormatter.Instance;
        BuiltinFormatterCache<byte?>.Formatter = NullableByteFormatter.Instance;
        BuiltinFormatterCache<sbyte>.Formatter = SByteFormatter.Instance;
        BuiltinFormatterCache<sbyte?>.Formatter = NullableSByteFormatter.Instance;
        BuiltinFormatterCache<char>.Formatter = CharFormatter.Instance;
        BuiltinFormatterCache<char?>.Formatter = NullableCharFormatter.Instance;
        BuiltinFormatterCache<float>.Formatter = FloatFormatter.Instance;
        BuiltinFormatterCache<float?>.Formatter = NullableFloatFormatter.Instance;
        BuiltinFormatterCache<double>.Formatter = DoubleFormatter.Instance;
        BuiltinFormatterCache<double?>.Formatter = NullableDoubleFormatter.Instance;
        BuiltinFormatterCache<short>.Formatter = Int16Formatter.Instance;
        BuiltinFormatterCache<short?>.Formatter = NullableInt16Formatter.Instance;
        BuiltinFormatterCache<ushort>.Formatter = UInt16Formatter.Instance;
        BuiltinFormatterCache<ushort?>.Formatter = NullableUInt16Formatter.Instance;
        BuiltinFormatterCache<int>.Formatter = Int32Formatter.Instance;
        BuiltinFormatterCache<int?>.Formatter = NullableInt32Formatter.Instance;
        BuiltinFormatterCache<uint>.Formatter = UInt32Formatter.Instance;
        BuiltinFormatterCache<uint?>.Formatter = NullableUInt32Formatter.Instance;
        BuiltinFormatterCache<long>.Formatter = Int64Formatter.Instance;
        BuiltinFormatterCache<long?>.Formatter = NullableInt64Formatter.Instance;
        BuiltinFormatterCache<ulong>.Formatter = UInt64Formatter.Instance;
        BuiltinFormatterCache<ulong?>.Formatter = NullableUInt64Formatter.Instance;

        BuiltinFormatterCache<DateTimeOffset>.Formatter = DateTimeOffsetFormatter.Instance;
        BuiltinFormatterCache<DateTimeOffset?>.Formatter = NullableDateTimeOffsetFormatter.Instance;
        BuiltinFormatterCache<DateTime>.Formatter = DateTimeFormatter.Instance;
        BuiltinFormatterCache<DateTime?>.Formatter = NullableDateTimeFormatter.Instance;
        BuiltinFormatterCache<DateOnly>.Formatter = DateOnlyFormatter.Instance;
        BuiltinFormatterCache<DateOnly?>.Formatter = NullableDateOnlyFormatter.Instance;
        BuiltinFormatterCache<TimeOnly>.Formatter = TimeOnlyFormatter.Instance;
        BuiltinFormatterCache<TimeOnly?>.Formatter = NullableTimeOnlyFormatter.Instance;
        BuiltinFormatterCache<string?>.Formatter = NullableStringFormatter.Instance;
        BuiltinFormatterCache<decimal>.Formatter = DecimalFormatter.Instance;
        BuiltinFormatterCache<decimal?>.Formatter = NullableDecimalFormatter.Instance;
        BuiltinFormatterCache<Half>.Formatter = HalfFormatter.Instance;
        BuiltinFormatterCache<Half?>.Formatter = NullableHalfFormatter.Instance;
        BuiltinFormatterCache<Int128>.Formatter = Int128Formatter.Instance;
        BuiltinFormatterCache<Int128?>.Formatter = NullableInt128Formatter.Instance;
        BuiltinFormatterCache<UInt128>.Formatter = UInt128Formatter.Instance;
        BuiltinFormatterCache<UInt128?>.Formatter = NullableUInt128Formatter.Instance;
        BuiltinFormatterCache<BigInteger>.Formatter = BigIntegerFormatter.Instance;
        BuiltinFormatterCache<BigInteger?>.Formatter = NullableBigIntegerFormatter.Instance;
        BuiltinFormatterCache<TimeSpan>.Formatter = TimeSpanFormatter.Instance;
        BuiltinFormatterCache<TimeSpan?>.Formatter = NullableTimeSpanFormatter.Instance;

        BuiltinFormatterCache<Uri?>.Formatter = UriFormatter.Instance;
        BuiltinFormatterCache<Guid>.Formatter = GuidFormatter.Instance;
        BuiltinFormatterCache<Guid?>.Formatter = NullableGuidFormatter.Instance;
        BuiltinFormatterCache<Version?>.Formatter = VersionFormatter.Instance;
        BuiltinFormatterCache<StringBuilder?>.Formatter = StringBuilderFormatter.Instance;
        BuiltinFormatterCache<BitArray?>.Formatter = BitArrayFormatter.Instance;
        BuiltinFormatterCache<Type?>.Formatter = TypeFormatter.Instance;
        BuiltinFormatterCache<Complex>.Formatter = ComplexFormatter.Instance;
        BuiltinFormatterCache<Complex?>.Formatter = NullableComplexFormatter.Instance;

        BuiltinFormatterCache<bool[]?>.Formatter = new ArrayFormatter<bool>();
        BuiltinFormatterCache<byte[]?>.Formatter = new ArrayFormatter<byte>();
        BuiltinFormatterCache<sbyte[]?>.Formatter = new ArrayFormatter<sbyte>();
        BuiltinFormatterCache<short[]?>.Formatter = new ArrayFormatter<short>();
        BuiltinFormatterCache<ushort[]?>.Formatter = new ArrayFormatter<ushort>();
        BuiltinFormatterCache<int[]?>.Formatter = new ArrayFormatter<int>();
        BuiltinFormatterCache<uint[]?>.Formatter = new ArrayFormatter<uint>();
        BuiltinFormatterCache<long[]?>.Formatter = new ArrayFormatter<long>();
        BuiltinFormatterCache<ulong[]?>.Formatter = new ArrayFormatter<ulong>();
        BuiltinFormatterCache<float[]?>.Formatter = new ArrayFormatter<float>();
        BuiltinFormatterCache<double[]?>.Formatter = new ArrayFormatter<double>();
        BuiltinFormatterCache<decimal[]?>.Formatter = new ArrayFormatter<decimal>();
        BuiltinFormatterCache<DateTimeOffset[]?>.Formatter = new ArrayFormatter<DateTimeOffset>();
        BuiltinFormatterCache<DateTime[]?>.Formatter = new ArrayFormatter<DateTime>();
        BuiltinFormatterCache<DateOnly[]?>.Formatter = new ArrayFormatter<DateOnly>();
        BuiltinFormatterCache<TimeOnly[]?>.Formatter = new ArrayFormatter<TimeOnly>();
        BuiltinFormatterCache<char[]?>.Formatter = new ArrayFormatter<char>();
        BuiltinFormatterCache<string[]?>.Formatter = new ArrayFormatter<string>();

        BuiltinFormatterCache<List<bool>?>.Formatter = new ListFormatter<bool>();
        BuiltinFormatterCache<List<byte>?>.Formatter = new ListFormatter<byte>();
        BuiltinFormatterCache<List<sbyte>?>.Formatter = new ListFormatter<sbyte>();
        BuiltinFormatterCache<List<short>?>.Formatter = new ListFormatter<short>();
        BuiltinFormatterCache<List<ushort>?>.Formatter = new ListFormatter<ushort>();
        BuiltinFormatterCache<List<int>?>.Formatter = new ListFormatter<int>();
        BuiltinFormatterCache<List<uint>?>.Formatter = new ListFormatter<uint>();
        BuiltinFormatterCache<List<long>?>.Formatter = new ListFormatter<long>();
        BuiltinFormatterCache<List<ulong>?>.Formatter = new ListFormatter<ulong>();
        BuiltinFormatterCache<List<float>?>.Formatter = new ListFormatter<float>();
        BuiltinFormatterCache<List<double>?>.Formatter = new ListFormatter<double>();
        BuiltinFormatterCache<List<decimal>?>.Formatter = new ListFormatter<decimal>();
        BuiltinFormatterCache<List<DateTimeOffset>?>.Formatter = new ListFormatter<DateTimeOffset>();
        BuiltinFormatterCache<List<DateTime>?>.Formatter = new ListFormatter<DateTime>();
        BuiltinFormatterCache<List<DateOnly>?>.Formatter = new ListFormatter<DateOnly>();
        BuiltinFormatterCache<List<TimeOnly>?>.Formatter = new ListFormatter<TimeOnly>();
        BuiltinFormatterCache<List<char>?>.Formatter = new ListFormatter<char>();
        BuiltinFormatterCache<List<string>?>.Formatter = new ListFormatter<string>();

        BuiltinFormatterCache<Hashtable>.Formatter = HashtableFormatter.Instance;
        BuiltinFormatterCache<ArrayList?>.Formatter = ArrayListFormatter.Instance;

        BuiltinFormatterCache<Dictionary<string, object?>>.Formatter = new DictionaryFormatter<string, object?>();
        BuiltinFormatterCache<Dictionary<object, object?>>.Formatter = new DictionaryFormatter<object, object?>();
        BuiltinFormatterCache<IDictionary<string, object?>>.Formatter = new IDictionaryFormatter<string, object?>();
        BuiltinFormatterCache<IDictionary<object, object?>>.Formatter = new IDictionaryFormatter<object, object?>();

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

        CacheCheck<Dictionary<string, object?>>.Registered = true;
        CacheCheck<Dictionary<object, object?>>.Registered = true;
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
        var formatter = BuiltinFormatterCache<T>.Formatter;
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
