using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;

namespace CsToml.Values;

internal interface ITomlStringParser<T>
    where T : TomlValue
{
    static abstract T Parse(ReadOnlySpan<byte> value);
}

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlUnquotedString(string value) : TomlString(value), ITomlStringParser<TomlUnquotedString>
{
    public static readonly TomlUnquotedString EmptyString = new (string.Empty);

    static TomlUnquotedString ITomlStringParser<TomlUnquotedString>.Parse(ReadOnlySpan<byte> value)
    {
        if (value.Length == 0)
        {
            return EmptyString;
        }
        return new TomlUnquotedString(Utf8Helper.ToUtf16(value));
    }

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        var valueSpan = value.AsSpan();
        if (valueSpan.Length == 0)
        {
            return;
        }

        // buffer size to 3 times worst-case (UTF16 -> UTF8)
        var maxBufferSize = (valueSpan.Length + 1) * 3;
        if (maxBufferSize <= 1024)
        {
            Span<byte> dest = stackalloc byte[maxBufferSize];
            Utf8Helper.FromUtf16(valueSpan, dest, out var _, out var bytesWritten);

            if (bytesWritten > 0)
            {
                ref byte destReference = ref MemoryMarshal.GetReference(dest);
                unsafe
                {
                    fixed (byte* ptr = &destReference)
                    {
                        var writtenSpan = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(ptr), bytesWritten);
                        writer.WriteBytes(writtenSpan);
                    }
                }
            }
        }
        else
        {
            var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
            try
            {
                Utf8Helper.FromUtf16(bufferWriter, valueSpan);
                if (valueSpan.Length > 0)
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
}

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlBasicString(string value) : TomlString(value), ITomlStringParser<TomlBasicString>
{
    public static readonly TomlBasicString EmptyString = new(string.Empty);

    static TomlBasicString ITomlStringParser<TomlBasicString>.Parse(ReadOnlySpan<byte> value)
    {
        if (value.Length == 0)
        {
            return EmptyString;
        }
        return new TomlBasicString(Utf8Helper.ToUtf16(value));
    }

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        var valueSpan = value.AsSpan();
        if (valueSpan.Length == 0)
        {
            writer.WriteBytes("\"\""u8);
            return;
        }

        // buffer size to 3 times worst-case (UTF16 -> UTF8)
        var maxBufferSize = (valueSpan.Length + 1) * 3;
        if (maxBufferSize <= 1024)
        {
            Span<byte> dest = stackalloc byte[maxBufferSize];
            Utf8Helper.FromUtf16(valueSpan, dest, out var _, out var bytesWritten);

            if (bytesWritten > 0)
            {
                ref byte destReference = ref MemoryMarshal.GetReference(dest);
                unsafe
                {
                    fixed (byte* ptr = &destReference)
                    {
                        var writtenSpan = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(ptr), bytesWritten);
                        ToTomlBasicString(ref writer, writtenSpan);
                    }
                }
            }
        }
        else
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
    }

    internal static void ToTomlBasicString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> byteSpan)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteBytes("\""u8);

        var index = byteSpan.IndexOfAny(EscapedChars);
        if (index < 0)
        {
            writer.WriteBytes(byteSpan);
        }
        else
        {
            if (index > 0)
            {
                writer.WriteBytes(byteSpan.Slice(0, index));
                byteSpan = byteSpan.Slice(index);
            }

            for (index = 0; index < byteSpan.Length; index++)
            {
                var ch = byteSpan[index];
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
                        if (TomlCodes.IsEscape(ch))
                        {
                            WriteUnicodeEscape(ref writer, ch);
                        }
                        else
                        {
                            writer.Write(ch);
                        }
                        continue;
                }

            }
        }

        writer.WriteBytes("\""u8);
    }

}

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlMultiLineBasicString(string value) : TomlString(value), ITomlStringParser<TomlMultiLineBasicString>
{
    public static readonly TomlMultiLineBasicString EmptyString = new(string.Empty);

    static TomlMultiLineBasicString ITomlStringParser<TomlMultiLineBasicString>.Parse(ReadOnlySpan<byte> value)
    {
        if (value.Length == 0)
        {
            return EmptyString;
        }
        return new TomlMultiLineBasicString(Utf8Helper.ToUtf16(value));
    }

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        var valueSpan = value.AsSpan();

        if (valueSpan.Length == 0)
        {
            writer.WriteBytes("\"\"\"\"\"\""u8);
            return;
        }

        // buffer size to 3 times worst-case (UTF16 -> UTF8)
        var maxBufferSize = (valueSpan.Length + 1) * 3;
        if (maxBufferSize <= 1024)
        {
            Span<byte> dest = stackalloc byte[maxBufferSize];
            Utf8Helper.FromUtf16(valueSpan, dest, out var _, out var bytesWritten);

            if (bytesWritten > 0)
            {
                ref byte destReference = ref MemoryMarshal.GetReference(dest);
                unsafe
                {
                    fixed (byte* ptr = &destReference)
                    {
                        var writtenSpan = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(ptr), bytesWritten);
                        ToTomlMultiLineBasicString(ref writer, writtenSpan);
                    }
                }
            }
        }
        else
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
    }

    internal static void ToTomlMultiLineBasicString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> byteSpan)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteBytes("\"\"\""u8);

        var index = byteSpan.IndexOfAny(EscapedChars);
        if (index < 0)
        {
            writer.WriteBytes(byteSpan);
        }
        else
        {
            if (index > 0)
            {
                writer.WriteBytes(byteSpan.Slice(0, index));
                byteSpan = byteSpan.Slice(index);
            }

            for (index = 0; index < byteSpan.Length; index++)
            {
                var ch = byteSpan[index];
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
                        if (TomlCodes.IsEscape(ch))
                        {
                            WriteUnicodeEscape(ref writer, ch);
                        }
                        else
                        {
                            writer.Write(ch);
                        }
                        continue;
                }
            }

        }

        writer.WriteBytes("\"\"\""u8);
    }

}

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlLiteralString(string value) : TomlString(value), ITomlStringParser<TomlLiteralString>
{
    public static readonly TomlLiteralString EmptyString = new (string.Empty);

    static TomlLiteralString ITomlStringParser<TomlLiteralString>.Parse(ReadOnlySpan<byte> value)
    {
        if (value.Length == 0)
        {
            return EmptyString;
        }
        return new TomlLiteralString(Utf8Helper.ToUtf16(value));
    }

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        var valueSpan = value.AsSpan();
        if (valueSpan.Length == 0)
        {
            writer.WriteBytes("''"u8);
            return;
        }

        // buffer size to 3 times worst-case (UTF16 -> UTF8)
        var maxBufferSize = (valueSpan.Length + 1) * 3;
        if (maxBufferSize <= 1024)
        {
            Span<byte> dest = stackalloc byte[maxBufferSize];
            Utf8Helper.FromUtf16(valueSpan, dest, out var _, out var bytesWritten);

            if (bytesWritten > 0)
            {
                ref byte destReference = ref MemoryMarshal.GetReference(dest);
                unsafe
                {
                    fixed (byte* ptr = &destReference)
                    {
                        var writtenSpan = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(ptr), bytesWritten);
                        ToTomlLiteralString(ref writer, writtenSpan);
                    }
                }
            }
        }
        else
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
internal sealed class TomlMultiLineLiteralString(string value) : TomlString(value), ITomlStringParser<TomlMultiLineLiteralString>
{
    public static readonly TomlMultiLineLiteralString EmptyString = new(string.Empty);

    static TomlMultiLineLiteralString ITomlStringParser<TomlMultiLineLiteralString>.Parse(ReadOnlySpan<byte> value)
    {
        if (value.Length == 0)
        {
            return EmptyString;
        }
        return new TomlMultiLineLiteralString(Utf8Helper.ToUtf16(value));
    }

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        var valueSpan = value.AsSpan();
        if (valueSpan.Length == 0)
        {
            writer.WriteBytes("''''''"u8);
            return;
        }

        // buffer size to 3 times worst-case (UTF16 -> UTF8)
        var maxBufferSize = (valueSpan.Length + 1) * 3;
        if (maxBufferSize <= 1024)
        {
            Span<byte> dest = stackalloc byte[maxBufferSize];
            Utf8Helper.FromUtf16(valueSpan, dest, out var _, out var bytesWritten);

            if (bytesWritten > 0)
            {
                ref byte destReference = ref MemoryMarshal.GetReference(dest);
                unsafe
                {
                    fixed (byte* ptr = &destReference)
                    {
                        var writtenSpan = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(ptr), bytesWritten);
                        ToTomlMultiLineLiteralString(ref writer, writtenSpan);
                    }
                }
            }
        }
        else
        {

            var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
            try
            {
                Utf8Helper.FromUtf16(bufferWriter, Utf16String.AsSpan());
                ToTomlMultiLineLiteralString(ref writer, bufferWriter.WrittenSpan);
            }
            finally
            {
                RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
            }
        }
    }

    internal static void ToTomlMultiLineLiteralString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> byteSpan)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteBytes("'''"u8);
        writer.WriteBytes(byteSpan);
        writer.WriteBytes("'''"u8);
    }
}

[DebuggerDisplay("{Utf16String}")]
internal abstract partial class TomlString(string value) : TomlValue()
{
    private static ReadOnlySpan<byte> EscapedCharBytes =>
    [
        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
        0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
        0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
        0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
        0x22, 0x5C, 0x7F,
    ];
    protected static readonly SearchValues<byte> EscapedChars = SearchValues.Create(EscapedCharBytes);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void WriteUnicodeEscape<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, byte ch)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteBytes("\\u00"u8);
        writer.Write((byte)ToHexChar((ch >> 4) & 0xF));
        writer.Write((byte)ToHexChar(ch & 0xF));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static char ToHexChar(int nibble) => (char)(nibble < 10 ? '0' + nibble : 'a' + nibble - 10);

    protected readonly string value = value;

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

internal enum TomlStringType : byte
{
    Unquoted,
    Basic,
    MultiLineBasic,
    Literal,
    MultiLineLiteral
}
