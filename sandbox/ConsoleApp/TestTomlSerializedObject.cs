

using CsToml;
using CsToml.Formatter;
using CsToml.Formatter.Resolver;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Runtime.Serialization;

namespace ConsoleApp;

public enum Test123
{
    [EnumMember(Value = "1")]
    ONe,
    Two,
    tHrEE
};

[TomlSerializedObject]
public partial class EnumObject
{
    [TomlValueOnSerialized()]
    public Test123 Value { get; set; }
}

[TomlSerializedObject]
public partial class TestObject
{
    [TomlValueOnSerialized]
    public BlockingCollection<int> Type { get; set; }
}

[TomlSerializedObject]
public partial class TestTomlSerializedObject2Nest
{
    [TomlValueOnSerialized()]
    public string color { get; set; }
}

[TomlSerializedObject]
public partial class TestTomlSerializedObject3Nest
{
    [TomlValueOnSerialized()]
    public TestTomlSerializedObject2Nest color { get; set; }
}

[TomlSerializedObject]
public partial class TestTomlSerializedObject2
{
    [TomlValueOnSerialized()]
    public IDictionary<object, object> Dict { get; set; }

    [TomlValueOnSerialized()]
    public string bare_key { get; set; }

    [TomlValueOnSerialized()]
    public TestTomlSerializedObject2Nest physical { get; set; }

    [TomlValueOnSerialized()]
    public TestTomlSerializedObject3Nest physical2 { get; set; }


    //[TomlValueOnSerialized()]
    //public string str { get; set; }

    //[TomlValueOnSerialized()]
    //public uint int1 { get; set; }

    //[TomlValueOnSerialized()]
    //public long int8 { get; set; }

    //[TomlValueOnSerialized()]
    //public bool bool1 { get; set; }

    //[TomlValueOnSerialized()]
    //public double flt2 { get; set; }

    //[TomlValueOnSerialized()]
    //public DateTimeOffset odt1 { get; set; }

    //[TomlValueOnSerialized()]
    //public DateTimeOffset odt2 { get; set; }

    //[TomlValueOnSerialized()]
    //public DateTimeOffset odt3 { get; set; }

    //[TomlValueOnSerialized()]
    //public DateTime odt4 { get; set; }

    //[TomlValueOnSerialized()]
    //public DateOnly odt5 { get; set; }

    //[TomlValueOnSerialized()]
    //public TimeOnly lt1 { get; set; }

    //[TomlValueOnSerialized()]
    //public int[] integers { get; set; }

    //[TomlValueOnSerialized()]
    //public List<long> integers2 { get; set; }

    //[TomlValueOnSerialized()]
    //public double[] numbers { get; set; }

    //[TomlValueOnSerialized()]
    //public string[] colors { get; set; }
}

[TomlSerializedObject]
public partial class TestTomlSerializedObject
{
    [TomlValueOnSerialized()]
    public DateTimeOffset odt1 { get; set; }
    [TomlValueOnSerialized()]
    public DateTimeOffset odt2 { get; set; }
    [TomlValueOnSerialized()]
    public string str { get; set; } = "this is str.";

    [TomlValueOnSerialized()]
    public int[] value { get; set; } = [1, 2, 3, 4, 5];

    [TomlValueOnSerialized()]
    public List<long> value2 { get; set; } = [1, 2, 3, 4, 5];

    [TomlValueOnSerialized()]
    public double[] valued { get; set; } = [1.1, 2, 3.4, 4.5, 5];

    [TomlValueOnSerialized()]
    public bool[] value3 { get; set; } = [false, true, false, true];

    [TomlValueOnSerialized()]
    public DateTime[] value4 { get; set; } = [DateTime.Now, DateTime.Now.AddHours(1), DateTime.Now.AddHours(2)];

    [TomlValueOnSerialized()]
    public string[] value5 { get; set; } = ["test", "test", "C:\\Users\\nodejs\\templates", "\\\\ServerX\\admin$\\system32\\"];

    public ushort Ignore { get; set; } = 2;
    [TomlValueOnSerialized()]
    public int[] intArr { get; set; } = new int[] { 2, 3, 4, 5 };

    [TomlValueOnSerialized()]
    public object[] oArr { get; set; } = new object[] { 2, "test", 4.02d, DateTime.Now };

    //[TomlValueOnSerialized()]
    //public List<object> oArr3 { get; set; } = new List<object>(new object[] { new long[] { 1, 2, 3 }, 2, new object[] { 2, "test", 4.02d, DateTime.Now }, });

    [TomlValueOnSerialized()]
    public string EmptyStr { get; set; } = string.Empty;

    //[TomlValueOnSerialized()]
    //public ICollection<object> oArr4 => oArr;

    [TomlValueOnSerialized()]
    public uint IntValue { get; set; } = 2;


    //[TomlValueOnSerialized()]
    //public object DynamicObject { get; set; } = "test";

    //[TomlValueOnSerialized()]
    //public object intVo { get; set; } = 12345;

    [TomlValueOnSerialized()]
    public long LongValue { get; set; } = 100;
    [TomlValueOnSerialized()]
    public bool boolValue { get; set; } = true;
    [TomlValueOnSerialized()]
    public DateTime DateTimeValue { get; set; } = DateTime.Now;
    [TomlValueOnSerialized()]
    public DateOnly DateOnlyValue { get; set; } = DateOnly.MinValue;
    [TomlValueOnSerialized()]
    public TimeOnly TImeOnlyValue { get; set; } = TimeOnly.MaxValue;
    [TomlValueOnSerialized()]
    public DateTimeOffset DateTimeOffsetValue { get; set; } = DateTimeOffset.UtcNow;
    [TomlValueOnSerialized()]
    public double DoubleValue { get; set; } = 0.5d;
}

[TomlSerializedObject]
public partial class TestClass
{
    [TomlValueOnSerialized()]
    public Guid? guid { get; set; }

    [TomlValueOnSerialized(aliasName:"uri2")]
    public Uri? uri { get; set; }

    //[TomlValueOnSerialized()]
    //public Half test128 { get; set; }

    [TomlValueOnSerialized()]
    public List<long>? array { get; set; }

    [TomlValueOnSerialized()]
    public ValueTuple<DateTimeOffset, long, long, string, string, bool, float>? tuple { get; set; }

    [TomlValueOnSerialized()]
    public KeyValuePair<DateTimeOffset, string>? KeyValuePair { get; set; }

    [TomlValueOnSerialized()]
    public IReadOnlyDictionary<object, object> Dict4 { get; set; }

    //[TomlValueOnSerialized()]
    //public long[][] array2 { get; set; }

    //[TomlValueOnSerialized()]
    //public List<long> array3 { get; set; }

    [TomlValueOnSerialized()]
    public ulong Value { get; set; }

    //[TomlValueOnSerialized()]
    //public string Str { get; set; }

    //[TomlValueOnSerialized()]
    //public double Doubl { get; set; }

    [TomlValueOnSerialized()]
    public DateTimeOffset? odt1 { get; set; }

    [TomlValueOnSerialized()]
    public IDictionary<string, object?> Dict { get; set; }

    [TomlValueOnSerialized()]
    public TestC_? Dict2 { get; set; }

    [TomlValueOnSerialized()]
    public TestE_ Dict3 { get; set; }
}

[TomlSerializedObject]
public partial class TestC_
{
    [TomlValueOnSerialized()]
    public TestD_ a { get; set; }

    [TomlValueOnSerialized()]
    public IDictionary<string, object?> Key{ get; set; }

}
[TomlSerializedObject]
public partial class TestD_
{
    [TomlValueOnSerialized()]
    public long a { get; set; }


}
[TomlSerializedObject]
public partial class TestE_
{
    [TomlValueOnSerialized()]
    public List<TestD_> b { get; set; }
}

[TomlSerializedObject]
public partial class CsTomlClass
{
    //[TomlValueOnSerialized]
    //public string Key { get; set; }

    //[TomlValueOnSerialized]
    //public int? Number { get; set; }

    //[TomlValueOnSerialized]
    //public int[] Array { get; set; }

    //[TomlValueOnSerialized(aliasName: "alias")]
    //public string Value { get; set; }

    [TomlValueOnSerialized]
    public TableClass Table { get; set; } = new TableClass();
}

[TomlSerializedObject]
public partial class TableClass
{
    [TomlValueOnSerialized()]
    public string Key { get; set; }

    [TomlValueOnSerialized()]
    public int Number { get; set; }
}

[TomlSerializedObject]
public partial class TableNest
{
    [TomlValueOnSerialized()]
    public string Key { get; set; }

    [TomlValueOnSerialized()]
    public int Number { get; set; }

    [TomlValueOnSerialized()]
    public TableClass Table { get; set; }
}

[TomlSerializedObject]
public partial struct AliasName
{
    [TomlValueOnSerialized]
    public string Key { get; set; }

    [TomlValueOnSerialized]
    public TableNest Table { get; set; }
}

