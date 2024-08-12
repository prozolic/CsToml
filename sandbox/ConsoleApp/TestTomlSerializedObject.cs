
using CsToml;
using System.Buffers;

namespace ConsoleApp;

[TomlSerializedObject]
public partial class TestTomlSerializedObject
{
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public DateTimeOffset odt1 { get; set; }
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public DateTimeOffset odt2 { get; set; }
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public string str { get; set; }

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
    [TomlValueOnSerialized(TomlValueType.Array)]
    public IEnumerable<object> oArr2 => oArr;
    [TomlValueOnSerialized(TomlValueType.Array)]
    public List<object> oArr3 { get; set; } = new List<object>(new object[] { new long[] { 1, 2, 3 }, 2, new object[] { 2, "test", 4.02d, DateTime.Now }, });

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public string EmptyStr { get; set; } = string.Empty;

    //[TomlValueOnSerialized(TomlValueType.Array)]
    //public ICollection<object> oArr4 => oArr;

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public uint IntValue { get; set; } = 2;


    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public object DynamicObject { get; set; } = "test";

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public object intVo { get; set; } = 12345;

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
    [TomlValueOnSerialized(TomlValueType.Table)]
    public Table TableValue { get; set; } = new Table();

    [TomlValueOnSerialized(TomlValueType.InlineTable)]
    public Table TableValue2 { get; set; } = new Table();

    public class Table
    {
        [TomlValueOnSerialized(TomlValueType.KeyValue)]
        public long IntValue { get; set; } = 2;
        [TomlValueOnSerialized(TomlValueType.Array)]
        public List<long> IntArray { get; set; } = new List<long>();
        [TomlValueOnSerialized(TomlValueType.Array)]
        public object[] oArr { get; set; } = new object[] { 2, "test", 4.02d, DateTime.Now };

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
        [TomlValueOnSerialized(TomlValueType.KeyValue)]
        public string あいうえお { get; set; } = "string";
        [TomlValueOnSerialized(TomlValueType.KeyValue)]
        public char As { get; set; } = 'a';

        [TomlValueOnSerialized(TomlValueType.KeyValue)]
        public object UnknownValue { get; set; } = new Test();

        [TomlValueOnSerialized(TomlValueType.InlineTable)]
        public Test Test123 { get; set; } = new Test();
    }

    public class ArrayOfTables
    {
    }
    public class ArrayOfTables1 : ArrayOfTables
    {
        [TomlValueOnSerialized(TomlValueType.KeyValue)]
        public int IntValue { get; set; } = 42342;

    }
    public class ArrayOfTables2 : ArrayOfTables
    {
        [TomlValueOnSerialized(TomlValueType.KeyValue)]
        public string StringValue { get; set; } = "test";

        [TomlValueOnSerialized(TomlValueType.KeyValue)]
        public string StringValue2 { get; set; } = "test";

    }

    public class Test
    {
        [TomlValueOnSerialized(TomlValueType.KeyValue)]
        public int IntValue { get; set; } = 2;

        [TomlValueOnSerialized(TomlValueType.InlineTable)]
        public Test2 Test456 { get; set; } = new Test2();
    }

    public class Test2
    {
        [TomlValueOnSerialized(TomlValueType.KeyValue)]
        public int IntValue { get; set; } = 2;
    }

}

