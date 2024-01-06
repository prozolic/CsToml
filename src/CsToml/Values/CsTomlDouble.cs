
using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlDouble: {Value}")]
internal class CsTomlDouble : CsTomlValue
{
    public readonly static CsTomlDouble Inf = new(CsTomlSyntax.Double.Inf, DoubleKind.Inf);
    public readonly static CsTomlDouble NInf = new (CsTomlSyntax.Double.NInf, DoubleKind.NInf);
    public readonly static CsTomlDouble Nan = new (CsTomlSyntax.Double.Nan, DoubleKind.Nan);
    public readonly static CsTomlDouble PNan = new(CsTomlSyntax.Double.Inf, DoubleKind.PNan);

    internal enum DoubleKind : byte
    {
        Normal,
        Inf,
        NInf,
        Nan,
        PNan
    }

    public double Value { get; set; }

    internal DoubleKind Kind { get; }

    public CsTomlDouble(double value, DoubleKind kind = DoubleKind.Normal) : base(CsTomlType.Float)
    {
        Value = value;
        Kind = kind;
    }

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        if (Kind == DoubleKind.Normal)
        {
            DoubleFormatter.Serialize(ref writer, Value);
            return true;
        }

        switch(Kind)
        {
            case DoubleKind.Inf:
                writer.Write("inf"u8);
                break;
            case DoubleKind.NInf:
                writer.Write("-inf"u8);
                break;
            case DoubleKind.Nan:
                writer.Write("nan"u8);
                break;
            case DoubleKind.PNan:
                writer.Write("-nan"u8);
                break;
            default:
                return false;
        }
        return true;
    }
}

