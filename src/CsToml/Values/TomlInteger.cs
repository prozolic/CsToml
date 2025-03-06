using CsToml.Error;
using System.Buffers.Text;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Values;

[DebuggerDisplay("{Value}")]
internal sealed partial class TomlInteger : TomlValue
{
    internal static readonly TomlInteger[] Cache = CreateCacheValue();

    private static TomlInteger[] CreateCacheValue()
    {
        var intCacheValues = new TomlInteger[10];
        for (int i = 0; i < intCacheValues.Length; i++)
        {
            intCacheValues[i] = new TomlInteger(i - 1);
        }

        return intCacheValues;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TomlInteger Create(long value)
    {
        if ((ulong)(value + 1) < (ulong)Cache.Length)
        {
            return Cache[value + 1];
        }
        return new TomlInteger(value);
    }

    public static TomlInteger Zero => Cache[1];

    public long Value { get; init; } 

    public override bool HasValue => true;

    public override TomlValueType Type => TomlValueType.Integer;

    private TomlInteger(long value)
    {
        this.Value = value;
    }

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        writer.WriteInt64(Value);
    }

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (format.Length == 0 && provider == null)
        {
            return Value.TryFormat(destination, out charsWritten, format, CultureInfo.InvariantCulture);
        }
        return Value.TryFormat(destination, out charsWritten, format, provider);
    }

    public override string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format) && formatProvider == null)
        {
            return ToString();
        }
        return Value.ToString(format, formatProvider);
    }

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (format.Length == 0 && provider == null)
        {
            return Value.TryFormat(utf8Destination, out bytesWritten, format, CultureInfo.InvariantCulture);
        }
        return Value.TryFormat(utf8Destination, out bytesWritten, format, provider);
    }

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TomlInteger Parse(ReadOnlySpan<byte> bytes)
    {
        if (Utf8Parser.TryParse(bytes, out long value, out int _))
        {
            return TomlInteger.Create(value);
        }

        ExceptionHelper.ThrowFailedToParseToNumeric();
        return default!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TomlInteger ParseBinary(ReadOnlySpan<byte> bytes)
    {
        return TomlInteger.Create(ParseBinaryValue(bytes[2..]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TomlInteger ParseOctal(ReadOnlySpan<byte> bytes)
    {
        return TomlInteger.Create(ParseOctalValue(bytes[2..]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TomlInteger ParseHex(ReadOnlySpan<byte> bytes)
    {
        return TomlInteger.Create(ParseHexValue(bytes[2..]));
    }

    private static long ParseBinaryValue(ReadOnlySpan<byte> utf8Bytes)
    {
        var digits = utf8Bytes.Length;
        if (digits > 64) ExceptionHelper.ThrowOverflowCount();

        long value = 0;
        int digitsCount = 1;
        long baseValue = 1;
        while (true)
        {
            value += ParseBinaryByte(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 2;

            value += ParseBinaryByte(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 2;

            value += ParseBinaryByte(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 2;

            value += ParseBinaryByte(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 2;
        }
    }

    private static long ParseOctalValue(ReadOnlySpan<byte> utf8Bytes)
    {
        var digits = utf8Bytes.Length;
        if (digits > 21) ExceptionHelper.ThrowOverflowCount();

        long value = 0;
        int digitsCount = 1;
        long baseValue = 1;
        while (true)
        {
            value += ParseOctalByte(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 8;

            value += ParseOctalByte(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 8;

            value += ParseOctalByte(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 8;

            value += ParseOctalByte(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 8;
        }
    }

    private static long ParseHexValue(ReadOnlySpan<byte> utf8Bytes)
    {
        var digits = utf8Bytes.Length;
        if (digits > 16) ExceptionHelper.ThrowOverflowCount();

        long value = 0;
        int digitsCount = 1;
        long baseValue = 1;
        while (true)
        {
            value += ParseHexByte(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 16;

            value += ParseHexByte(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 16;

            value += ParseHexByte(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 16;

            value += ParseHexByte(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 16;
        }
    }

    private static long ParseBinaryByte(byte utf8Byte)
    {
        if (!TomlCodes.IsBinary(utf8Byte))
        {
            ExceptionHelper.ThrowNumericConversionFailed(utf8Byte);
        }
        return TomlCodes.Number.ParseDecimal(utf8Byte);
    }

    private static long ParseOctalByte(byte utf8Byte)
    {
        if (!TomlCodes.IsOctal(utf8Byte))
        {
            ExceptionHelper.ThrowNumericConversionFailed(utf8Byte);
        }
        return TomlCodes.Number.ParseDecimal(utf8Byte);
    }

    private static long ParseHexByte(byte utf8Byte)
    {
        if (TomlCodes.IsUpperHexAlphabet(utf8Byte))
        {
            return utf8Byte - 0x37;
        }
        if (TomlCodes.IsLowerHexAlphabet(utf8Byte))
        {
            return utf8Byte - 0x57;
        }
        if (!TomlCodes.IsNumber(utf8Byte))
        {
            ExceptionHelper.ThrowNumericConversionFailed(utf8Byte);
        }
        return TomlCodes.Number.ParseDecimal(utf8Byte);
    }
}

