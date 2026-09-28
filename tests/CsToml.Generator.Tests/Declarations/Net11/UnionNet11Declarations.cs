#pragma warning disable CS8618

namespace CsToml.Generator.Tests;

[TomlUnion<long>]
public union IntOrText(long, string);

[TomlUnion<long>]
public union StandaloneIntOrText(long, string);

public union ListOrTextDecl<T>(List<T>, string);

[TomlSerializedObject]
public partial class Net11UnionHolder
{
    [TomlValueOnSerialized]
    public string? Name { get; set; }

    [TomlValueOnSerialized]
    public IntOrText Num { get; set; }
}

[TomlSerializedObject]
public partial class Net11GenericUnionHolder
{
    [TomlValueOnSerialized]
    [TomlUnion<List<long>>]
    public ListOrTextDecl<long> Value { get; set; }
}
