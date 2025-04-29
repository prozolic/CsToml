
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
    Object,
    ArrayOfITomlSerializedObject,
    CollectionOfITomlSerializedObject,
    Dictionary,
    TomlSerializedObject,
    Error
}
