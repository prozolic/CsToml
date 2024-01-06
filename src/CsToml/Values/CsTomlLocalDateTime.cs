
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlLocalDateTime: {Value}")]
internal class CsTomlLocalDateTime : CsTomlValue
{
    public DateTime Value { get; set; }

    public CsTomlLocalDateTime(DateTime value) : base(CsTomlType.LocalDateTime)
    {
        this.Value = value;
    }

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        DateTimeFormatter.Serialize(ref writer, Value);
        return true;
    }
}
