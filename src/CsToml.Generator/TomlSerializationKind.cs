
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
    Dictionary,
    ArrayOfITomlSerializedObject,
    CollectionOfITomlSerializedObject,
    TomlSerializedObject,
    Error
}
