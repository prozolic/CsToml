using CsToml.Error;
using CsToml.Utility;
using System.Buffers;
using System.Buffers.Text;

namespace CsToml.Formatter;

internal class Int64Formatter : ICsTomlFormatter<long>
{
    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, long value)
        where TBufferWriter : IBufferWriter<byte>
    {
        var length = CsTomlSyntax.Number.DigitsDecimalUnroll4(value);
        if (value < 0) length++;

        value.TryFormat(writer.GetWriteSpan(length), out int bytesWritten);
        return;
    }

    public static long Deserialize(ref Utf8Reader reader, int length)
    {
        var bytes = reader.ReadBytes(length);

        // hexadecimal, octal, or binary
        if (bytes.Length > 2 && bytes[0] == 0x30)
        {
            switch (bytes[1])
            {
                case CsTomlSyntax.AlphaBet.b: //0b:binary
                    return DeserializeBinary(bytes[2..]);
                case CsTomlSyntax.AlphaBet.o: //0o:octal
                    return DeserializeOctal(bytes[2..]);
                case CsTomlSyntax.AlphaBet.x: //0x:hexadecimal
                    return DeserializeHex(bytes[2..]);
            }
        }

        if (Utf8Parser.TryParse(bytes, out long value, out int bytesConsumed2))
        {
            return value;
        }
        return ExceptionHelper.NotReturnThrow<int>(ExceptionHelper.ThrowFailedToParseToNumeric);
    }

    public static long DeserializeBinary(ReadOnlySpan<byte> utf8Bytes)
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

    public static long DeserializeOctal(ReadOnlySpan<byte> utf8Bytes)
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

    private static long DeserializeHex(ReadOnlySpan<byte> utf8Bytes)
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

    internal static long DeserializeDecimal(byte utf8Byte)
    {
        if (!CsTomlSyntax.IsNumber(utf8Byte))
        {
            ExceptionHelper.ThrowNumericConversionFailed(utf8Byte);
        }
        return CsTomlSyntax.Number.ParseDecimal(utf8Byte);
    }

    private static long DeserializeBinary(byte utf8Byte)
    {
        if (!CsTomlSyntax.IsBinary(utf8Byte))
        {
            ExceptionHelper.ThrowNumericConversionFailed(utf8Byte);
        }
        return CsTomlSyntax.Number.ParseDecimal(utf8Byte);
    }

    private static long DeserializeOctal(byte utf8Byte)
    {
        if (!CsTomlSyntax.IsOctal(utf8Byte))
        {
            ExceptionHelper.ThrowNumericConversionFailed(utf8Byte);
        }
        return CsTomlSyntax.Number.ParseDecimal(utf8Byte);
    }

    private static long DeserializeHex(byte utf8Byte)
    {
        if (CsTomlSyntax.IsUpperHexAlphabet(utf8Byte))
        {
            return utf8Byte - 0x37;
        }
        if (CsTomlSyntax.IsLowerHexAlphabet(utf8Byte))
        {
            return utf8Byte - 0x57;
        }
        if (!CsTomlSyntax.IsNumber(utf8Byte))
        {
            ExceptionHelper.ThrowNumericConversionFailed(utf8Byte);
        }
        return CsTomlSyntax.Number.ParseDecimal(utf8Byte);
    }

}


