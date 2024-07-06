
namespace CsToml.Values;

[Flags]
internal enum CsTomlTableNodeType : byte
{
    None = 0,
    GroupingProperty = 1 << 0,
    TableHeaderProperty = 1 << 1,
    TableHeaderDefinitionPosition = 1 << 2,
    ArrayOfTablesHeaderProperty = 1 << 3,
    ArrayOfTablesHeaderDefinitionPosition = 1 << 4,
}