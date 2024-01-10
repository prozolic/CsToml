
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlLocalTime: {Value}")]
internal class CsTomlLocalTime(TimeOnly value) : CsTomlValue(CsTomlType.LocalTime)
{
    public TimeOnly Value { get; set; } = value;

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        TimeOnlyFormatter.Serialize(ref writer, Value);
        return true;
    }
}
