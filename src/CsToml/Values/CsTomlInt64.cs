
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlInt: {Value}")]
internal class CsTomlInt64(long value) : CsTomlValue(CsTomlType.Integar)
{
    public long Value { get; set; } = value;

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        Int64Formatter.Serialize(ref writer, Value);
        return true;
    }
}

