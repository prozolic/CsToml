
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

    // Inline format or Array of tables format
    TomlSerializedObjectArray,
    TomlSerializedObjectCollection,

    // Inline format or table format
    TypeParameter,
    NullableStructWithTypeParameter,
    Dictionary,

    // Union or IUnionMembers
    Union,
    TomlSerializedObject,
    TomlSerializedObjectArrayForHeaderStyle,
    TomlSerializedObjectCollectionForHeaderStyle,
    Error
}
