#pragma warning disable CS8618

using CsToml.Formatter;
using CsToml.Values;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text;

using CsToml.Generator.Other;
using System.Numerics;
using System.Collections.Frozen;

namespace CsToml.Generator.Tests;

[TomlSerializedObject]
internal partial class TypeZero
{

}

[TomlSerializedObject]
internal partial class TypeOne
{
    [TomlValueOnSerialized]
    public int Value { get; set; }
}

[TomlSerializedObject]
internal partial class TypeTwo
{
    [TomlValueOnSerialized]
    public int Value { get; set; }

    [TomlValueOnSerialized]
    public int Value2 { get; set; }
}

[TomlSerializedObject]
internal partial record TypeRecord
{
    [TomlValueOnSerialized]
    public int Value { get; set; }
    [TomlValueOnSerialized]
    public string Str { get; set; }
}

[TomlSerializedObject]
internal partial struct TestStructParent
{
    [TomlValueOnSerialized]
    public string Value { get; set; }

    [TomlValueOnSerialized]
    public TestStruct TestStruct { get; set; }

    [TomlValueOnSerialized]
    public List<TestStruct> TestStructList { get; set; }
}

[TomlSerializedObject]
internal partial record struct TestRecordStruct
{
    [TomlValueOnSerialized]
    public int Value { get; set; }

    [TomlValueOnSerialized]
    public string Str { get; set; }
}

[TomlSerializedObject]
internal partial record TypeIgnore
{
    [TomlValueOnSerialized]
    public int Value { get; set; }
    public string Str { get; set; }
}

[TomlSerializedObject]
internal partial class WithArray
{
    [TomlValueOnSerialized]
    public long[] Value { get; set; }
}

[TomlSerializedObject]
internal partial class WithArray2
{
    [TomlValueOnSerialized]
    public long[][] Value { get; set; }
}

[TomlSerializedObject]
internal partial class WithNullableArray
{
    [TomlValueOnSerialized]
    public long[]? Value { get; set; }
}

[TomlSerializedObject]
internal partial class WithNullableArray2
{
    [TomlValueOnSerialized]
    public long?[]? Value { get; set; }
}

[TomlSerializedObject]
internal partial class WithCollection
{
    [TomlValueOnSerialized]
    public List<long> Value { get; set; }
}

[TomlSerializedObject]
internal partial class WithNullableCollection
{
    [TomlValueOnSerialized]
    public List<long>? Value { get; set; }
}

[TomlSerializedObject]
internal partial class TomlPrimitive
{
    [TomlValueOnSerialized]
    public string Str { get; set; }

    [TomlValueOnSerialized]
    public long Long { get; set; }

    [TomlValueOnSerialized]
    public double Float { get; set; }

    [TomlValueOnSerialized]
    public bool Boolean { get; set; }

    [TomlValueOnSerialized]
    public DateTimeOffset OffsetDateTime { get; set; }

    [TomlValueOnSerialized]
    public DateTime LocalDateTime { get; set; }

    [TomlValueOnSerialized]
    public DateOnly LocalDate { get; set; }

    [TomlValueOnSerialized]
    public TimeOnly LocalTime { get; set; }

}

[TomlSerializedObject]
internal partial class TypeInteger
{
    [TomlValueOnSerialized]
    public byte Byte { get; set; }

    [TomlValueOnSerialized]
    public sbyte SByte { get; set; }

    [TomlValueOnSerialized]
    public short Short { get; set; }

    [TomlValueOnSerialized]
    public ushort UShort { get; set; }

    [TomlValueOnSerialized]
    public int Int { get; set; }

    [TomlValueOnSerialized]
    public uint Uint { get; set; }

    [TomlValueOnSerialized]
    public long Long { get; set; }

    [TomlValueOnSerialized]
    public ulong ULong { get; set; }

    [TomlValueOnSerialized]
    public decimal Decimal { get; set; }

    [TomlValueOnSerialized]
    public BigInteger BigInteger { get; set; }

    [TomlValueOnSerialized]
    public Int128 Int128 { get; set; }

    [TomlValueOnSerialized]
    public UInt128 UInt128 { get; set; }

}

[TomlSerializedObject]
internal partial class TypeBuiltin
{
    [TomlValueOnSerialized]
    public TimeSpan TimeSpan { get; set; }

    [TomlValueOnSerialized]
    public Guid Guid { get; set; }

    [TomlValueOnSerialized]
    public Version Version { get; set; }

    [TomlValueOnSerialized]
    public Uri Uri { get; set; }

    [TomlValueOnSerialized]
    public BitArray BitArray { get; set; }

    [TomlValueOnSerialized]
    public Type Type { get; set; }

    [TomlValueOnSerialized]
    public Complex Complex { get; set; }
}

[TomlSerializedObject]
internal partial class NullableTypeBuiltin
{
    [TomlValueOnSerialized]
    public TimeSpan? TimeSpan { get; set; }

    [TomlValueOnSerialized]
    public Guid? Guid { get; set; }

    [TomlValueOnSerialized]
    public Complex? Complex { get; set; }
}

[TomlSerializedObject]
public partial class TypeTomlDouble
{
    [TomlValueOnSerialized]
    public double Normal { get; set; }

    [TomlValueOnSerialized]
    public double Inf { get; set; }

    [TomlValueOnSerialized]
    public double NInf { get; set; }

    [TomlValueOnSerialized]
    public double Nan { get; set; }
}

[TomlSerializedObject]
internal partial class NullableType
{
    [TomlValueOnSerialized]
    public int? Value { get; set; }
}

[TomlSerializedObject]
public partial class WithTuple
{
    [TomlValueOnSerialized]
    public Tuple<int> One { get; set; } = default!;
    [TomlValueOnSerialized]
    public Tuple<int, int> Two { get; set; } = default!;
    [TomlValueOnSerialized]
    public Tuple<int, int, int> Three { get; set; } = default!;
    [TomlValueOnSerialized]
    public Tuple<int, int, int, int> Four { get; set; } = default!;
    [TomlValueOnSerialized]
    public Tuple<int, int, int, int, int> Five { get; set; } = default!;
    [TomlValueOnSerialized]
    public Tuple<int, int, int, int, int, int> Six { get; set; } = default!;
    [TomlValueOnSerialized]
    public Tuple<int, int, int, int, int, int, int> Seven { get; set; } = default!;
    [TomlValueOnSerialized]
    public Tuple<int, int, int, int, int, int, int, Tuple<int>> Eight { get; set; } = default!;
}


[TomlSerializedObject]
public partial class WithValueTuple
{
    [TomlValueOnSerialized]
    public ValueTuple<int> One { get; set; } = default!;
    [TomlValueOnSerialized]
    public ValueTuple<int, int> Two { get; set; } = default!;
    [TomlValueOnSerialized]
    public ValueTuple<int, int, int> Three { get; set; } = default!;
    [TomlValueOnSerialized]
    public ValueTuple<int, int, int, int> Four { get; set; } = default!;
    [TomlValueOnSerialized]
    public ValueTuple<int, int, int, int, int> Five { get; set; } = default!;
    [TomlValueOnSerialized]
    public ValueTuple<int, int, int, int, int, int> Six { get; set; } = default!;
    [TomlValueOnSerialized]
    public ValueTuple<int, int, int, int, int, int, int> Seven { get; set; } = default!;
    [TomlValueOnSerialized]
    public ValueTuple<int, int, int, int, int, int, int, ValueTuple<int>> Eight { get; set; } = default!;
}


[TomlSerializedObject]
internal partial class TypeTable
{
    [TomlValueOnSerialized]
    public TypeTable2 Table2 { get; set; }
}

[TomlSerializedObject]
internal partial class TypeTable2
{
    [TomlValueOnSerialized]
    public TypeTable3 Table3 { get; set; }
}

[TomlSerializedObject]
internal partial class TypeTomlSerializedObjectList
{
    [TomlValueOnSerialized]
    public List<TypeTable2> Table2 { get; set; }
}

[TomlSerializedObject]
internal partial class TypeTomlSerializedObjectList2
{
    [TomlValueOnSerialized]
    public int Value { get; set; }

    [TomlValueOnSerialized]
    public List<TypeTable> Table { get; set; }
}

[TomlSerializedObject]
internal partial class TypeCollection
{
    [TomlValueOnSerialized]
    public List<int> Value { get; set; }

    [TomlValueOnSerialized]
    public Stack<int> Value2 { get; set; }

    [TomlValueOnSerialized]
    public HashSet<int> Value3 { get; set; }

    [TomlValueOnSerialized]
    public SortedSet<int> Value4 { get; set; }

    [TomlValueOnSerialized]
    public Queue<int> Value5 { get; set; }

    [TomlValueOnSerialized]
    public LinkedList<int> Value6 { get; set; }

    [TomlValueOnSerialized]
    public ConcurrentQueue<int> Value7 { get; set; }

    [TomlValueOnSerialized]
    public ConcurrentStack<int> Value8 { get; set; }

    [TomlValueOnSerialized]
    public ConcurrentBag<int> Value9 { get; set; }

    [TomlValueOnSerialized]
    public ReadOnlyCollection<int> Value10 { get; set; }
}

[TomlSerializedObject]
internal partial class TypeCollectionInterface
{
    [TomlValueOnSerialized]
    public IEnumerable<int> Value { get; set; }

    [TomlValueOnSerialized]
    public ICollection<int> Value2 { get; set; }

    [TomlValueOnSerialized]
    public IList<int> Value3 { get; set; }

    [TomlValueOnSerialized]
    public ISet<int> Value4 { get; set; }

    [TomlValueOnSerialized]
    public IReadOnlyCollection<int> Value5 { get; set; }

    [TomlValueOnSerialized]
    public IReadOnlyList<int> Value6 { get; set; }

    [TomlValueOnSerialized]
    public IReadOnlySet<int> Value7 { get; set; }
}

[TomlSerializedObject]
internal partial class TypeLinqInterface
{
    [TomlValueOnSerialized]
    public ILookup<string, KeyValuePair<int, string>> Lookup { get; set; }

    [TomlValueOnSerialized]
    public ILookup<string, KeyValuePair<int, TestStruct>> Lookup2 { get; set; }

    [TomlValueOnSerialized]
    public IGrouping<string, KeyValuePair<int, string>> Grouping { get; set; }

    [TomlValueOnSerialized]
    public IGrouping<string, KeyValuePair<int, TestStruct>> Grouping2 { get; set; }
}

[TomlSerializedObject]
internal partial class TypeDictionary
{
    [TomlValueOnSerialized()]
    public IDictionary<string, object?> Value { get; set; }
}

[TomlSerializedObject]
internal partial class TypeDictionary2
{
    [TomlValueOnSerialized()]
    public Dictionary<object, object> Value { get; set; }

    [TomlValueOnSerialized()]
    public ReadOnlyDictionary<object, object> Value2 { get; set; }

    [TomlValueOnSerialized()]
    public SortedDictionary<object, object> Value3 { get; set; }

    [TomlValueOnSerialized()]
    public ConcurrentDictionary<object, object> Value4 { get; set; }

    [TomlValueOnSerialized()]
    public IDictionary<object, object> Value5 { get; set; }

    [TomlValueOnSerialized()]
    public IReadOnlyDictionary<object, object> Value6 { get; set; }
}

[TomlSerializedObject]
internal partial class TypeDictionary3
{
    [TomlValueOnSerialized()]
    public Dictionary<long, string> Value { get; set; }
}

[TomlSerializedObject]
internal partial class TypeFrozen
{
    [TomlValueOnSerialized()]
    public FrozenSet<long> Value { get; set; }

    [TomlValueOnSerialized()]
    public FrozenDictionary<long, string> Value2 { get; set; }
}

[TomlSerializedObject]
internal partial class TypeHashtable
{
    [TomlValueOnSerialized()]
    public Hashtable Value { get; set; }
}

[TomlSerializedObject]
internal partial class TypeArrayOfTable
{
    [TomlValueOnSerialized]
    public TypeTomlSerializedObjectList Header { get; set; }
}


[TomlSerializedObject]
internal partial class TypeArrayOfTable2
{
    [TomlValueOnSerialized]
    public ImmutableArray<TestStruct?> TestStructArray { get; set; }

    [TomlValueOnSerialized]
    public Dictionary<long, string> Dict { get; set; }
}

[TomlSerializedObject]
internal partial class TypeAlias
{
    [TomlValueOnSerialized(aliasName:"alias")]
    public string Value { get; set; }
}


[TomlSerializedObject]
internal partial class TypeTableA
{
    [TomlValueOnSerialized]
    public TypeTableB TableB { get; set; }

    [TomlValueOnSerialized]

    public ConcurrentDictionary<int, string> Dict { get; set; }
}

[TomlSerializedObject]
internal partial class TypeTableC
{
    [TomlValueOnSerialized]
    public TypeTableD TableD { get; set; }

    [TomlValueOnSerialized]
    public string Value { get; set; }
}

[TomlSerializedObject]
internal partial class TypeTableD
{
    [TomlValueOnSerialized]
    public string Value { get; set; }
}

[TomlSerializedObject]
internal partial class TypeTableE
{
    [TomlValueOnSerialized]
    public TypeTableF TableF { get; set; }
}

[TomlSerializedObject]
internal partial class TypeTableF
{
    [TomlValueOnSerialized]
    public string Value { get; set; }
}

[TomlSerializedObject]
internal partial class TypeSortedList
{
    [TomlValueOnSerialized]
    public SortedList<string, string> Value { get; set; }
}

[TomlSerializedObject]
internal partial class Constructor
{
    public Constructor()
    {}

    [TomlValueOnSerialized]
    public string Str { get; set; }

    [TomlValueOnSerialized]
    public long Long { get; set; }

    [TomlValueOnSerialized]
    public double Float { get; set; }

    [TomlValueOnSerialized]
    public bool Boolean { get; set; }
}


[TomlSerializedObject]
internal partial class Constructor2(string str, double floatValue)
{
    [TomlValueOnSerialized]
    public string Str { get; set; } = str;

    [TomlValueOnSerialized]
    public long IntValue { get; set; }

    [TomlValueOnSerialized]
    public double FloatValue { get; set; } = floatValue;

    [TomlValueOnSerialized]
    public bool BooleanValue { get; set; }
}

[TomlSerializedObject]
internal partial class Constructor3(string str, long intValue, double floatValue, bool booleanValue)
{
    [TomlValueOnSerialized]
    public string Str { get; set; } = str;

    [TomlValueOnSerialized]
    public long IntValue { get; set; } = intValue;

    [TomlValueOnSerialized]
    public double FloatValue { get; set; } = floatValue;

    [TomlValueOnSerialized]
    public bool BooleanValue { get; set; } = booleanValue;
}

[TomlSerializedObject]
internal partial class Constructor4
{
    public Constructor4() { }

    public Constructor4(string str, long intValue)
    {
        Str = str;
        IntValue = intValue;
    }

    public Constructor4(string str,double floatValue, long intValue)
    {
        Str = str;
        IntValue = intValue;
    }
    public Constructor4(bool booleanValue, string str, double floatValue, long intValue)
    {
        Str = str;
        IntValue = intValue;
        FloatValue = floatValue;
        BooleanValue = booleanValue;
    }

        [TomlValueOnSerialized]
    public string Str { get; set; }

    [TomlValueOnSerialized]
    public long IntValue { get; set; }

    [TomlValueOnSerialized]
    public double FloatValue { get; set; }

    [TomlValueOnSerialized]
    public bool BooleanValue { get; set; }
}

[TomlSerializedObject]
internal partial class Constructor5
{
    public Constructor5() { }

    public Constructor5(string str2, long intValue2) // parameter name is different
    {
        Str = str2;
        IntValue = intValue2;
    }

    public Constructor5(bool booleanValue, string str, float floatValue) // parameter type is different
    {
        Str = str;
        FloatValue = floatValue;
        BooleanValue = booleanValue;
    }

    [TomlValueOnSerialized]
    public string Str { get; set; }

    [TomlValueOnSerialized]
    public long IntValue { get; set; }

    [TomlValueOnSerialized]
    public double FloatValue { get; set; }

    [TomlValueOnSerialized]
    public bool BooleanValue { get; set; }
}

[TomlSerializedObject]
internal partial class Constructor6
{
    internal Constructor6() { }

    public Constructor6(string str, long intValue)
    {
        Str = str;
        IntValue = intValue;
    }

    [TomlValueOnSerialized]
    public string Str { get; set; }

    [TomlValueOnSerialized]
    public long IntValue { get; set; }

    [TomlValueOnSerialized]
    public double FloatValue { get; set; }

    [TomlValueOnSerialized]
    public bool BooleanValue { get; set; }
}

[TomlSerializedObject]
internal partial class Constructor7
{
    public Constructor7(string str, long intValue)
    {
        Str = str;
        IntValue = intValue;
    }

    [TomlValueOnSerialized]
    public string Str { get; }

    [TomlValueOnSerialized]
    public long IntValue { get;}
}

[TomlSerializedObject]
internal partial class Constructor8
{
    public Constructor8(string str, long intValue)
    {
        Str = str;
        IntValue = intValue;
    }

    [TomlValueOnSerialized]
    public string Str { get; }

    [TomlValueOnSerialized]
    public long IntValue { get; }

    [TomlValueOnSerialized]
    public double FloatValue { get; set; }

    [TomlValueOnSerialized]
    public bool BooleanValue { get; set; }
}

[TomlSerializedObject]
internal partial class Init
{
    [TomlValueOnSerialized]
    public string Str { get; init; }

    [TomlValueOnSerialized]
    public long IntValue { get; init; }

    [TomlValueOnSerialized]
    public double FloatValue { get; init; }

    [TomlValueOnSerialized]
    public bool BooleanValue { get; init; }
}

[TomlSerializedObject]
internal partial class Init2
{
    [TomlValueOnSerialized]
    public string Str { get; set; }

    [TomlValueOnSerialized]
    public long IntValue { get; init; }

    [TomlValueOnSerialized]
    public double FloatValue { get; set; }

    [TomlValueOnSerialized]
    public bool BooleanValue { get; init; }
}

[TomlSerializedObject]
internal partial class ConstructorAndInit(long intValue, double floatValue)
{
    [TomlValueOnSerialized]
    public string Str { get; set; }

    [TomlValueOnSerialized]
    public long IntValue { get; init; } = intValue;

    [TomlValueOnSerialized]
    public double FloatValue { get; set; } = floatValue;

    [TomlValueOnSerialized]
    public bool BooleanValue { get; init; }
}

[TomlSerializedObject]
internal partial class NullableReferenceTypes
{
    [TomlValueOnSerialized]
    public string Str { get; set; }

    [TomlValueOnSerialized]
    public string? NullableStr { get; set; }

    [TomlValueOnSerialized]
    public Uri Uri { get; set; }

    [TomlValueOnSerialized]
    public Uri? NullableUri { get; set; }

    [TomlValueOnSerialized]
    public Version Version { get; set; }

    [TomlValueOnSerialized]
    public Version? NullableVersion { get; set; }

    [TomlValueOnSerialized]
    public StringBuilder StringBuilder { get; set; }

    [TomlValueOnSerialized]
    public StringBuilder? NullableStringBuilder { get; set; }

    [TomlValueOnSerialized]
    public Type Type { get; set; }

    [TomlValueOnSerialized]
    public Type? NullableType { get; set; }

    [TomlValueOnSerialized]
    public BitArray BitArray { get; set; }

    [TomlValueOnSerialized]
    public BitArray? NullableBitArray { get; set; }
}

[TomlSerializedObject]
internal partial class TypeImmutable
{
    [TomlValueOnSerialized]
    public ImmutableArray<int> ImmutableArray { get; set; }

    [TomlValueOnSerialized]
    public ImmutableList<int> ImmutableList { get; set; }

    [TomlValueOnSerialized]
    public ImmutableStack<int> ImmutableStack { get; set; }

    [TomlValueOnSerialized]
    public ImmutableHashSet<int> ImmutableHashSet { get; set; }

    [TomlValueOnSerialized]
    public ImmutableSortedSet<int> ImmutableSortedSet { get; set; }

    [TomlValueOnSerialized]
    public ImmutableQueue<int> ImmutableQueue { get; set; }

    [TomlValueOnSerialized]
    public ImmutableDictionary<string, object?> ImmutableDictionary { get; set; }

    [TomlValueOnSerialized]
    public ImmutableSortedDictionary<string, object?> ImmutableSortedDictionary { get; set; }
}

[TomlSerializedObject]
internal partial class TypeImmutable2
{
    [TomlValueOnSerialized]
    public ImmutableArray<TypeTable3> ImmutableArray { get; set; }

    [TomlValueOnSerialized]
    public ImmutableArray<TypeTable3>? NullableImmutableArray { get; set; }

    [TomlValueOnSerialized]
    public ImmutableList<TypeTable3> ImmutableList { get; set; }

    [TomlValueOnSerialized]
    public IImmutableList<TypeTable3> IImmutableList { get; set; }
}

[TomlSerializedObject]
internal partial class TypeImmutableInterface
{
    [TomlValueOnSerialized]
    public IImmutableList<int> IImmutableList { get; set; }

    [TomlValueOnSerialized]
    public IImmutableStack<int> IImmutableStack { get; set; }

    [TomlValueOnSerialized]
    public IImmutableQueue<int> IImmutableQueue { get; set; }

    [TomlValueOnSerialized]
    public IImmutableSet<int> IImmutableSet { get; set; }

    [TomlValueOnSerialized]
    public IImmutableDictionary<string, object?> IImmutableDictionary { get; set; }
}

[TomlSerializedObject]
public partial class TypeMemory
{
    [TomlValueOnSerialized]
    public Memory<byte> Memory { get; set; }
}

public enum Color
{
    Red,
    Green,
    Blue
}

[TomlSerializedObject]
public partial class TypeEnum
{
    [TomlValueOnSerialized]
    public Color Color { get; set; }
}

public struct SpecialHash : IEquatable<uint>, IEquatable<SpecialHash>, IEquatable<string>
{
    public uint Hash { get; }
    public SpecialHash()
    {
    }

    public SpecialHash(uint hash)
    {
        Hash = hash;
    }

    public SpecialHash(string unhashedStr)
    {
        var bytesToHash = unhashedStr.ToCharArray()
                .Select(c => new[] { (byte)((c - (byte)c) >> 8), (byte)c })
                .SelectMany(c => c);

        Hash = 2166136261; // Fnv offset

        foreach (var chunk in bytesToHash)
        {
            Hash ^= chunk;
            Hash *= 16777619; // fnv prime
        }
    }

    public readonly bool Equals(SpecialHash other) => other.Hash == Hash;
    public readonly bool Equals(uint other) => Hash == other;
    public readonly bool Equals(string? other) => other is not null && Hash == new SpecialHash(other).Hash;
    public override readonly bool Equals(object? obj) => (obj is SpecialHash h && Equals(h)) || (obj is string s && Equals(s)) || (obj is uint u && Equals(u));
    public override readonly int GetHashCode() => unchecked((int)Hash);
}

[TomlSerializedObject]
public partial class Entity
{
    [TomlValueOnSerialized]
    public SpecialHash Name { get; set; }
}

public class SpecialHashFormatter : ITomlValueFormatter<SpecialHash>
{
    public SpecialHash Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.ValueType == TomlValueType.Integer)
        {
            return new SpecialHash(rootNode.GetValue<uint>());
        }

        if (rootNode.ValueType == TomlValueType.String)
        {
            return new SpecialHash(rootNode.GetString());
        }

        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, SpecialHash target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteInt64(target.Hash);
    }
}

[TomlSerializedObject]
public partial class WithLazy
{
    [TomlValueOnSerialized]
    public Lazy<int> Int { get; set; }

    [TomlValueOnSerialized]
    public Lazy<int?> NullableInt { get; set; }

    [TomlValueOnSerialized]
    public Lazy<string> Str { get; set; }

    [TomlValueOnSerialized]
    public Lazy<List<int>> IntList { get; set; }
}

[TomlSerializedObject]
internal partial record AliasName
{
    [TomlValueOnSerialized("ba-re_Key")]
    public string? BareKey { get; set; }

    [TomlValueOnSerialized("")]
    public string? Empty { get; set; }

    [TomlValueOnSerialized("あいうえお")]
    public string? Hiragana { get; set; }

    [TomlValueOnSerialized("127.0.0.1")]
    public string? IpAddress { get; set; }

    [TomlValueOnSerialized("https://github.com/prozolic/CsToml")]
    public string? Url { get; set; }

    [TomlValueOnSerialized("<\\i\\c*\\s*\\\\>")]
    public string? Literal { get; set; }
}

// Naming Convention Tests
[TomlSerializedObject(TomlNamingConvention.SnakeCase)]
internal partial class TypeSnakeCase
{
    [TomlValueOnSerialized]
    public string MyProperty { get; set; }

    [TomlValueOnSerialized]
    public int AnotherValue { get; set; }

    [TomlValueOnSerialized]
    public string XMLParser { get; set; }
}

[TomlSerializedObject(TomlNamingConvention.KebabCase)]
internal partial class TypeKebabCase
{
    [TomlValueOnSerialized]
    public string MyProperty { get; set; }

    [TomlValueOnSerialized]
    public int AnotherValue { get; set; }
}

[TomlSerializedObject(TomlNamingConvention.CamelCase)]
internal partial class TypeCamelCase
{
    [TomlValueOnSerialized]
    public string MyProperty { get; set; }

    [TomlValueOnSerialized]
    public int AnotherValue { get; set; }
}

[TomlSerializedObject(TomlNamingConvention.PascalCase)]
internal partial class TypePascalCase
{
    [TomlValueOnSerialized]
    public string myProperty { get; set; }

    [TomlValueOnSerialized]
    public int anotherValue { get; set; }
}

[TomlSerializedObject(TomlNamingConvention.LowerCase)]
internal partial class TypeLowerCase
{
    [TomlValueOnSerialized]
    public string MyProperty { get; set; }

    [TomlValueOnSerialized]
    public int AnotherValue { get; set; }
}

[TomlSerializedObject(TomlNamingConvention.UpperCase)]
internal partial class TypeUpperCase
{
    [TomlValueOnSerialized]
    public string MyProperty { get; set; }

    [TomlValueOnSerialized]
    public int AnotherValue { get; set; }
}

[TomlSerializedObject(TomlNamingConvention.ScreamingSnakeCase)]
internal partial class TypeScreamingSnakeCase
{
    [TomlValueOnSerialized]
    public string MyProperty { get; set; }

    [TomlValueOnSerialized]
    public int AnotherValue { get; set; }
}

[TomlSerializedObject(TomlNamingConvention.ScreamingKebabCase)]
internal partial class TypeScreamingKebabCase
{
    [TomlValueOnSerialized]
    public string MyProperty { get; set; }

    [TomlValueOnSerialized]
    public int AnotherValue { get; set; }
}

// Test that explicit alias overrides naming convention
[TomlSerializedObject(TomlNamingConvention.SnakeCase)]
internal partial class TypeAliasOverridesConvention
{
    [TomlValueOnSerialized]
    public string MyProperty { get; set; }  // Will be converted to "my_property"

    [TomlValueOnSerialized("custom_name")]
    public int AnotherValue { get; set; }   // Will use "custom_name" instead of "another_value"
}

#if NET9_0_OR_GREATER

[TomlSerializedObject]
internal partial class TypeOrderedDictionary
{
    [TomlValueOnSerialized]
    public OrderedDictionary<string, object?> Value { get; set; }
}

[TomlSerializedObject]
internal partial class TypeReadOnlySetFormatter
{
    [TomlValueOnSerialized]
    public ReadOnlySet<long> Value { get; set; }
}

#endif
