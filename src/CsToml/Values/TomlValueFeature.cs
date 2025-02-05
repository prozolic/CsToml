
namespace CsToml.Values;

[Flags]
public enum TomlValueFeature
{
    None = 0,
    String = 1,
    Int64 = 1 << 1,
    Double = 1 << 2,
    Boolean = 1 << 3,
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

public enum TomlValueType
{
    Key = -1, // This is for internal use only.
    Empty = 0, // Rarely used.
    String = 1,
    Integer = 2,
    Float = 3,
    Boolean = 4,
    OffsetDateTime = 5,
    LocalDateTime = 6,
    LocalDate = 7,
    LocalTime = 8,
    Array = 9,
    Table = 10, // Rarely used.
    InlineTable = 11, // Rarely used.
}