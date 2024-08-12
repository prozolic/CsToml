
namespace CsToml.Generator;

internal enum TomlTypeKind : byte
{
    Primitive,
    Collection,
    TableOrArrayOfTables,
    ArrayOfTables,
    Unknown,
    Error
}

