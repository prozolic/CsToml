
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml;

internal static class CsTomlSyntax
{
    public readonly struct Number
    {
        public static readonly byte[] Value10 = "0123456789"u8.ToArray();

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ParseDecimal(byte number) => number - 0x30;

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ParseHex(byte hexNumber)
        {
            if (IsNumber(hexNumber)) return ParseDecimal(hexNumber);
            if ((AlphaBet.A <= hexNumber && hexNumber <= AlphaBet.F)) return hexNumber - 0x37;
            if ((AlphaBet.a <= hexNumber && hexNumber <= AlphaBet.f)) return hexNumber - 0x57;
            return -1;
        }

        public static int DigitsDecimalUnroll4(long value)
        {
            var number = 1;
            value = Math.Abs(value);

            for (;;)
            {
                if (value < 10) return number;
                if (value < 100) return number + 1;
                if (value < 1000) return number + 2;
                if (value < 10000) return number + 3;
                value /= 10000;
                number += 4;
            }
        }
    }

    public readonly struct Double
    {
        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types
        // double range: ±5.0 × 10−324 - ±1.7 × 10 308
        public static readonly double[] PositivePosExps = new double[309];
        public static readonly double[] NagativePosExps = new double[325];

        public const double Inf = double.PositiveInfinity;
        public const double NInf = double.NegativeInfinity;
        public const double Nan = double.NaN;
        public const double PNan = -double.NaN;

        static Double()
        {
            var pPosExps = PositivePosExps.AsSpan();
            for (int i = 0; i < pPosExps.Length; i++)
            {
                pPosExps[i] = Math.Pow(10d , i);
            }
            var nPosExps = NagativePosExps.AsSpan();
            for (int i = 0; i < nPosExps.Length; i++)
            {
                nPosExps[i] = Math.Pow(10d, -i);
            }
        }
    }

    public readonly struct AlphaBet
    {
        public const byte a = 0x61;
        public const byte b = 0x62;
        public const byte e = 0x65;
        public const byte f = 0x66;
        public const byte i = 0x69;
        public const byte l = 0x6c;
        public const byte n = 0x6e;
        public const byte o = 0x6f;
        public const byte r = 0x72;
        public const byte s = 0x73;
        public const byte t = 0x74;
        public const byte u = 0x75;
        public const byte x = 0x78;

        public const byte A = 0x41;
        public const byte E = 0x45;
        public const byte F = 0x46;
        public const byte T = 0x54;
        public const byte U = 0x55;
        public const byte Z = 0x5a;
    }

    public readonly struct Symbol
    {
        public const byte UNDERSCORE = 0x5f;
        public const byte DASH = 0x2d;
        public const byte BACKSPACE = 0x08;
        public const byte TAB = 0x09;
        public const byte SPACE = 0x20;
        public const byte NUMBERSIGN = 0x23;
        public const byte CARRIAGE = 0x0d;
        public const byte LINEFEED = 0x0a;
        public const byte FORMFEED = 0x0c;
        public const byte DOUBLEQUOTED = 0x22;
        public const byte SINGLEQUOTED = 0x27;
        public const byte LEFTSQUAREBRACKET = 0x5b;
        public const byte RIGHTSQUAREBRACKET = 0x5d;
        public const byte LEFTBRACES = 0x7b;
        public const byte RIGHTBRACES = 0x7d;
        public const byte EQUAL = 0x3d;
        public const byte PLUS = 0x2b;
        public const byte MINUS = 0x2d;
        public const byte BACKSLASH = 0x5c;
        public const byte COLON = 0x3a;
        public const byte PERIOD = 0x2e;
        public const byte COMMA = 0x2c;

        public static readonly byte[] UnixNewLine = [LINEFEED];
        public static readonly byte[] WindowsNewLine = [CARRIAGE, LINEFEED]; 
    }

    public readonly struct DateTime
    {
        public static readonly byte[] LocalTimeFormat = "HH:mm:ss"u8.ToArray();
        public static readonly byte[] LocalDateFormat = "yyyy-MM-dd"u8.ToArray();
        public static readonly byte[] LocalDateTimeFormat = "yyyy-MM-ddTHH:mm:ss"u8.ToArray();
        public static readonly byte[] OffsetDateTimeZFormat = "yyyy-MM-ddTHH:mm:ssZ"u8.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEscape(byte rawByte) // U+0000-U+0008,U+000A=U+001F,U+007F
        => (0x00 <= rawByte && rawByte <= 0x08) || (0x0a <= rawByte && rawByte <= 0x1f) || rawByte == 0x7f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEscapeSequence(byte rawByte)
        => rawByte == AlphaBet.b || rawByte == AlphaBet.t || rawByte == AlphaBet.n ||rawByte == AlphaBet.f || rawByte == AlphaBet.r ||
           IsDoubleQuoted(rawByte) || IsBackSlash(rawByte) || IsEscapeSequenceUnicode(rawByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEscapeSequenceUnicode(byte rawByte)
        => rawByte == AlphaBet.u || rawByte == AlphaBet.U;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNumberSign(byte rawByte)
        => rawByte == Symbol.NUMBERSIGN;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTab(byte rawByte)
        => rawByte == Symbol.TAB;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCr(byte ch)
        => ch == Symbol.CARRIAGE;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLf(byte ch) 
        => ch == Symbol.LINEFEED;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNewLine(byte rawByte)
        => IsCr(rawByte) || IsLf(rawByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLineBreak(byte rawByte)
        => IsLf(rawByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBareKey(byte rawByte)
        => IsAlphabet(rawByte) || IsNumber(rawByte) || IsKeySymbol(rawByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsKeySymbol(byte rawByte)
        => IsUnderScore(rawByte) || IsDash(rawByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUnderScore(byte rawByte)
        => rawByte == Symbol.UNDERSCORE;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDash(byte rawByte)
        => rawByte == Symbol.DASH;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDoubleQuoted(byte rawByte)
        => rawByte == Symbol.DOUBLEQUOTED;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSingleQuoted(byte rawByte)
        => rawByte == Symbol.SINGLEQUOTED;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsQuoted(byte rawByte)
        => IsDoubleQuoted(rawByte) || IsSingleQuoted(rawByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLeftSquareBrackets(byte rawByte)
        => rawByte == Symbol.LEFTSQUAREBRACKET;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRightSquareBrackets(byte rawByte)
        => rawByte == Symbol.RIGHTSQUAREBRACKET;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLeftBraces(byte rawByte)
        => rawByte == Symbol.LEFTBRACES;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRightBraces(byte rawByte)
        => rawByte == Symbol.RIGHTBRACES;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTabOrWhiteSpace(byte rawByte)
        => IsTab(rawByte) || IsWhiteSpace(rawByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWhiteSpace(byte rawByte)
        => rawByte == Symbol.SPACE;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEqual(byte rawByte)
        => rawByte == Symbol.EQUAL;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNumber(byte rawByte)
        => Number.Value10[0] <= rawByte && rawByte <= Number.Value10[9];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBinary(byte rawByte)
        => Number.Value10[0] <= rawByte && rawByte <= Number.Value10[1];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOctal(byte rawByte)
        => Number.Value10[0] <= rawByte && rawByte <= Number.Value10[7];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHex(byte rawByte)
        => IsNumber(rawByte) || (AlphaBet.A <= rawByte && rawByte <= AlphaBet.F) || (AlphaBet.a <= rawByte && rawByte <= AlphaBet.f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUpperHexAlphabet(byte rawByte)
        => 0x41 <= rawByte && rawByte <= 0x46;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLowerHexAlphabet(byte rawByte)
        => 0x61 <= rawByte && rawByte <= 0x66;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAlphabet(byte rawByte)
        => IsUpperAlphabet(rawByte) || IsLowerAlphabet(rawByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUpperAlphabet(byte ch)
        => 0x41 <= ch && ch <= 0x5a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLowerAlphabet(byte rawByte)
        => 0x61 <= rawByte && rawByte <= 0x7a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPlusSign(byte rawByte)
        => rawByte == Symbol.PLUS;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMinusSign(byte rawByte)
        => rawByte == Symbol.MINUS;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPlusOrMinusSign(byte rawByte)
        => IsPlusSign(rawByte) || IsMinusSign(rawByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBackSlash(byte rawByte)
        => rawByte == Symbol.BACKSLASH;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHyphen(byte rawByte)
        => IsMinusSign(rawByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsColon(byte rawByte)
        => rawByte == Symbol.COLON;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPeriod(byte rawByte)
        => rawByte == Symbol.PERIOD;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsExpSymbol(byte rawByte)
        => rawByte == AlphaBet.e || rawByte == AlphaBet.E;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsComma(byte rawByte)
        => rawByte == Symbol.COMMA;
}

