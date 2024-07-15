

namespace CsToml.Generator.Tests;

[CsTomlPackagePart]
public partial class TestPackagePart
{
    [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
    public int IntValue { get; set; } = 123;

    [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
    public string StringValue { get; set; } = nameof(TestPackagePart);

    public string IgnoreValue { get; set; } = nameof(TestPackagePart);
}

[CsTomlPackagePart]
public partial class TestPackagePart2
{
    public ushort Ignore { get; set; } = 2;
    [CsTomlValueOnSerialized(CsTomlValueType.Array)]
    public int[] intArr { get; set; } = [2, 3, 4, 5];
    [CsTomlValueOnSerialized(CsTomlValueType.Array)]
    public object[] oArr { get; set; } = [2, "test", 4.02d, DateTime.Now];
    [CsTomlValueOnSerialized(CsTomlValueType.Array)]
    public IEnumerable<object> oArr2 => oArr;
    [CsTomlValueOnSerialized(CsTomlValueType.Array)]
    public List<object> oArr3 { get; set; } = new List<object>(new object[] { 2, "test", 4.02d, DateTime.Now });
    [CsTomlValueOnSerialized(CsTomlValueType.Array)]
    public ICollection<object> oArr4 => oArr;
    [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
    public uint IntValue { get; set; } = 2;
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
    public string StringValue { get; set; } = "";

    [CsTomlValueOnSerialized(CsTomlValueType.Table)]
    public Table TableValue { get; set; } = new Table();

    [CsTomlValueOnSerialized(CsTomlValueType.ArrayOfTables)]
    [CsTomlArrayOfTablesKey("ArrayOfTablesValue")]
    public ArrayOfTables1 ArrayOfTablesValue { get; set; } = new ArrayOfTables1();

    [CsTomlValueOnSerialized(CsTomlValueType.ArrayOfTables)]
    [CsTomlArrayOfTablesKey("ArrayOfTablesValue")]
    public ArrayOfTables2 ArrayOfTablesValue2 { get; set; } = new ArrayOfTables2();

    public class Table
    {
        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public int IntValue { get; set; } = 2;
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
        public object intVo { get; set; } = 12345;
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


}

