
using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Text.Unicode;

namespace CsToml.Values;

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlUnquotedString(string value) : TomlString(value)
{
    public new static readonly TomlUnquotedString Empty = new TomlUnquotedString(string.Empty);

    internal override bool ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            Utf8Helper.FromUtf16(bufferWriter, Utf16String.AsSpan());
            if (value.Length > 0)
            {
                writer.WriteBytes(bufferWriter.WrittenSpan);
                return true;
            }

            return false;
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }
}

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlBasicString(string value) : TomlString(value)
{
    public new static readonly TomlBasicString Empty = new TomlBasicString(string.Empty);

    internal override bool ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            Utf8Helper.FromUtf16(bufferWriter, Utf16String.AsSpan());
            return ToTomlBasicString(ref writer, bufferWriter.WrittenSpan);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }

    internal static bool ToTomlBasicString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> byteSpan)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteBytes("\""u8);

        for (int i = 0; i < byteSpan.Length; i++)
        {
            var ch = byteSpan[i];
            switch (ch)
            {
                case TomlCodes.Symbol.DOUBLEQUOTED:
                    writer.Write(TomlCodes.Symbol.BACKSLASH);
                    writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                    continue;
                case TomlCodes.Symbol.BACKSLASH:
                    writer.Write(TomlCodes.Symbol.BACKSLASH);
                    writer.Write(TomlCodes.Symbol.BACKSLASH);
                    continue;
                case TomlCodes.Symbol.BACKSPACE:
                    writer.Write(TomlCodes.Symbol.BACKSLASH);
                    writer.Write(TomlCodes.Alphabet.b);
                    continue;
                case TomlCodes.Symbol.TAB:
                    writer.Write(TomlCodes.Symbol.BACKSLASH);
                    writer.Write(TomlCodes.Alphabet.t);
                    continue;
                case TomlCodes.Symbol.LINEFEED:
                    writer.Write(TomlCodes.Symbol.BACKSLASH);
                    writer.Write(TomlCodes.Alphabet.n);
                    continue;
                case TomlCodes.Symbol.FORMFEED:
                    writer.Write(TomlCodes.Symbol.BACKSLASH);
                    writer.Write(TomlCodes.Alphabet.f);
                    continue;
                case TomlCodes.Symbol.CARRIAGE:
                    writer.Write(TomlCodes.Symbol.BACKSLASH);
                    writer.Write(TomlCodes.Alphabet.r);
                    continue;
                default:
                    writer.Write(ch);
                    continue;
            }

        }

        writer.WriteBytes("\""u8);
        return true;
    }

}

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlMultiLineBasicString(string value) : TomlString(value)
{
    public new static readonly TomlMultiLineBasicString Empty = new TomlMultiLineBasicString(string.Empty);

    internal override bool ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            Utf8Helper.FromUtf16(bufferWriter, Utf16String.AsSpan());
            return ToTomlMultiLineBasicString(ref writer, bufferWriter.WrittenSpan);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }

    internal static bool ToTomlMultiLineBasicString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> byteSpan)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteBytes("\"\"\""u8);

        for (int i = 0; i < byteSpan.Length; i++)
        {
            var ch = byteSpan[i];
            switch (ch)
            {
                case TomlCodes.Symbol.DOUBLEQUOTED:
                    writer.Write(TomlCodes.Symbol.BACKSLASH);
                    writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                    continue;
                case TomlCodes.Symbol.BACKSLASH:
                    writer.Write(TomlCodes.Symbol.BACKSLASH);
                    writer.Write(TomlCodes.Symbol.BACKSLASH);
                    continue;
                case TomlCodes.Symbol.BACKSPACE:
                    writer.Write(TomlCodes.Symbol.BACKSLASH);
                    writer.Write(TomlCodes.Alphabet.b);
                    continue;
                case TomlCodes.Symbol.TAB:
                    writer.Write(TomlCodes.Symbol.BACKSLASH);
                    writer.Write(TomlCodes.Alphabet.t);
                    continue;
                case TomlCodes.Symbol.LINEFEED:
                    writer.Write(TomlCodes.Symbol.BACKSLASH);
                    writer.Write(TomlCodes.Alphabet.n);
                    continue;
                case TomlCodes.Symbol.FORMFEED:
                    writer.Write(TomlCodes.Symbol.BACKSLASH);
                    writer.Write(TomlCodes.Alphabet.f);
                    continue;
                case TomlCodes.Symbol.CARRIAGE:
                    writer.Write(TomlCodes.Symbol.BACKSLASH);
                    writer.Write(TomlCodes.Alphabet.r);
                    continue;
                default:
                    writer.Write(ch);
                    continue;
            }
        }

        writer.WriteBytes("\"\"\""u8);
        return true;
    }

}

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlLiteralString(string value) : TomlString(value)
{
    public new static readonly TomlLiteralString Empty = new TomlLiteralString(string.Empty);

    internal override bool ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            Utf8Helper.FromUtf16(bufferWriter, Utf16String.AsSpan());
            return ToTomlLiteralString(ref writer, bufferWriter.WrittenSpan);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }

    internal static bool ToTomlLiteralString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> byteSpan)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteBytes("'"u8);
        writer.WriteBytes(byteSpan);
        writer.WriteBytes("'"u8);
        return true;
    }

}

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlMultiLineLiteralString(string value) : TomlString(value)
{
    public new static readonly TomlMultiLineLiteralString Empty = new TomlMultiLineLiteralString(string.Empty);

    internal override bool ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            Utf8Helper.FromUtf16(bufferWriter, Utf16String.AsSpan());
            return ToTomlMultiLineLiteralString(ref writer, bufferWriter.WrittenSpan);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }

    internal static bool ToTomlMultiLineLiteralString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> byteSpan)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteBytes("'''"u8);
        writer.WriteBytes(byteSpan);
        writer.WriteBytes("'''"u8);
        return true;
    }
}

[DebuggerDisplay("{Utf16String}")]
internal abstract partial class TomlString(string value) : TomlValue, ITomlStringParser<TomlString>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly string value = value;

    public override bool HasValue => true;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public string Utf16String => value;

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (destination.Length < value.Length)
        {
            charsWritten = 0;
            return false;
        }
        value.TryCopyTo(destination);
        charsWritten = value.Length;
        return true;
    }

    public override string ToString(string? format, IFormatProvider? formatProvider)
        => GetString();

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        var status = Utf8.FromUtf16(Utf16String.AsSpan(), utf8Destination, out var bytesRead, out bytesWritten, replaceInvalidSequences: false);
        return status == OperationStatus.Done;
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
