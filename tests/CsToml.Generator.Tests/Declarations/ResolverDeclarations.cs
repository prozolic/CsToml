#pragma warning disable CS8618

namespace CsToml.Generator.Tests;

[TomlSerializedObject]
public partial class TypeConcurrentResolveChild
{
    [TomlValueOnSerialized]
    public string Name { get; set; }
}

[TomlSerializedObject]
public partial class TypeConcurrentResolve
{
    [TomlValueOnSerialized]
    public int Value { get; set; }

    [TomlValueOnSerialized]
    public TypeConcurrentResolveChild Child { get; set; }

    [TomlValueOnSerialized]
    public TypeConcurrentResolveChild[] Children { get; set; }
}
