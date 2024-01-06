
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlLocalDate: {Value}")]
internal class CsTomlLocalDate : CsTomlValue
{
    public DateOnly Value { get; set; }

    public CsTomlLocalDate(DateOnly value) : base(CsTomlType.LocalDate)
    {
        this.Value = value;
    }

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        DateOnlyFormatter.Serialize(ref writer, Value);
        return true;
    }
}
