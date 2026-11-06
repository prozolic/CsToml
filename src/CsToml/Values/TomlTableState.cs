
namespace CsToml.Values;

[Flags]
internal enum TomlTableState : byte
{
    None = 0,
    Header = 1 << 0,
    Defined = 1 << 1,
    ViaArrayOfTables = 1 << 2,
}
