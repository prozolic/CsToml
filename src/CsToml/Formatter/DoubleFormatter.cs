
using CsToml.Error;
using CsToml.Utility;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace CsToml.Formatter;

internal class DoubleFormatter : ICsTomlFormatter<double>
{
    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, double value)
        where TBufferWriter : IBufferWriter<byte>
    {
        var length = 32;
        int bytesWritten;
        if (value - System.Math.Floor(value) != 0)
        {
            while (!value.TryFormat(writer.GetSpan(length), out bytesWritten))
            {
                length *= 2;
            }
        }
        else
        {
            while (!value.TryFormat(writer.GetSpan(length), out bytesWritten, "G"))
            {
                length *= 2;
            }
        }
        writer.Advance(bytesWritten);
    }

    public static double Deserialize(ref Utf8Reader reader, int length)
    {
        var bytes = reader.ReadBytes(length);
        if (bytes.Length < 3) ExceptionHelper.ThrowIncorrectTomlFloatFormat();

        // sign check
        if (CsTomlSyntax.IsPlusOrMinusSign(bytes[0]))
        {
            var plusOrMinus = CsTomlSyntax.IsPlusSign(bytes[0]) ? 1 : -1;
            if (bytes[1] == CsTomlSyntax.Alphabet.i &&
                bytes[2] == CsTomlSyntax.Alphabet.n &&
                bytes[3] == CsTomlSyntax.Alphabet.f)
            {
                return plusOrMinus * CsTomlSyntax.Float.Inf;
            }
            if (bytes[1] == CsTomlSyntax.Alphabet.n &&
                bytes[2] == CsTomlSyntax.Alphabet.a &&
                bytes[3] == CsTomlSyntax.Alphabet.n)
            {
                return plusOrMinus * CsTomlSyntax.Float.Nan;
            }

            return plusOrMinus * DeserializeDouble(bytes[1..]);
        }
        else
        {
            if (bytes[0] == CsTomlSyntax.Alphabet.i &&
                bytes[1] == CsTomlSyntax.Alphabet.n &&
                bytes[2] == CsTomlSyntax.Alphabet.f)
            {
                return CsTomlSyntax.Float.Inf;
            }
            if (bytes[0] == CsTomlSyntax.Alphabet.n &&
                bytes[1] == CsTomlSyntax.Alphabet.a &&
                bytes[2] == CsTomlSyntax.Alphabet.n)
            {
                return CsTomlSyntax.Float.Nan;
            }

            return DeserializeDouble(bytes);
        }
    }

    private static double DeserializeDouble(ReadOnlySpan<byte> utf8Bytes)
    {
        double integerValue = 0d;
        var index = 0;

        // integer part
        while (true)
        {
            if (CsTomlSyntax.IsNumber(utf8Bytes[index]))
            {
                integerValue *= 10d;
                integerValue += CsTomlSyntax.Number.ParseDecimal(utf8Bytes[index++]);
                continue;
            }
            else if (CsTomlSyntax.IsPeriod(utf8Bytes[index]))
            {
                break;
            }
            else if (CsTomlSyntax.IsExpSymbol(utf8Bytes[index]))
            {
                break;
            }
            ExceptionHelper.ThrowIncorrectFormattingOfFloatingNumbers();
        }

        // decimal part
        double decimalValue = 0d;
        if (index < utf8Bytes.Length && CsTomlSyntax.IsPeriod(utf8Bytes[index]))
        {
            index++;
            var nIndex = 1;
            while (index < utf8Bytes.Length && CsTomlSyntax.IsNumber(utf8Bytes[index]))
            {
                decimalValue += CsTomlSyntax.Number.ParseDecimal(utf8Bytes[index++]) / GetPositiveExponent(nIndex++);
            }
        }

        // exponent part
        long exponentValue = 0;
        var plusSign = true;
        if (index < utf8Bytes.Length && CsTomlSyntax.IsExpSymbol(utf8Bytes[index]))
        {
            index++;
            if (CsTomlSyntax.IsPlusOrMinusSign(utf8Bytes[index]))
            {
                if (CsTomlSyntax.IsMinusSign(utf8Bytes[index]))
                {
                    plusSign = false;
                }
                index++;
            }
            while (index < utf8Bytes.Length && CsTomlSyntax.IsNumber(utf8Bytes[index]))
            {
                exponentValue *= 10;
                exponentValue += CsTomlSyntax.Number.ParseDecimal(utf8Bytes[index++]);
            }
        }

        return plusSign ?
            (integerValue + decimalValue) * GetPositiveExponent(exponentValue) :
            (integerValue + decimalValue) / GetPositiveExponent(exponentValue);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double GetPositiveExponent(long exponentValue)
    {
        if (exponentValue <= 15)
        {
            return CsTomlSyntax.Float.PositivePosExps15[exponentValue];
        }

        return Math.Pow(10d, exponentValue);
    }
}

