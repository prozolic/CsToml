#pragma warning disable CS8618

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

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
internal partial struct TestStruct
{
    [TomlValueOnSerialized]
    public int Value { get; set; }

    [TomlValueOnSerialized]
    public string Str { get; set; }
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
internal partial class TypeTable3
{
    [TomlValueOnSerialized]
    public string Value { get; set; }
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
internal partial class TypeTableB
{
    [TomlValueOnSerialized]
    public TypeTableC TableC { get; set; }

    [TomlValueOnSerialized]
    public string Value { get; set; }

    [TomlValueOnSerialized]
    public List<TypeTableE> TableECollection { get; set; }
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