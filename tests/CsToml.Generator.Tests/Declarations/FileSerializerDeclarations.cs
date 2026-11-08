#pragma warning disable CS8618

namespace CsToml.Generator.Tests;

// Used only through CsTomlFileSerializer, so NativeAOT keeps their Register() only if its T is annotated.
[TomlSerializedObject]
internal partial class FileSerializerClass
{
    [TomlValueOnSerialized]
    public int Value { get; set; }

    [TomlValueOnSerialized]
    public string Name { get; set; }
}

[TomlSerializedObject]
internal partial struct FileSerializerStruct
{
    [TomlValueOnSerialized]
    public long Value { get; set; }

    [TomlValueOnSerialized]
    public long?[] Values { get; set; }
}
