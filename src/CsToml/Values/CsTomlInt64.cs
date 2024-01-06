
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlInt: {Value}")]
internal class CsTomlInt64 : CsTomlValue
{
    public long Value { get; set; } // 64-bit signed integers

    public CsTomlInt64(long value) : base(CsTomlType.Integar)
    {
        this.Value = value;
    }

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        Int64Formatter.Serialize(ref writer, Value);
        return true;
    }
}

