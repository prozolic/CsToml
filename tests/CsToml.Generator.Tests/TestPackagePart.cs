

namespace CsToml.Generator.Tests;

[TomlSerializedObject]
public partial class TestPackagePart
{
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public int IntValue { get; set; } = 123;

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public string StringValue { get; set; } = nameof(TestPackagePart);

    public string IgnoreValue { get; set; } = nameof(TestPackagePart);
}

[TomlSerializedObject]
public partial class TestPackagePart2
{
    public ushort Ignore { get; set; } = 2;
    [TomlValueOnSerialized(TomlValueType.Array)]
    public int[] intArr { get; set; } = [2, 3, 4, 5];
    [TomlValueOnSerialized(TomlValueType.Array)]
    public object[] oArr { get; set; } = [2, "test", 4.02d, DateTime.Now];
    public IEnumerable<object> oArr2 => oArr;
    [TomlValueOnSerialized(TomlValueType.Array)]
    public List<object> oArr3 { get; set; } = new List<object>(new object[] { 2, "test", 4.02d, DateTime.Now });

    public ICollection<object> oArr4 => oArr;
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public uint IntValue { get; set; } = 2;
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
    public string StringValue { get; set; } = "";

    [TomlValueOnSerialized(TomlValueType.Table)]
    public Table TableValue { get; set; } = new Table();

    //[TomlValueOnSerialized(TomlValueType.ArrayOfTables)]
    //[CsTomlArrayOfTablesKey("ArrayOfTablesValue",0)]
    public ArrayOfTables1 ArrayOfTablesValue { get; set; } = new ArrayOfTables1();

    //[TomlValueOnSerialized(TomlValueType.ArrayOfTables)]
    //[CsTomlArrayOfTablesKey("ArrayOfTablesValue",1)]
    public ArrayOfTables2 ArrayOfTablesValue2 { get; set; } = new ArrayOfTables2();

    public class Table
    {
        [TomlValueOnSerialized(TomlValueType.KeyValue)]
        public int IntValue { get; set; } = 2;
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
        public object intVo { get; set; } = 12345;
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


}

