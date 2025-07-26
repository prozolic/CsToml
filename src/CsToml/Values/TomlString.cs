
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
