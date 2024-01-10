
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlLocalDateTime: {Value}")]
internal class CsTomlLocalDateTime(DateTime value) : CsTomlValue(CsTomlType.LocalDateTime)
{
    public DateTime Value { get; set; } = value;

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        DateTimeFormatter.Serialize(ref writer, Value);
        return true;
    }
}
