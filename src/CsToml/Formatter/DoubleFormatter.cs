
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
        if (TomlCodes.IsPlusOrMinusSign(bytes[0]))
        {
            var plusOrMinus = TomlCodes.IsPlusSign(bytes[0]) ? 1 : -1;
            if (bytes[1] == TomlCodes.Alphabet.i &&
                bytes[2] == TomlCodes.Alphabet.n &&
                bytes[3] == TomlCodes.Alphabet.f)
            {
                return plusOrMinus * TomlCodes.Float.Inf;
            }
            if (bytes[1] == TomlCodes.Alphabet.n &&
                bytes[2] == TomlCodes.Alphabet.a &&
                bytes[3] == TomlCodes.Alphabet.n)
            {
                return plusOrMinus * TomlCodes.Float.Nan;
            }

            return plusOrMinus * DeserializeDouble(bytes[1..]);
        }
        else
        {
            if (bytes[0] == TomlCodes.Alphabet.i &&
                bytes[1] == TomlCodes.Alphabet.n &&
                bytes[2] == TomlCodes.Alphabet.f)
            {
                return TomlCodes.Float.Inf;
            }
            if (bytes[0] == TomlCodes.Alphabet.n &&
                bytes[1] == TomlCodes.Alphabet.a &&
                bytes[2] == TomlCodes.Alphabet.n)
            {
                return TomlCodes.Float.Nan;
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

