using CsToml.Formatter;
using CsToml.Utility;
using System;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlBool: {Value}")]
internal class CsTomlBool(bool value) : CsTomlValue(CsTomlType.Boolean)
{
    public bool Value { get; set; } = value;

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        BoolFormatter.Serialize(ref writer, Value);
        return true;
    }

}

