

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
    //[TomlValueOnSerialized(TomlValueType.Array)]
    //public object[] oArr { get; set; } = [2, "test", 4.02d, DateTime.Now];

    //[TomlValueOnSerialized(TomlValueType.Array)]
    //public List<object> oArr3 { get; set; } = new List<object>(new object[] { 2, "test", 4.02d, DateTime.Now });

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

}

