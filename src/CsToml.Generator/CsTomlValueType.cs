
namespace CsToml.Generator;

internal enum CsTomlValueType : byte
{
    KeyValue = 0,
    Array = 1,
    Table = 2,
    ArrayOfTables = 3,
    InlineTable = 4
}
