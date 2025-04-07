using CsToml;

namespace Benchmark.Model;


[CsToml.TomlSerializedObject]
public partial class TestTomlSerializedObject
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
    public string[]? Array { get; set; }

    [TomlValueOnSerialized]
    public Table? Table { get; set; }

    [TomlValueOnSerialized]
    public List<Table2>? ArrayOfTable { get; set; }
}

[TomlSerializedObject]
public partial class Table
{
    [TomlValueOnSerialized]
    public string? Value { get; set; }
}

[TomlSerializedObject]
public partial class Table2
{
    [TomlValueOnSerialized]
    public string? Value { get; set; }
}

[CsToml.TomlSerializedObject]
public partial class TestTomlSerializedObjectInSnakeCase
{
    [TomlValueOnSerialized("str")]
    public string? Str { get; set; }

    [TomlValueOnSerialized("long")]
    public long Long { get; set; }

    [TomlValueOnSerialized("float")]
    public double Float { get; set; }

    [TomlValueOnSerialized("boolean")]
    public bool Boolean { get; set; }

    [TomlValueOnSerialized("offsetDateTime")]
    public DateTimeOffset OffsetDateTime { get; set; }

    [TomlValueOnSerialized("local_date_time")]
    public DateTime LocalDateTime { get; set; }

    [TomlValueOnSerialized("array")]
    public string[]? Array { get; set; }

    [TomlValueOnSerialized("table")]
    public Table? Table { get; set; }

    [TomlValueOnSerialized("array_of_table")]
    public List<Table2>? ArrayOfTable { get; set; }
}

[TomlSerializedObject]
public partial class TableInSnakeCase
{
    [TomlValueOnSerialized("value")]
    public string? Value { get; set; }
}

[TomlSerializedObject]
public partial class Table2InSnakeCase
{
    [TomlValueOnSerialized("value")]
    public string? Value { get; set; }
}