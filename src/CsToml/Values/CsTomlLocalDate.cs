
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlLocalDate: {Value}")]
internal class CsTomlLocalDate(DateOnly value) : CsTomlValue(CsTomlType.LocalDate)
{
    public DateOnly Value { get; set; } = value;

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        DateOnlyFormatter.Serialize(ref writer, Value);
        return true;
    }
}
