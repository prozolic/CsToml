﻿
using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Text.Unicode;

namespace CsToml.Values;

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlUnquotedString(string value) : TomlString(value)
{
    public new static readonly TomlUnquotedString Empty = new TomlUnquotedString(string.Empty);

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            Utf8Helper.FromUtf16(bufferWriter, Utf16String.AsSpan());
            if (Utf16String.Length > 0)
            {
                writer.WriteBytes(bufferWriter.WrittenSpan);
            }
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

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            Utf8Helper.FromUtf16(bufferWriter, Utf16String.AsSpan());
            ToTomlBasicString(ref writer, bufferWriter.WrittenSpan);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }

    internal static void ToTomlBasicString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> byteSpan)
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
    }

}

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlMultiLineBasicString(string value) : TomlString(value)
{
    public new static readonly TomlMultiLineBasicString Empty = new TomlMultiLineBasicString(string.Empty);

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            Utf8Helper.FromUtf16(bufferWriter, Utf16String.AsSpan());
            ToTomlMultiLineBasicString(ref writer, bufferWriter.WrittenSpan);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }

    internal static void ToTomlMultiLineBasicString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> byteSpan)
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
    }

}

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlLiteralString(string value) : TomlString(value)
{
    public new static readonly TomlLiteralString Empty = new TomlLiteralString(string.Empty);

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            Utf8Helper.FromUtf16(bufferWriter, Utf16String.AsSpan());
            ToTomlLiteralString(ref writer, bufferWriter.WrittenSpan);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }

    internal static void ToTomlLiteralString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> byteSpan)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteBytes("'"u8);
        writer.WriteBytes(byteSpan);
        writer.WriteBytes("'"u8);
    }

}

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlMultiLineLiteralString(string value) : TomlString(value)
{
    public new static readonly TomlMultiLineLiteralString Empty = new TomlMultiLineLiteralString(string.Empty);

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            Utf8Helper.FromUtf16(bufferWriter, Utf16String.AsSpan());
            writer.WriteBytes("'''"u8);
            writer.WriteBytes(bufferWriter.WrittenSpan);
            writer.WriteBytes("'''"u8);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }
}

[DebuggerDisplay("{Utf16String}")]
internal abstract partial class TomlString(string value) : TomlValue, ITomlStringParser<TomlString>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly string value = value;

    public override bool HasValue => true;

    public override TomlValueType Type => TomlValueType.String;

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

    public override string ToString(string? format, IFormatProvider? formatProvider) => Utf16String;

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        var status = Utf8.FromUtf16(Utf16String.AsSpan(), utf8Destination, out var bytesRead, out bytesWritten, replaceInvalidSequences: false);
        return status == OperationStatus.Done;
    }

    public override string ToString() => Utf16String;
}

internal enum CsTomlStringType : byte
{
    Unquoted,
    Basic,
    MultiLineBasic,
    Literal,
    MultiLineLiteral
}
