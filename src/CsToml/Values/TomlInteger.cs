using CsToml.Error;
using System.Buffers.Text;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace CsToml.Values;

internal interface ITomlIntegerCreator<T>
    where T : TomlInteger
{
    static abstract T Create(long value);
}

[DebuggerDisplay("{Value}")]
internal abstract partial class TomlInteger : TomlValue
{
    public static TomlInteger Zero => Cache<TomlDefaultInteger>.Values[1];

    public long Value { get; init; } 

    public override bool HasValue => true;

    public override TomlValueType Type => TomlValueType.Integer;

    private TomlInteger(long value)
    {
        this.Value = value;
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
            return TomlDefaultInteger.Create(value);
        }

        ExceptionHelper.ThrowFailedToParseToNumeric();
        return default!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TomlInteger ParseBinary(ReadOnlySpan<byte> bytes)
    {
        return TomlBinaryInteger.Create(bytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TomlInteger ParseOctal(ReadOnlySpan<byte> bytes)
    {
        return TomlOctalInteger.Create(bytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TomlInteger ParseHex(ReadOnlySpan<byte> bytes)
    {
        return TomlHexInteger.Create(bytes);
    }

    private sealed class TomlDefaultInteger(long value) : TomlInteger(value), ITomlIntegerCreator<TomlDefaultInteger>
    {
        static TomlDefaultInteger ITomlIntegerCreator<TomlDefaultInteger>.Create(long value)
        {
            return new TomlDefaultInteger(value);
        }

        public static TomlDefaultInteger Create(long value)
        {
            if ((ulong)(value + 1) < (ulong)CacheHelper.Length)
            {
                return Cache<TomlDefaultInteger>.Values[value + 1];
            }
            return new TomlDefaultInteger(value);
        }

        internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
        {
            writer.WriteInt64(Value);
        }
    }

    private sealed class TomlBinaryInteger(long value) : TomlInteger(value), ITomlIntegerCreator<TomlBinaryInteger>
    {
        static TomlBinaryInteger ITomlIntegerCreator<TomlBinaryInteger>.Create(long value)
        {
            return new TomlBinaryInteger(value);
        }

        public static TomlBinaryInteger Create(ReadOnlySpan<byte> bytes)
        {
            var value = ParseBinaryValue(bytes);
            if ((ulong)(value + 1) < (ulong)CacheHelper.Length)
            {
                return Cache<TomlBinaryInteger>.Values[value + 1];
            }
            return new TomlBinaryInteger(value);
        }

        private static long ParseBinaryValue(ReadOnlySpan<byte> bytes)
        {
            var digits = bytes.Length;
            if (digits > 64) ExceptionHelper.ThrowOverflowCount();

            long value = 0;
            int digitsCount = 1;
            long baseValue = 1;
            while (true)
            {
                value += ParseBinaryByte(bytes[bytes.Length - digitsCount]) * baseValue;
                if (value < 0) ExceptionHelper.ThrowOverflowCount();
                if (digits == digitsCount++) return value;
                baseValue *= 2;

                value += ParseBinaryByte(bytes[bytes.Length - digitsCount]) * baseValue;
                if (value < 0) ExceptionHelper.ThrowOverflowCount();
                if (digits == digitsCount++) return value;
                baseValue *= 2;

                value += ParseBinaryByte(bytes[bytes.Length - digitsCount]) * baseValue;
                if (value < 0) ExceptionHelper.ThrowOverflowCount();
                if (digits == digitsCount++) return value;
                baseValue *= 2;

                value += ParseBinaryByte(bytes[bytes.Length - digitsCount]) * baseValue;
                if (value < 0) ExceptionHelper.ThrowOverflowCount();
                if (digits == digitsCount++) return value;
                baseValue *= 2;
            }

            static long ParseBinaryByte(byte utf8Byte)
            {
                if (!TomlCodes.IsBinary(utf8Byte))
                {
                    ExceptionHelper.ThrowNumericConversionFailed(utf8Byte);
                }
                return TomlCodes.Number.ParseDecimal(utf8Byte);
            }

        }

        internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
        {
            writer.WriteInt64InBinaryFormat(Value);
        }
    }

    private sealed class TomlOctalInteger(long value) : TomlInteger(value), ITomlIntegerCreator<TomlOctalInteger>
    {
        static TomlOctalInteger ITomlIntegerCreator<TomlOctalInteger>.Create(long value)
        {
            return new TomlOctalInteger(value);
        }

        public static TomlOctalInteger Create(ReadOnlySpan<byte> bytes)
        {
            var value = ParseOctalValue(bytes);
            if ((ulong)(value + 1) < (ulong)CacheHelper.Length)
            {
                return Cache<TomlOctalInteger>.Values[value + 1];
            }
            return new TomlOctalInteger(value);
        }

        private static long ParseOctalValue(ReadOnlySpan<byte> bytes)
        {
            var digits = bytes.Length;
            if (digits > 21) ExceptionHelper.ThrowOverflowCount();

            long value = 0;
            int digitsCount = 1;
            long baseValue = 1;
            while (true)
            {
                value += ParseOctalByte(bytes[bytes.Length - digitsCount]) * baseValue;
                if (value < 0) ExceptionHelper.ThrowOverflowCount();
                if (digits == digitsCount++) return value;
                baseValue *= 8;

                value += ParseOctalByte(bytes[bytes.Length - digitsCount]) * baseValue;
                if (value < 0) ExceptionHelper.ThrowOverflowCount();
                if (digits == digitsCount++) return value;
                baseValue *= 8;

                value += ParseOctalByte(bytes[bytes.Length - digitsCount]) * baseValue;
                if (value < 0) ExceptionHelper.ThrowOverflowCount();
                if (digits == digitsCount++) return value;
                baseValue *= 8;

                value += ParseOctalByte(bytes[bytes.Length - digitsCount]) * baseValue;
                if (value < 0) ExceptionHelper.ThrowOverflowCount();
                if (digits == digitsCount++) return value;
                baseValue *= 8;
            }

            static long ParseOctalByte(byte utf8Byte)
            {
                if (!TomlCodes.IsOctal(utf8Byte))
                {
                    ExceptionHelper.ThrowNumericConversionFailed(utf8Byte);
                }
                return TomlCodes.Number.ParseDecimal(utf8Byte);
            }

        }

        internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
        {
            writer.WriteInt64InOctalFormat(Value);
        }
    }

    private sealed class TomlHexInteger(long value) : TomlInteger(value), ITomlIntegerCreator<TomlHexInteger>
    {
        static TomlHexInteger ITomlIntegerCreator<TomlHexInteger>.Create(long value)
        {
            return new TomlHexInteger(value);
        }

        public static TomlHexInteger Create(ReadOnlySpan<byte> bytes)
        {
            var value = ParseHexValue(bytes);
            if ((ulong)(value + 1) < (ulong)CacheHelper.Length)
            {
                return Cache<TomlHexInteger>.Values[value + 1];
            }
            return new TomlHexInteger(value);
        }

        private static long ParseHexValue(ReadOnlySpan<byte> bytes)
        {
            var digits = bytes.Length;
            if (digits > 16) ExceptionHelper.ThrowOverflowCount();

            long value = 0;
            int digitsCount = 1;
            long baseValue = 1;
            while (true)
            {
                value += ParseHexByte(bytes[bytes.Length - digitsCount]) * baseValue;
                if (value < 0) ExceptionHelper.ThrowOverflowCount();
                if (digits == digitsCount++) return value;
                baseValue *= 16;

                value += ParseHexByte(bytes[bytes.Length - digitsCount]) * baseValue;
                if (value < 0) ExceptionHelper.ThrowOverflowCount();
                if (digits == digitsCount++) return value;
                baseValue *= 16;

                value += ParseHexByte(bytes[bytes.Length - digitsCount]) * baseValue;
                if (value < 0) ExceptionHelper.ThrowOverflowCount();
                if (digits == digitsCount++) return value;
                baseValue *= 16;

                value += ParseHexByte(bytes[bytes.Length - digitsCount]) * baseValue;
                if (value < 0) ExceptionHelper.ThrowOverflowCount();
                if (digits == digitsCount++) return value;
                baseValue *= 16;
            }

            static long ParseHexByte(byte utf8Byte)
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

        internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
        {
            writer.WriteInt64InHexFormat(Value);
        }
    }

    private readonly struct Cache<T> where T : TomlInteger, ITomlIntegerCreator<T>
    {
        internal static readonly T[] Values;

        static Cache()
        {
            Values = CacheHelper.Create<T>();
        }
    }

    private static class CacheHelper
    {
        public static readonly int Length = 10;

        internal static T[] Create<T>() where T : TomlInteger, ITomlIntegerCreator<T>
        {
            var array = new T[Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = T.Create(i - 1);
            }
            return array;
        }
    }
}