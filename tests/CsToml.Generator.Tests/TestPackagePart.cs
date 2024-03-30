

namespace CsToml.Generator.Tests;

[CsTomlPackagePart]
public partial class TestPackagePart
{
    [CsTomlPackageValue(CsTomlValueType.KeyValue)]
    public int IntValue { get; set; } = 123;

    [CsTomlPackageValue(CsTomlValueType.KeyValue)]
    public string StringValue { get; set; } = nameof(TestPackagePart);

    public string IgnoreValue { get; set; } = nameof(TestPackagePart);
}

[CsTomlPackagePart]
public partial class TestPackagePart2
{
    public ushort Ignore { get; set; } = 2;
    [CsTomlPackageValue(CsTomlValueType.Array)]
    public int[] intArr { get; set; } = [2, 3, 4, 5];
    [CsTomlPackageValue(CsTomlValueType.Array)]
    public object[] oArr { get; set; } = [2, "test", 4.02d, DateTime.Now];
    [CsTomlPackageValue(CsTomlValueType.Array)]
    public IEnumerable<object> oArr2 => oArr;
    [CsTomlPackageValue(CsTomlValueType.Array)]
    public List<object> oArr3 { get; set; } = new List<object>(new object[] { 2, "test", 4.02d, DateTime.Now });
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
    [CsTomlArrayOfTablesKey("ArrayOfTablesValue")]
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

