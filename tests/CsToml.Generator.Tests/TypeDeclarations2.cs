using CsToml.Generator.Tests;

namespace CsToml.Generator.Other; // Separate namespace for verification.

[TomlSerializedObject]
internal partial struct TestStruct
{
    [TomlValueOnSerialized]
    public int Value { get; set; }

    [TomlValueOnSerialized]
    public string Str { get; set; }
}


[TomlSerializedObject]
internal partial class TypeTable3
{
    [TomlValueOnSerialized]
    public string Value { get; set; }
}

[TomlSerializedObject]
internal partial class TypeTableB
{
    [TomlValueOnSerialized]
    public TypeTableC TableC { get; set; }

    [TomlValueOnSerialized]
    public string Value { get; set; }

    [TomlValueOnSerialized]
    public List<TypeTableE> TableECollection { get; set; }
}
