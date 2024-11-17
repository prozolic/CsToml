
namespace CsToml.Generator;

internal enum TomlSerializationKind
{
    Primitive,
    PrimitiveArray,
    PrimitiveCollection,
    Enum,
    Struct,
    Class,
    Interface,
    Dictionary,
    Object,
    ArrayOfITomlSerializedObject,
    TomlSerializedObject,
    Error
}
