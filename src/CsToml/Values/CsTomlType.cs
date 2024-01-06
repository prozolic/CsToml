
namespace CsToml.Values;

public enum CsTomlType : byte
{
    None = 0,
    String = 1,
    Integar = 2,
    Float = 3,
    Boolean = 4,
    OffsetDateTime = 5,
    OffsetDateTimeByNumber = 6,
    LocalDateTime = 7,
    LocalDate = 8,
    LocalTime = 9,
    Array = 10,
    Table = 11,
    InlineTable = 12,
}