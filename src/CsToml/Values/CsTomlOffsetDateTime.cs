
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlOffsetDateTime: {Value}")]
internal class CsTomlOffsetDateTime(DateTimeOffset value, bool byNumber) 
    : CsTomlValue(byNumber ? CsTomlType.OffsetDateTimeByNumber : CsTomlType.OffsetDateTime)
{
    public DateTimeOffset Value { get; set; } = value;

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        DateTimeOffsetFormatter.Serialize(ref writer, Value);
        return true;
    }
}
