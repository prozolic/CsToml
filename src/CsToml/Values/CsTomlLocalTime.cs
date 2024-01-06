
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlLocalTime: {Value}")]
internal class CsTomlLocalTime : CsTomlValue
{
    public TimeOnly Value { get; set; }

    public CsTomlLocalTime(TimeOnly value) : base(CsTomlType.LocalTime)
    {
        this.Value = value;
    }

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        TimeOnlyFormatter.Serialize(ref writer, Value);
        return true;
    }
}
