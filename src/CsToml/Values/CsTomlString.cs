
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlString: {Value}")]
internal class CsTomlString(ReadOnlySpan<byte> value, CsTomlString.CsTomlStringType type = CsTomlString.CsTomlStringType.Basic) : CsTomlValue(CsTomlType.String)
{
    public static readonly char doubleQuoted = '"';
    public static readonly char singleQuoted = '\'';

    public enum CsTomlStringType : byte
    {
        Unquoted,
        Basic,
        MultiLineBasic,
        Literal,
        MultiLineLiteral
    }

    public CsTomlStringType StringType { get; } = type;

    public Utf8FixString Value { get; } = new Utf8FixString(value);

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        if (StringType == CsTomlStringType.Unquoted)
        {
            if (Value.Length == 0)
            {
                return false;
            }
            writer.Write(Value.BytesSpan);
            return true;
        }
        switch (StringType)
        {
            case CsTomlStringType.Basic:
            case CsTomlStringType.MultiLineBasic:
            case CsTomlStringType.Literal:
            case CsTomlStringType.MultiLineLiteral:
                break;
        }

        return true;
    }
}
