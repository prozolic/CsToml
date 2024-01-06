using CsToml.Formatter;
using CsToml.Utility;
using System;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlBool: {Value}")]
internal class CsTomlBool : CsTomlValue
{
    public bool Value { get; set; }

    public CsTomlBool(bool value) : base(CsTomlType.Boolean)
    {
        this.Value = value;
    }

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        BoolFormatter.Serialize(ref writer, Value);
        return true;
    }

}

