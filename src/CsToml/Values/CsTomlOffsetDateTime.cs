
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlOffsetDateTime: {Value}")]
internal class CsTomlOffsetDateTime : CsTomlValue
{
    public DateTimeOffset Value { get; set; }

    public CsTomlOffsetDateTime(DateTimeOffset value, bool byNumber) : base(byNumber ? CsTomlType.OffsetDateTimeByNumber : CsTomlType.OffsetDateTime)
    {
        this.Value = value;
    }

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        DateTimeOffsetFormatter.Serialize(ref writer, Value);
        return true;
    }
}
