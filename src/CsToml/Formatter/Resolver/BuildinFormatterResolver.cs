
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Numerics;

namespace CsToml.Formatter.Resolver;

internal sealed class BuildinFormatterResolver : ITomlValueFormatterResolver
{
    private sealed class DefaultFormatterCache<T>
    {
        public static ITomlValueFormatter<T>? Formatter;
    }

    private sealed class ArrayFormatterCache<T>
    {
        public static ITomlValueFormatter<T>? Formatter = GetArrayFormatter<T>() as ITomlValueFormatter<T>;
    }

    private sealed class GeneratedFormatterCache<T>
    {
        public static ITomlValueFormatter<T>? Formatter = GetGenericTypeFormatter<T>() as ITomlValueFormatter<T>;
    }

    private static readonly Dictionary<Type, Type> formatterTypeTable = new()
    {
        { typeof(Nullable<>), typeof(NullableStructFormatter<>) },
        { typeof(Memory<>),  typeof(MemoryFormatter<>) },
        { typeof(ReadOnlyMemory<>),  typeof(ReadOnlyMemoryFormatter<>) },

        { typeof(List<>), typeof(ListFormatter<>) },
        { typeof(Stack<>), typeof(StackFormatter<>) },
        { typeof(HashSet<>), typeof(HashSetFormatter<>) },
        { typeof(SortedSet<>), typeof(SortedSetFormatter<>) },
        { typeof(Queue<>), typeof(QueueFormatter<>) },
        { typeof(LinkedList<>), typeof(LinkedListFormatter<>) },
        { typeof(ConcurrentQueue<>), typeof(ConcurrentQueueFormatter<>) },
        { typeof(ConcurrentStack<>), typeof(ConcurrentStackFormatter<>) },
        { typeof(ConcurrentBag<>), typeof(ConcurrentBagFormatter<>) },
        { typeof(ReadOnlyCollection<>), typeof(ReadOnlyCollectionFormatter<>) },

        { typeof(IEnumerable<>), typeof(IEnumerableFormatter<>) },
        { typeof(ICollection<>),  typeof(ICollectionFormatter<>) },
        { typeof(IReadOnlyCollection<>),  typeof(IReadOnlyCollectionFormatter<>) },
        { typeof(IList<>),  typeof(IListFormatter<>) },
        { typeof(IReadOnlyList<>),  typeof(IReadOnlyListFormatter<>) },
        { typeof(ISet<>),  typeof(ISetFormatter<>) },
        { typeof(IReadOnlySet<>),  typeof(IReadOnlySetFormatter<>) },

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
    };

    static BuildinFormatterResolver()
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
        DefaultFormatterCache<string>.Formatter = NullableStringFormatter.Instance;
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

        DefaultFormatterCache<Uri>.Formatter = UriFormatter.Instance;
        DefaultFormatterCache<Guid>.Formatter = GuidFormatter.Instance;
        DefaultFormatterCache<Guid?>.Formatter = NullableGuidFormatter.Instance;
        DefaultFormatterCache<Version>.Formatter = VersionFormatter.Instance;

        DefaultFormatterCache<bool[]>.Formatter = new ArrayFormatter<bool>();
        DefaultFormatterCache<byte[]>.Formatter = new ArrayFormatter<byte>();
        DefaultFormatterCache<sbyte[]>.Formatter = new ArrayFormatter<sbyte>();
        DefaultFormatterCache<short[]>.Formatter = new ArrayFormatter<short>();
        DefaultFormatterCache<ushort[]>.Formatter = new ArrayFormatter<ushort>();
        DefaultFormatterCache<int[]>.Formatter = new ArrayFormatter<int>();
        DefaultFormatterCache<uint[]>.Formatter = new ArrayFormatter<uint>();
        DefaultFormatterCache<long[]>.Formatter = new ArrayFormatter<long>();
        DefaultFormatterCache<ulong[]>.Formatter = new ArrayFormatter<ulong>();
        DefaultFormatterCache<float[]>.Formatter = new ArrayFormatter<float>();
        DefaultFormatterCache<double[]>.Formatter = new ArrayFormatter<double>();
        DefaultFormatterCache<decimal[]>.Formatter = new ArrayFormatter<decimal>();
        DefaultFormatterCache<DateTimeOffset[]>.Formatter = new ArrayFormatter<DateTimeOffset>();
        DefaultFormatterCache<DateTime[]>.Formatter = new ArrayFormatter<DateTime>();
        DefaultFormatterCache<DateOnly[]>.Formatter = new ArrayFormatter<DateOnly>();
        DefaultFormatterCache<TimeOnly[]>.Formatter = new ArrayFormatter<TimeOnly>();
        DefaultFormatterCache<char[]>.Formatter = new ArrayFormatter<char>();
        DefaultFormatterCache<string[]>.Formatter = new ArrayFormatter<string>();

        DefaultFormatterCache<List<bool>>.Formatter = new ListFormatter<bool>();
        DefaultFormatterCache<List<byte>>.Formatter = new ListFormatter<byte>();
        DefaultFormatterCache<List<sbyte>>.Formatter = new ListFormatter<sbyte>();
        DefaultFormatterCache<List<short>>.Formatter = new ListFormatter<short>();
        DefaultFormatterCache<List<ushort>>.Formatter = new ListFormatter<ushort>();
        DefaultFormatterCache<List<int>>.Formatter = new ListFormatter<int>();
        DefaultFormatterCache<List<uint>>.Formatter = new ListFormatter<uint>();
        DefaultFormatterCache<List<long>>.Formatter = new ListFormatter<long>();
        DefaultFormatterCache<List<ulong>>.Formatter = new ListFormatter<ulong>();
        DefaultFormatterCache<List<float>>.Formatter = new ListFormatter<float>();
        DefaultFormatterCache<List<double>>.Formatter = new ListFormatter<double>();
        DefaultFormatterCache<List<decimal>>.Formatter = new ListFormatter<decimal>();
        DefaultFormatterCache<List<DateTimeOffset>>.Formatter = new ListFormatter<DateTimeOffset>();
        DefaultFormatterCache<List<DateTime>>.Formatter = new ListFormatter<DateTime>();
        DefaultFormatterCache<List<DateOnly>>.Formatter = new ListFormatter<DateOnly>();
        DefaultFormatterCache<List<TimeOnly>>.Formatter = new ListFormatter<TimeOnly>();
        DefaultFormatterCache<List<char>>.Formatter = new ListFormatter<char>();
        DefaultFormatterCache<List<string>>.Formatter = new ListFormatter<string>();

        DefaultFormatterCache<Dictionary<string, object?>>.Formatter = new DictionaryFormatter();
        DefaultFormatterCache<IDictionary<string, object?>>.Formatter = new IDictionaryFormatter();
    }

    public static readonly BuildinFormatterResolver Instance = new BuildinFormatterResolver();

    public ITomlValueFormatter<T>? GetFormatter<T>()
    {
        var formatter = DefaultFormatterCache<T>.Formatter;
        if (formatter != null)
            return formatter;

        if (typeof(T).IsArray)
        {
            var arrayFormatter = ArrayFormatterCache<T>.Formatter;
            if (arrayFormatter != null)
                return arrayFormatter;
        }

        if (typeof(T).IsGenericType)
        {
            var genericFormatter = GeneratedFormatterCache<T>.Formatter;
            if (genericFormatter != null)
                return genericFormatter;
        }
        
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
            var genericType =  formatterType.MakeGenericType(type.GetGenericArguments());
            if (genericType != null)
            {
                return Activator.CreateInstance(genericType);
            }
        }
        return null;
    }

}
