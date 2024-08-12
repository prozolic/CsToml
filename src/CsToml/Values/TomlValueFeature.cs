
namespace CsToml.Values;

[Flags]
public enum TomlValueFeature
{
    None = 0,
    String = 1,
    Int64 = 1 << 1,
    Double = 1 << 2,
    Bool = 1 << 3,
    Number = 1 << 4,
    DateTime = 1 << 5,
    DateTimeOffset = 1 << 6,
    DateOnly = 1 << 7,
    TimeOnly = 1 << 8,
    Object = 1 << 9,
    Array = 1 << 10,
    Table = 1 << 11,
    InlineTable = 1 << 12,
}