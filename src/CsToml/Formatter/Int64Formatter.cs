using CsToml.Error;
using CsToml.Utility;
using System.Buffers;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Formatter;

internal class Int64Formatter : ITomlValueFormatter<long>
{
    public static readonly Int64Formatter Default = new Int64Formatter();

    public void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, long value)
        where TBufferWriter : IBufferWriter<byte>
    {
        var length = TomlCodes.Number.DigitsDecimalUnroll4(value);
        if (value < 0) length++;

        value.TryFormat(writer.GetWrittenSpan(length), out int bytesWritten);
        return;
    }

    public void Deserialize(ReadOnlySpan<byte> bytes, ref long value)
    {
        // hexadecimal, octal, or binary
        if (bytes.Length > 2)
        {
            var prefix = Unsafe.ReadUnaligned<short>(ref MemoryMarshal.GetReference<byte>(bytes));
            switch (prefix)
            {
                case 25136: //0b:binary
                    value = DeserializeBinary(bytes[2..]);
                    return;
                case 28464: //0o:octal
                    value = DeserializeOctal(bytes[2..]);
                    return;
                case 30768: //0x:hexadecimal
                    value = DeserializeHex(bytes[2..]);
                    return;
            }
        }

        if (Utf8Parser.TryParse(bytes, out value, out int bytesConsumed2))
        {
            return;
        }
        ExceptionHelper.ThrowFailedToParseToNumeric();
    }

    private long DeserializeBinary(ReadOnlySpan<byte> utf8Bytes)
    {
        var digits = utf8Bytes.Length;
        if (digits > 64) ExceptionHelper.ThrowOverflowCount();

        long value = 0;
        int digitsCount = 1;
        long baseValue = 1;
        while (true)
        {
            value += DeserializeBinary(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 2;

            value += DeserializeBinary(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 2;

            value += DeserializeBinary(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 2;

            value += DeserializeBinary(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 2;
        }
    }

    private long DeserializeOctal(ReadOnlySpan<byte> utf8Bytes)
    {
        var digits = utf8Bytes.Length;
        if (digits > 21) ExceptionHelper.ThrowOverflowCount();

        long value = 0;
        int digitsCount = 1;
        long baseValue = 1;
        while (true)
        {
            value += DeserializeOctal(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 8;

            value += DeserializeOctal(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 8;

            value += DeserializeOctal(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 8;

            value += DeserializeOctal(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 8;
        }
    }

    private long DeserializeHex(ReadOnlySpan<byte> utf8Bytes)
    {
        var digits = utf8Bytes.Length;
        if (digits > 16) ExceptionHelper.ThrowOverflowCount();

        long value = 0;
        int digitsCount = 1;
        long baseValue = 1;
        while (true)
        {
            value += DeserializeHex(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 16;

            value += DeserializeHex(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 16;

            value += DeserializeHex(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 16;

            value += DeserializeHex(utf8Bytes[utf8Bytes.Length - digitsCount]) * baseValue;
            if (value < 0) ExceptionHelper.ThrowOverflowCount();
            if (digits == digitsCount++) return value;
            baseValue *= 16;
        }
    }

    private long DeserializeBinary(byte utf8Byte)
    {
        if (!TomlCodes.IsBinary(utf8Byte))
        {
            ExceptionHelper.ThrowNumericConversionFailed(utf8Byte);
        }
        return TomlCodes.Number.ParseDecimal(utf8Byte);
    }

    private long DeserializeOctal(byte utf8Byte)
    {
        if (!TomlCodes.IsOctal(utf8Byte))
        {
            ExceptionHelper.ThrowNumericConversionFailed(utf8Byte);
        }
        return TomlCodes.Number.ParseDecimal(utf8Byte);
    }

    private long DeserializeHex(byte utf8Byte)
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


