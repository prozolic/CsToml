
namespace CsToml.Generator;

internal enum TomlSerializationKind
{
    Object,
    Primitive,
    Struct,
    Class,
    Interface,
    Enum,
    Array,
    Collection,
    Dictionary,
    TomlSerializedObject,
    ArrayOfITomlSerializedObject,
    NotAvailable
}
