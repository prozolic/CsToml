
using CsToml;

namespace ConsoleApp;

[CsTomlPackagePart]
public partial class TestPackagePart
{
    [CsTomlPackageValue(CsTomlValueType.Array)]
    public int[] value { get; set; } = [1, 2, 3, 4, 5];

    [CsTomlPackageValue(CsTomlValueType.Array)]
    public List<long> value2 { get; set; } = [1, 2, 3, 4, 5];

    [CsTomlPackageValue(CsTomlValueType.Array)]
    public double[] valued { get; set; } =  [1.1, 2, 3.4, 4.5, 5];

    [CsTomlPackageValue(CsTomlValueType.Array)]
    public bool[] value3 { get; set; } = [false, true, false, true];

    [CsTomlPackageValue(CsTomlValueType.Array)]
    public DateTime[] value4 { get; set; } = [DateTime.Now, DateTime.Now.AddHours(1), DateTime.Now.AddHours(2)];

    [CsTomlPackageValue(CsTomlValueType.Array)]
    public string[] value5 { get; set; } = ["test", "test", "C:\\Users\\nodejs\\templates", "\\\\ServerX\\admin$\\system32\\"];

    public ushort Ignore { get; set; } = 2;
    [CsTomlPackageValue(CsTomlValueType.Array)]
    public int[] intArr { get; set; } = new int[] { 2, 3, 4, 5 };
    [CsTomlPackageValue(CsTomlValueType.Array)]
    public object[] oArr { get; set; } = new object[] { 2, "test", 4.02d, DateTime.Now };
    [CsTomlPackageValue(CsTomlValueType.Array)]
    public IEnumerable<object> oArr2 => oArr;
    [CsTomlPackageValue(CsTomlValueType.Array)]
    public List<object> oArr3 { get; set; } = new List<object>(new object[] { new long[] { 1, 2, 3 }, 2, new object[] { 2, "test", 4.02d, DateTime.Now },  });

    [CsTomlPackageValue(CsTomlValueType.KeyValue)]
    public string EmptyStr { get; set; } = string.Empty;

    [CsTomlPackageValue(CsTomlValueType.Array)]
    public ICollection<object> oArr4 => oArr;
    [CsTomlPackageValue(CsTomlValueType.KeyValue)]
    public uint IntValue { get; set; } = 2;
    [CsTomlPackageValue(CsTomlValueType.KeyValue)]
    public long LongValue { get; set; } = 100;
    [CsTomlPackageValue(CsTomlValueType.KeyValue)]
    public bool boolValue { get; set; } = true;
    [CsTomlPackageValue(CsTomlValueType.KeyValue)]
    public DateTime DateTimeValue { get; set; } = DateTime.Now;
    [CsTomlPackageValue(CsTomlValueType.KeyValue)]
    public DateOnly DateOnlyValue { get; set; } = DateOnly.MinValue;
    [CsTomlPackageValue(CsTomlValueType.KeyValue)]
    public TimeOnly TImeOnlyValue { get; set; } = TimeOnly.MaxValue;
    [CsTomlPackageValue(CsTomlValueType.KeyValue)]
    public DateTimeOffset DateTimeOffsetValue { get; set; } = DateTimeOffset.UtcNow;
    [CsTomlPackageValue(CsTomlValueType.KeyValue)]
    public double DoubleValue { get; set; } = 0.5d;
    [CsTomlPackageValue(CsTomlValueType.KeyValue)]
    public string StringValue { get; set; } = "";

    [CsTomlPackageValue(CsTomlValueType.Table)]
    public Table TableValue { get; set; } = new Table();

    [CsTomlPackageValue(CsTomlValueType.ArrayOfTables)]
    [CsTomlArrayOfTablesKey("ArrayOfTablesValue")]
    public ArrayOfTables1 ArrayOfTablesValue { get; set; } = new ArrayOfTables1();

    [CsTomlPackageValue(CsTomlValueType.ArrayOfTables)]
    [CsTomlArrayOfTablesKey("ArrayOfTablesValue2")]
    public ArrayOfTables2 ArrayOfTablesValue2 { get; set; } = new ArrayOfTables2();

    public class Table
    {
        [CsTomlPackageValue(CsTomlValueType.KeyValue)]
        public int IntValue { get; set; } = 2;
        [CsTomlPackageValue(CsTomlValueType.KeyValue)]
        public long LongValue { get; set; } = 100;
        [CsTomlPackageValue(CsTomlValueType.KeyValue)]
        public bool boolValue { get; set; } = true;
        [CsTomlPackageValue(CsTomlValueType.KeyValue)]
        public DateTime DateTimeValue { get; set; } = DateTime.Now;
        [CsTomlPackageValue(CsTomlValueType.KeyValue)]
        public DateOnly DateOnlyValue { get; set; } = DateOnly.MinValue;
        [CsTomlPackageValue(CsTomlValueType.KeyValue)]
        public TimeOnly TImeOnlyValue { get; set; } = TimeOnly.MaxValue;
        [CsTomlPackageValue(CsTomlValueType.KeyValue)]
        public DateTimeOffset DateTimeOffsetValue { get; set; } = DateTimeOffset.UtcNow;
        [CsTomlPackageValue(CsTomlValueType.KeyValue)]
        public double DoubleValue { get; set; } = 0.5d;
        [CsTomlPackageValue(CsTomlValueType.KeyValue)]
        public string あいうえお { get; set; } = "string";
        [CsTomlPackageValue(CsTomlValueType.KeyValue)]
        public char As { get; set; } = 'a';
        [CsTomlPackageValue(CsTomlValueType.KeyValue)]
        public object intVo { get; set; } = 12345;
    }

    public class ArrayOfTables
    {
    }
    public class ArrayOfTables1 : ArrayOfTables
    {
        [CsTomlPackageValue(CsTomlValueType.KeyValue)]
        public int IntValue { get; set; } = 42342;

    }
    public class ArrayOfTables2 : ArrayOfTables
    {
        [CsTomlPackageValue(CsTomlValueType.KeyValue)]
        public string StringValue { get; set; } = "test";

        [CsTomlPackageValue(CsTomlValueType.KeyValue)]
        public string StringValue2 { get; set; } = "test";

    }


}
