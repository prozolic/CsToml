
namespace CsToml.Generator;

internal enum CsTomlTypeKind : byte
{
    Primitive,
    Collection,
    TableOrArrayOfTables,
    ArrayOfTables,
    Unknown,
    Error
}

