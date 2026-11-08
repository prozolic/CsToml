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

[TomlSerializedObject]
public partial class TypeA1Outer
{
    [TomlValueOnSerialized]
    public int Id { get; set; }

    [TomlValueOnSerialized]
    public List<TypeA1Slow> Slows { get; set; } = [];

    [TomlValueOnSerialized]
    public List<TypeA1Inner> Items { get; set; } = [];
}

[TomlSerializedObject]
public partial class TypeA1Slow
{
    // Holds the generated TypeA1Outer.Register() in the middle of registering its members.
    static TypeA1Slow()
    {
        TypeA1Gate.EnteredSlowConstructor.Set();
        TypeA1Gate.ReleaseSlowConstructor.Wait();
    }

    [TomlValueOnSerialized]
    public int Value { get; set; }
}

[TomlSerializedObject]
public partial class TypeA1Inner
{
    [TomlValueOnSerialized]
    public int Value { get; set; }
}

public static class TypeA1Gate
{
    public static readonly ManualResetEventSlim EnteredSlowConstructor = new();
    public static readonly ManualResetEventSlim ReleaseSlowConstructor = new();
}

[TomlSerializedObject]
public partial class TypeCycleNode
{
    [TomlValueOnSerialized]
    public int Value { get; set; }

    [TomlValueOnSerialized]
    public List<TypeCycleNode> Children { get; set; } = [];
}
