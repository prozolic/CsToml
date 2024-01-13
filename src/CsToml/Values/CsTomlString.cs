
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
                return ToTomlBasicString(ref writer);
            case CsTomlStringType.MultiLineBasic:
                return ToTomlMultiLineBasicString(ref writer);
            case CsTomlStringType.Literal:
                return ToTomlLiteralString(ref writer);
            case CsTomlStringType.MultiLineLiteral:
                return ToTomlMultiLineLiteralString(ref writer);
        }

        return false;
    }

    private bool ToTomlBasicString(ref Utf8Writer writer)
    {
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);

        var byteSpan = Value.BytesSpan;
        for (int i = 0; i < byteSpan.Length; i++)
        {
            var ch = byteSpan[i];
            switch (ch)
            {
                case CsTomlSyntax.Symbol.DOUBLEQUOTED:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
                    continue;
                case CsTomlSyntax.Symbol.BACKSLASH:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    continue;
                case CsTomlSyntax.Symbol.BACKSPACE:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.b);
                    continue;
                case CsTomlSyntax.Symbol.TAB:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.t);
                    continue;
                case CsTomlSyntax.Symbol.LINEFEED:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.n);
                    continue;
                case CsTomlSyntax.Symbol.FORMFEED:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.f);
                    continue;
                case CsTomlSyntax.Symbol.CARRIAGE:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.r);
                    continue;
                default:
                    writer.Write(ch);
                    continue;
            }

        }

        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
        return true;
    }

    private bool ToTomlMultiLineBasicString(ref Utf8Writer writer)
    {
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);

        var byteSpan = Value.BytesSpan;
        for (int i = 0; i < byteSpan.Length;i++)
        {
            var ch = byteSpan[i];
            switch (ch)
            {
                case CsTomlSyntax.Symbol.DOUBLEQUOTED:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
                    continue;
                case CsTomlSyntax.Symbol.BACKSLASH:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    continue;
                case CsTomlSyntax.Symbol.BACKSPACE:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.b);
                    continue;
                case CsTomlSyntax.Symbol.TAB:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.t);
                    continue;
                case CsTomlSyntax.Symbol.LINEFEED:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.n);
                    continue;
                case CsTomlSyntax.Symbol.FORMFEED:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.f);
                    continue;
                case CsTomlSyntax.Symbol.CARRIAGE:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.r);
                    continue;
                default:
                    writer.Write(ch);
                    continue;
            }
        }

        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
        return true;
    }

    private bool ToTomlLiteralString(ref Utf8Writer writer)
    {
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(Value.BytesSpan);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        return true;
    }

    private bool ToTomlMultiLineLiteralString(ref Utf8Writer writer)
    {
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(Value.BytesSpan);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        return true;
    }
}
