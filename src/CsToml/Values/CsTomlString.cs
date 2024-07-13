using CsToml.Formatter;
using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Text.Unicode;

namespace CsToml.Values;

[DebuggerDisplay("{Utf16String}")]
internal partial class CsTomlString :
    CsTomlValue,
    ICsTomlStringCreator<CsTomlString>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string utf16String;

    public override bool HasValue => true;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public CsTomlStringType TomlStringType { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public string Value => utf16String;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public string Utf16String => utf16String;

    public static CsTomlString CreateString(ReadOnlySpan<byte> value, CsTomlStringType type = CsTomlStringType.Basic)
    {
        return new CsTomlString(value, type);
    }

    public static CsTomlString CreateEmpty(CsTomlStringType type = CsTomlStringType.Basic)
    {
        return new CsTomlString(string.Empty, type);
    }

    public CsTomlString(ReadOnlySpan<byte> value, CsTomlStringType type = CsTomlStringType.Basic) : base()
    {
        TomlStringType = type;
        var tempReader = new Utf8Reader(value);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out utf16String);
    }

    private CsTomlString(string value, CsTomlStringType type = CsTomlStringType.Basic) : base()
    {
        TomlStringType = type;
        utf16String = value;
    }

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (destination.Length < Value.Length)
        {
            charsWritten = 0;
            return false;
        }
        Value.TryCopyTo(destination);
        charsWritten = Value.Length;
        return true;
    }

    public override string ToString(string? format, IFormatProvider? formatProvider)
        => GetString();

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        var status = Utf8.FromUtf16(Utf16String.AsSpan(), utf8Destination, out var bytesRead, out bytesWritten, replaceInvalidSequences: false);
        return status == OperationStatus.Done;
    }

    internal override bool ToTomlString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer)
    {
        var bufferWriter = new ArrayPoolBufferWriter<byte>(128);
        var utf8Writer = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref bufferWriter);
        ValueFormatter.Serialize(ref utf8Writer, Utf16String.AsSpan());

        try
        {
            switch (TomlStringType)
            {
                case CsTomlStringType.Basic:
                    return ToTomlBasicString(ref writer, bufferWriter.WrittenSpan);
                case CsTomlStringType.MultiLineBasic:
                    return ToTomlMultiLineBasicString(ref writer, bufferWriter.WrittenSpan);
                case CsTomlStringType.Literal:
                    return ToTomlLiteralString(ref writer, bufferWriter.WrittenSpan);
                case CsTomlStringType.MultiLineLiteral:
                    return ToTomlMultiLineLiteralString(ref writer, bufferWriter.WrittenSpan);
                case CsTomlStringType.Unquoted:
                    {
                        if (Value.Length > 0)
                        {
                            writer.Write(bufferWriter.WrittenSpan);
                            return true;
                        }
                        break;
                    }
            }
        }
        finally
        {
            using (bufferWriter) { }
        }

        return false;
    }

    internal static bool ToTomlBasicString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, ReadOnlySpan<byte> byteSpan)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);

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
                    writer.Write(CsTomlSyntax.Alphabet.b);
                    continue;
                case CsTomlSyntax.Symbol.TAB:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.Alphabet.t);
                    continue;
                case CsTomlSyntax.Symbol.LINEFEED:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.Alphabet.n);
                    continue;
                case CsTomlSyntax.Symbol.FORMFEED:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.Alphabet.f);
                    continue;
                case CsTomlSyntax.Symbol.CARRIAGE:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.Alphabet.r);
                    continue;
                default:
                    writer.Write(ch);
                    continue;
            }

        }

        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
        return true;
    }

    internal static bool ToTomlMultiLineBasicString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, ReadOnlySpan<byte> byteSpan)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);

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
                    writer.Write(CsTomlSyntax.Alphabet.b);
                    continue;
                case CsTomlSyntax.Symbol.TAB:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.Alphabet.t);
                    continue;
                case CsTomlSyntax.Symbol.LINEFEED:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.Alphabet.n);
                    continue;
                case CsTomlSyntax.Symbol.FORMFEED:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.Alphabet.f);
                    continue;
                case CsTomlSyntax.Symbol.CARRIAGE:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.Alphabet.r);
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

    internal static bool ToTomlLiteralString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, ReadOnlySpan<byte> byteSpan)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(byteSpan);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        return true;
    }

    internal static bool ToTomlMultiLineLiteralString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, ReadOnlySpan<byte> byteSpan)
         where TBufferWriter : IBufferWriter<byte>
    {
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(byteSpan);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        return true;
    }
}

internal enum CsTomlStringType : byte
{
    Unquoted,
    Basic,
    MultiLineBasic,
    Literal,
    MultiLineLiteral
}
