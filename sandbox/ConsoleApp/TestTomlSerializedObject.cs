
using CsToml;
using CsToml.Formatter;
using CsToml.Formatter.Resolver;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Numerics;

namespace ConsoleApp;

[TomlSerializedObject]
public partial class TestTomlSerializedObject2Nest
{
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public string color { get; set; }
}

[TomlSerializedObject]
public partial class TestTomlSerializedObject3Nest
{
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public TestTomlSerializedObject2Nest color { get; set; }
}

[TomlSerializedObject]
public partial class TestTomlSerializedObject2
{
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public IDictionary<string, object?> Dict { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public string bare_key { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public TestTomlSerializedObject2Nest physical { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public TestTomlSerializedObject3Nest physical2 { get; set; }


    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public string str { get; set; }

    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public uint int1 { get; set; }

    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public long int8 { get; set; }

    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public bool bool1 { get; set; }

    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public double flt2 { get; set; }

    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public DateTimeOffset odt1 { get; set; }

    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public DateTimeOffset odt2 { get; set; }

    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public DateTimeOffset odt3 { get; set; }

    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public DateTime odt4 { get; set; }

    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public DateOnly odt5 { get; set; }

    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public TimeOnly lt1 { get; set; }

    //[TomlValueOnSerialized(TomlValueType.Array)]
    //public int[] integers { get; set; }

    //[TomlValueOnSerialized(TomlValueType.Array)]
    //public List<long> integers2 { get; set; }

    //[TomlValueOnSerialized(TomlValueType.Array)]
    //public double[] numbers { get; set; }

    //[TomlValueOnSerialized(TomlValueType.Array)]
    //public string[] colors { get; set; }
}

[TomlSerializedObject]
public partial class TestTomlSerializedObject
{
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public DateTimeOffset odt1 { get; set; }
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public DateTimeOffset odt2 { get; set; }
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public string str { get; set; } = "this is str.";

    [TomlValueOnSerialized(TomlValueType.Array)]
    public int[] value { get; set; } = [1, 2, 3, 4, 5];

    [TomlValueOnSerialized(TomlValueType.Array)]
    public List<long> value2 { get; set; } = [1, 2, 3, 4, 5];

    [TomlValueOnSerialized(TomlValueType.Array)]
    public double[] valued { get; set; } = [1.1, 2, 3.4, 4.5, 5];

    [TomlValueOnSerialized(TomlValueType.Array)]
    public bool[] value3 { get; set; } = [false, true, false, true];

    [TomlValueOnSerialized(TomlValueType.Array)]
    public DateTime[] value4 { get; set; } = [DateTime.Now, DateTime.Now.AddHours(1), DateTime.Now.AddHours(2)];

    [TomlValueOnSerialized(TomlValueType.Array)]
    public string[] value5 { get; set; } = ["test", "test", "C:\\Users\\nodejs\\templates", "\\\\ServerX\\admin$\\system32\\"];

    public ushort Ignore { get; set; } = 2;
    [TomlValueOnSerialized(TomlValueType.Array)]
    public int[] intArr { get; set; } = new int[] { 2, 3, 4, 5 };

    [TomlValueOnSerialized(TomlValueType.Array)]
    public object[] oArr { get; set; } = new object[] { 2, "test", 4.02d, DateTime.Now };

    //[TomlValueOnSerialized(TomlValueType.Array)]
    //public List<object> oArr3 { get; set; } = new List<object>(new object[] { new long[] { 1, 2, 3 }, 2, new object[] { 2, "test", 4.02d, DateTime.Now }, });

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public string EmptyStr { get; set; } = string.Empty;

    //[TomlValueOnSerialized(TomlValueType.Array)]
    //public ICollection<object> oArr4 => oArr;

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public uint IntValue { get; set; } = 2;


    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public object DynamicObject { get; set; } = "test";

    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public object intVo { get; set; } = 12345;

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public long LongValue { get; set; } = 100;
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public bool boolValue { get; set; } = true;
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public DateTime DateTimeValue { get; set; } = DateTime.Now;
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public DateOnly DateOnlyValue { get; set; } = DateOnly.MinValue;
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public TimeOnly TImeOnlyValue { get; set; } = TimeOnly.MaxValue;
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public DateTimeOffset DateTimeOffsetValue { get; set; } = DateTimeOffset.UtcNow;
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public double DoubleValue { get; set; } = 0.5d;
}

[TomlSerializedObject]
public partial class TestClass
{
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public Guid? guid { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public Uri? uri { get; set; }

    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public Half test128 { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public ConcurrentBag<long> array { get; set; }

    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public long[][] array2 { get; set; }

    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public List<long> array3 { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public ulong Value { get; set; }

    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public string Str { get; set; }

    //[TomlValueOnSerialized(TomlValueType.KeyValue)]
    //public double Doubl { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public DateTimeOffset? odt1 { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public IDictionary<string, object?> Dict { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public TestC_? Dict2 { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public TestE_ Dict3 { get; set; }
}

[TomlSerializedObject]
public partial class TestC_
{
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public TestD_ a { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public IDictionary<string, object?> Key{ get; set; }

}
[TomlSerializedObject]
public partial class TestD_
{
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public long a { get; set; }


}
[TomlSerializedObject]
public partial class TestE_
{
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public List<TestD_> b { get; set; }
}

