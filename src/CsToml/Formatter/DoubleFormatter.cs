
using CsToml.Error;
using CsToml.Utility;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace CsToml.Formatter;

internal class DoubleFormatter : ITomlValueFormatter<double>
{
    public static readonly DoubleFormatter Default = new DoubleFormatter();

    public void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, double value)
        where TBufferWriter : IBufferWriter<byte>
    {
        var length = 32;
        int bytesWritten;
        while (!value.TryFormat(writer.GetSpan(length), out bytesWritten, "G"))
        {
            length *= 2;
        }
        writer.Advance(bytesWritten);
    }

    public void Deserialize(ReadOnlySpan<byte> bytes, ref double value)
    {
        if (bytes.Length < 3) ExceptionHelper.ThrowIncorrectTomlFloatFormat();

        // sign check
        if (TomlCodes.IsPlusOrMinusSign(bytes[0]))
        {
            var plusOrMinus = TomlCodes.IsPlusSign(bytes[0]) ? 1 : -1;
            if (bytes[1] == TomlCodes.Alphabet.i &&
                bytes[2] == TomlCodes.Alphabet.n &&
                bytes[3] == TomlCodes.Alphabet.f)
            {
                value = plusOrMinus * TomlCodes.Float.Inf;
                return;
            }
            if (bytes[1] == TomlCodes.Alphabet.n &&
                bytes[2] == TomlCodes.Alphabet.a &&
                bytes[3] == TomlCodes.Alphabet.n)
            {
                value =  plusOrMinus * TomlCodes.Float.Nan;
                return;
            }

            value = plusOrMinus * DeserializeUnsafe(bytes[1..]);
            return;
        }
        else
        {
            if (bytes[0] == TomlCodes.Alphabet.i &&
                bytes[1] == TomlCodes.Alphabet.n &&
                bytes[2] == TomlCodes.Alphabet.f)
            {
                value =  TomlCodes.Float.Inf;
                return;
            }
            if (bytes[0] == TomlCodes.Alphabet.n &&
                bytes[1] == TomlCodes.Alphabet.a &&
                bytes[2] == TomlCodes.Alphabet.n)
            {
                value =  TomlCodes.Float.Nan;
                return;
            }

            value = DeserializeUnsafe(bytes);
        }
    }

    private double DeserializeUnsafe(ReadOnlySpan<byte> utf8Bytes)
    {
        if(double.TryParse(utf8Bytes, out var value))
        {
            return value;
        }

        double integerValue = 0d;
        var index = 0;

        // integer part
        while (true)
        {
            if (TomlCodes.IsNumber(utf8Bytes[index]))
            {
                integerValue *= 10d;
                integerValue += TomlCodes.Number.ParseDecimal(utf8Bytes[index++]);
                continue;
            }
            else if (TomlCodes.IsDot(utf8Bytes[index]))
            {
                break;
            }
            else if (TomlCodes.IsExpSymbol(utf8Bytes[index]))
            {
                break;
            }
            ExceptionHelper.ThrowIncorrectFormattingOfFloatingNumbers();
        }

        // decimal part
        double decimalValue = 0d;
        if (index < utf8Bytes.Length && TomlCodes.IsDot(utf8Bytes[index]))
        {
            index++;
            var nIndex = 1;
            while (index < utf8Bytes.Length && TomlCodes.IsNumber(utf8Bytes[index]))
            {
                decimalValue += TomlCodes.Number.ParseDecimal(utf8Bytes[index++]) / GetPositiveExponent(nIndex++);
            }
        }

        // exponent part
        long exponentValue = 0;
        var plusSign = true;
        if (index < utf8Bytes.Length && TomlCodes.IsExpSymbol(utf8Bytes[index]))
        {
            index++;
            if (TomlCodes.IsPlusOrMinusSign(utf8Bytes[index]))
            {
                if (TomlCodes.IsMinusSign(utf8Bytes[index]))
                {
                    plusSign = false;
                }
                index++;
            }
            while (index < utf8Bytes.Length && TomlCodes.IsNumber(utf8Bytes[index]))
            {
                exponentValue *= 10;
                exponentValue += TomlCodes.Number.ParseDecimal(utf8Bytes[index++]);
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
            return TomlCodes.Float.PositivePosExps15[exponentValue];
        }

        return Math.Pow(10d, exponentValue);
    }
}

