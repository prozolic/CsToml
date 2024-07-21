
using CsToml;
using System.Buffers;

namespace ConsoleApp;

[CsTomlPackagePart]
public partial class TestPackagePart
{
    [CsTomlValueOnSerialized(CsTomlValueType.Array)]
    public int[] value { get; set; } = [1, 2, 3, 4, 5];

    [CsTomlValueOnSerialized(CsTomlValueType.Array)]
    public List<long> value2 { get; set; } = [1, 2, 3, 4, 5];

    [CsTomlValueOnSerialized(CsTomlValueType.Array)]
    public double[] valued { get; set; } = [1.1, 2, 3.4, 4.5, 5];

    [CsTomlValueOnSerialized(CsTomlValueType.Array)]
    public bool[] value3 { get; set; } = [false, true, false, true];

    [CsTomlValueOnSerialized(CsTomlValueType.Array)]
    public DateTime[] value4 { get; set; } = [DateTime.Now, DateTime.Now.AddHours(1), DateTime.Now.AddHours(2)];

    [CsTomlValueOnSerialized(CsTomlValueType.Array)]
    public string[] value5 { get; set; } = ["test", "test", "C:\\Users\\nodejs\\templates", "\\\\ServerX\\admin$\\system32\\"];

    public ushort Ignore { get; set; } = 2;
    [CsTomlValueOnSerialized(CsTomlValueType.Array)]
    public int[] intArr { get; set; } = new int[] { 2, 3, 4, 5 };
    [CsTomlValueOnSerialized(CsTomlValueType.Array)]
    public object[] oArr { get; set; } = new object[] { 2, "test", 4.02d, DateTime.Now };
    [CsTomlValueOnSerialized(CsTomlValueType.Array)]
    public IEnumerable<object> oArr2 => oArr;
    [CsTomlValueOnSerialized(CsTomlValueType.Array)]
    public List<object> oArr3 { get; set; } = new List<object>(new object[] { new long[] { 1, 2, 3 }, 2, new object[] { 2, "test", 4.02d, DateTime.Now }, });

    [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
    public string EmptyStr { get; set; } = string.Empty;

    //[CsTomlValueOnSerialized(CsTomlValueType.Array)]
    //public ICollection<object> oArr4 => oArr;

    [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
    public uint IntValue { get; set; } = 2;


    [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
    public object DynamicObject { get; set; } = "test";

    [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
    public object intVo { get; set; } = 12345;

    [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
    public long LongValue { get; set; } = 100;
    [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
    public bool boolValue { get; set; } = true;
    [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
    public DateTime DateTimeValue { get; set; } = DateTime.Now;
    [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
    public DateOnly DateOnlyValue { get; set; } = DateOnly.MinValue;
    [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
    public TimeOnly TImeOnlyValue { get; set; } = TimeOnly.MaxValue;
    [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
    public DateTimeOffset DateTimeOffsetValue { get; set; } = DateTimeOffset.UtcNow;
    [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
    public double DoubleValue { get; set; } = 0.5d;
    [CsTomlValueOnSerialized(CsTomlValueType.Table)]
    public Table TableValue { get; set; } = new Table();

    [CsTomlValueOnSerialized(CsTomlValueType.InlineTable)]
    public Table TableValue2 { get; set; } = new Table();

    public class Table
    {
        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public long IntValue { get; set; } = 2;
        [CsTomlValueOnSerialized(CsTomlValueType.Array)]
        public List<long> IntArray { get; set; } = new List<long>();
        [CsTomlValueOnSerialized(CsTomlValueType.Array)]
        public object[] oArr { get; set; } = new object[] { 2, "test", 4.02d, DateTime.Now };

        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public long LongValue { get; set; } = 100;
        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public bool boolValue { get; set; } = true;
        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public DateTime DateTimeValue { get; set; } = DateTime.Now;
        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public DateOnly DateOnlyValue { get; set; } = DateOnly.MinValue;
        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public TimeOnly TImeOnlyValue { get; set; } = TimeOnly.MaxValue;
        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public DateTimeOffset DateTimeOffsetValue { get; set; } = DateTimeOffset.UtcNow;
        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public double DoubleValue { get; set; } = 0.5d;
        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public string あいうえお { get; set; } = "string";
        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public char As { get; set; } = 'a';

        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public object UnknownValue { get; set; } = new Test();

        [CsTomlValueOnSerialized(CsTomlValueType.InlineTable)]
        public Test Test123 { get; set; } = new Test();
    }

    public class ArrayOfTables
    {
    }
    public class ArrayOfTables1 : ArrayOfTables
    {
        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public int IntValue { get; set; } = 42342;

    }
    public class ArrayOfTables2 : ArrayOfTables
    {
        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public string StringValue { get; set; } = "test";

        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public string StringValue2 { get; set; } = "test";

    }

    public class Test
    {
        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public int IntValue { get; set; } = 2;

        [CsTomlValueOnSerialized(CsTomlValueType.InlineTable)]
        public Test2 Test456 { get; set; } = new Test2();
    }

    public class Test2
    {
        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public int IntValue { get; set; } = 2;
    }

}

