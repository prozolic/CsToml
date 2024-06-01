using CsToml.Formatter;
using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Text.Unicode;

namespace CsToml.Values;

[DebuggerDisplay("{Utf16String}")]
internal partial class CsTomlString : CsTomlValue
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    protected byte[] bytes;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int Length => Value.Length;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public string Utf16String
    {
        get
        {
            var tempReader = new Utf8Reader(Value);
            ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out string value);
            return value;
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public ReadOnlySpan<byte> Value => bytes.AsSpan();

    public override bool HasValue => true;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public CsTomlStringType TomlStringType { get; }

    public CsTomlString(ReadOnlySpan<byte> value, CsTomlStringType type = CsTomlStringType.Basic) : base()
    {
        bytes = value.ToArray();
        TomlStringType = type;
    }

    public CsTomlString(byte[] value, CsTomlStringType type = CsTomlStringType.Basic) : base()
    {
        bytes = value;
        TomlStringType = type;
    }

    internal override bool ToTomlString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer)
    {
        switch (TomlStringType)
        {
            case CsTomlStringType.Basic:
                return ToTomlBasicString(ref writer);
            case CsTomlStringType.MultiLineBasic:
                return ToTomlMultiLineBasicString(ref writer);
            case CsTomlStringType.Literal:
                return ToTomlLiteralString(ref writer);
            case CsTomlStringType.MultiLineLiteral:
                return ToTomlMultiLineLiteralString(ref writer);
            case CsTomlStringType.Unquoted:
                {
                    if (Length > 0)
                    {
                        writer.Write(Value);
                        return true;
                    }
                    break;
                }
        }

        return false;
    }

    public override string ToString()
        => GetString();

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        var status = Utf8.ToUtf16(Value, destination, out var bytesRead, out charsWritten, replaceInvalidSequences: false);
        return status == OperationStatus.Done;
    }

    public override string ToString(string? format, IFormatProvider? formatProvider) 
        => GetString();

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (utf8Destination.Length < Value.Length)
        {
            bytesWritten = 0;
            return false;
        }

        Value.TryCopyTo(utf8Destination);
        bytesWritten = Value.Length;
        return true;
    }

    private bool ToTomlBasicString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);

        var byteSpan = Value;
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

    private bool ToTomlMultiLineBasicString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);

        var byteSpan = Value;
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

    private bool ToTomlLiteralString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(Value);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        return true;
    }

    private bool ToTomlMultiLineLiteralString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer)
         where TBufferWriter : IBufferWriter<byte>
    {
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(Value);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        return true;
    }

    public enum CsTomlStringType : byte
    {
        Unquoted,
        Basic,
        MultiLineBasic,
        Literal,
        MultiLineLiteral
    }

}
