using CsToml;

#pragma warning disable CS8618

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
    [TomlValueOnSerialized(AliasName = "str")]
    public string? Str { get; set; }

    [TomlValueOnSerialized(AliasName = "long")]
    public long Long { get; set; }

    [TomlValueOnSerialized(AliasName = "float")]
    public double Float { get; set; }

    [TomlValueOnSerialized(AliasName = "boolean")]
    public bool Boolean { get; set; }

    [TomlValueOnSerialized(AliasName = "offsetDateTime")]
    public DateTimeOffset OffsetDateTime { get; set; }

    [TomlValueOnSerialized(AliasName = "local_date_time")]
    public DateTime LocalDateTime { get; set; }

    [TomlValueOnSerialized(AliasName = "array")]
    public string[]? Array { get; set; }

    [TomlValueOnSerialized(AliasName = "table")]
    public Table? Table { get; set; }

    [TomlValueOnSerialized(AliasName = "array_of_table")]
    public List<Table2>? ArrayOfTable { get; set; }
}

[TomlSerializedObject]
public partial class TableInSnakeCase
{
    [TomlValueOnSerialized(AliasName = "value")]
    public string? Value { get; set; }
}

[TomlSerializedObject]
public partial class Table2InSnakeCase
{
    [TomlValueOnSerialized(AliasName = "value")]
    public string? Value { get; set; }
}