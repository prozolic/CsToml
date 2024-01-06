
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlString: {Value}")]
internal class CsTomlString : CsTomlValue
{
    public static readonly char doubleQuoted = '"';
    public static readonly char singleQuoted = '\'';

    public static CsTomlString Equal { get; } = new CsTomlString("="u8);

    public static CsTomlString NewLine { get; } = new CsTomlString("\n"u8);

    public static CsTomlString CrNewLine { get; } = new CsTomlString("\r\n"u8);

    public enum CsTomlStringType : byte
    {
        Unquoted,
        Basic,
        MultiLineBasic,
        Literal,
        MultiLineLiteral
    }

    public CsTomlStringType StringType { get; }

    public Utf8FixString Value { get;}

    public CsTomlString(ReadOnlySpan<byte> value, CsTomlStringType type = CsTomlStringType.Basic) : base(CsTomlType.String)
    {
        this.Value = new Utf8FixString(value);
        this.StringType = type;
    }

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
