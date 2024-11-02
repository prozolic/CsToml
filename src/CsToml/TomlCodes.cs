
using CsToml.Error;
using CsToml.Extension;
using CsToml.Utility;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace CsToml;

internal enum EscapeSequenceResult : byte
{
    Success,
    Failure,
    Unescaped
}

internal static class TomlCodes
{
    internal readonly struct Number
    {
        internal const byte Zero = 0x30;
        internal const byte One = 0x31;
        internal const byte Two = 0x32;
        internal const byte Three = 0x33;
        internal const byte Four = 0x34;
        internal const byte Five = 0x35;
        internal const byte Six = 0x36;
        internal const byte Seven = 0x37;
        internal const byte Eight = 0x38;
        internal const byte Nine = 0x39;

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ParseDecimal(byte number) => number - Zero;

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ParseHex(byte hexNumber)
        {
            //https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-8/
            ReadOnlySpan<sbyte> hexTable =
            [
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, // 0x00 - 0x0f
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, // 0x10 - 0x1f
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, // 0x20 - 0x2f
                 0,  1,  2,  3,  4,  5,  6,  7,  8,  9, -1, -1, -1, -1, -1, -1, // 0x30 - 0x3f
                -1, 10, 11, 12, 13, 14, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, // 0x40 - 0x4f
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, // 0x50 - 0x5f
                -1, 10, 11, 12, 13, 14, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, // 0x60 - 0x6f
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, // 0x70 - 0x7f
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, // 0x80 - 0x8f
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, // 0x90 - 0x9f
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, // 0xa0 - 0xaf
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, // 0xb0 - 0xbf
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, // 0xc0 - 0xcf
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, // 0xd0 - 0xdf
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, // 0xe0 - 0xef
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, // 0xf0 - 0xff
            ];
            return hexTable.At(hexNumber);
        }

        internal static int DigitsDecimalUnroll4(long value)
        {
            // long.MinValue raises an OverflowException, so fixed support
            // https://learn.microsoft.com/en-us/dotnet/api/system.math.abs?view=net-8.0#system-math-abs(system-int64)
            if (value == long.MinValue)
                return 19;

            var number = 1;
            value = Math.Abs(value);

            for (; ; )
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

    internal readonly struct Float
    {
        internal const double Inf = double.PositiveInfinity;
        internal const double NInf = double.NegativeInfinity;
        internal const double Nan = double.NaN;
    }

    internal readonly struct Alphabet
    {
        internal const byte a = 0x61;
        internal const byte b = 0x62;
        internal const byte e = 0x65;
        internal const byte f = 0x66;
        internal const byte i = 0x69;
        internal const byte l = 0x6c;
        internal const byte n = 0x6e;
        internal const byte o = 0x6f;
        internal const byte r = 0x72;
        internal const byte s = 0x73;
        internal const byte t = 0x74;
        internal const byte u = 0x75;
        internal const byte x = 0x78;
        internal const byte z = 0x7a;

        internal const byte A = 0x41;
        internal const byte E = 0x45;
        internal const byte F = 0x46;
        internal const byte T = 0x54;
        internal const byte U = 0x55;
        internal const byte Z = 0x5a;
    }

    internal readonly struct DateTime
    {
        internal const byte LocalTimeFormatLength = 8; // HH:mm:ss
        internal const byte LocalDateFormatLength = 10; // yyyy-MM-dd
        internal const byte LocalDateTimeFormatLength = 19; // yyyy-MM-ddTHH:mm:ss
        internal const byte OffsetDateTimeZFormatLength = 20; // yyyy-MM-ddTHH:mm:ssZ
    }

    internal readonly struct Symbol
    {
        internal const byte UNDERSCORE = 0x5f;
        internal const byte DASH = 0x2d;
        internal const byte BACKSPACE = 0x08;
        internal const byte TAB = 0x09;
        internal const byte SPACE = 0x20;
        internal const byte NUMBERSIGN = 0x23;
        internal const byte CARRIAGE = 0x0d;
        internal const byte LINEFEED = 0x0a;
        internal const byte FORMFEED = 0x0c;
        internal const byte DOUBLEQUOTED = 0x22;
        internal const byte SINGLEQUOTED = 0x27;
        internal const byte LEFTSQUAREBRACKET = 0x5b;
        internal const byte RIGHTSQUAREBRACKET = 0x5d;
        internal const byte LEFTBRACES = 0x7b;
        internal const byte RIGHTBRACES = 0x7d;
        internal const byte EQUAL = 0x3d;
        internal const byte PLUS = 0x2b;
        internal const byte MINUS = 0x2d;
        internal const byte BACKSLASH = 0x5c;
        internal const byte COLON = 0x3a;
        internal const byte DOT = 0x2e;
        internal const byte COMMA = 0x2c;
    }

    internal readonly struct Environment
    {
        internal static readonly byte[] NewLine = Encoding.UTF8.GetBytes(System.Environment.NewLine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsEscape(byte rawByte) // U+0000-U+0008,U+000A-U+001F,U+007F
    {
        ReadOnlySpan<bool> escapeTable =
        [
            true, true, true, true, true, true, true, true, true, false, true, true, true, true, true, true,                // 0x00 - 0x0f
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,                 // 0x10 - 0x1f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x20 - 0x2f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x30 - 0x3f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x40 - 0x4f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x50 - 0x5f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x60 - 0x6f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true,  // 0x70 - 0x7f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x80 - 0x8f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x90 - 0x9f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xa0 - 0xaf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xb0 - 0xbf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xc0 - 0xcf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xd0 - 0xdf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xe0 - 0xef
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xf0 - 0xff
        ];
        return escapeTable.At(rawByte);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsEscapeSequence(byte rawByte)
    {
        ReadOnlySpan<bool> escapeSequenceTable =
        [
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x00 - 0x0f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x10 - 0x1f
            false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false,  // 0x20 - 0x2f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x30 - 0x3f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x40 - 0x4f
            false, false, false, false, false, true, false, false, false, false, false, false, true, false, false, false,   // 0x50 - 0x5f
            false, false, true, false, false, false, true, false, false, false, false, false, false, false, true, false,    // 0x60 - 0x6f
            false, false, true, false, true, true, false, false, false, false, false, false, false, false, false, false,    // 0x70 - 0x7f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x80 - 0x8f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x90 - 0x9f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xa0 - 0xaf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xb0 - 0xbf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xc0 - 0xcf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xd0 - 0xdf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xe0 - 0xef
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xf0 - 0xff
        ];
        return escapeSequenceTable.At(rawByte);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsBareKey(byte rawByte)
    {
        ReadOnlySpan<bool> barekeyTable =
        [
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x00 - 0x0f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x10 - 0x1f
            false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false,  // 0x20 - 0x2f
            true, true, true, true, true, true, true, true, true, true, false, false, false, false, false, false,           // 0x30 - 0x3f
            false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,                // 0x40 - 0x4f
            true, true, true, true, true, true, true, true, true, true, true, false, false, false, false, true,             // 0x50 - 0x5f
            false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,                // 0x60 - 0x6f
            true, true, true, true, true, true, true, true, true, true, true, false, false, false, false, false,            // 0x70 - 0x7f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x80 - 0x8f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x90 - 0x9f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xa0 - 0xaf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xb0 - 0xbf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xc0 - 0xcf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xd0 - 0xdf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xe0 - 0xef
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xf0 - 0xff
        ];
        return barekeyTable.At(rawByte);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsNumberSign(byte rawByte)
        => rawByte == Symbol.NUMBERSIGN;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsTab(byte rawByte)
        => rawByte == Symbol.TAB;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsCr(byte ch)
        => ch == Symbol.CARRIAGE;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsLf(byte ch)
        => ch == Symbol.LINEFEED;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsNewLine(byte rawByte)
        => IsCr(rawByte) || IsLf(rawByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsKeySymbol(byte rawByte)
        => IsUnderScore(rawByte) || IsDash(rawByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsUnderScore(byte rawByte)
        => rawByte == Symbol.UNDERSCORE;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsDash(byte rawByte)
        => rawByte == Symbol.DASH;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsDoubleQuoted(byte rawByte)
        => rawByte == Symbol.DOUBLEQUOTED;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsSingleQuoted(byte rawByte)
        => rawByte == Symbol.SINGLEQUOTED;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsQuoted(byte rawByte)
        => IsDoubleQuoted(rawByte) || IsSingleQuoted(rawByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsLeftSquareBrackets(byte rawByte)
        => rawByte == Symbol.LEFTSQUAREBRACKET;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsRightSquareBrackets(byte rawByte)
        => rawByte == Symbol.RIGHTSQUAREBRACKET;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsLeftBraces(byte rawByte)
        => rawByte == Symbol.LEFTBRACES;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsRightBraces(byte rawByte)
        => rawByte == Symbol.RIGHTBRACES;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsTabOrWhiteSpace(byte rawByte)
        => IsTab(rawByte) || IsWhiteSpace(rawByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsWhiteSpace(byte rawByte)
        => rawByte == Symbol.SPACE;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsEqual(byte rawByte)
        => rawByte == Symbol.EQUAL;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsNumber(byte rawByte)
    {
        ReadOnlySpan<bool> numberTable =
        [
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x00 - 0x0f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x10 - 0x1f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x20 - 0x2f
            true, true, true, true, true, true, true, true, true, true, false, false, false, false, false, false,           // 0x30 - 0x3f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x40 - 0x4f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x50 - 0x5f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x60 - 0x6f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x70 - 0x7f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x80 - 0x8f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x90 - 0x9f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xa0 - 0xaf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xb0 - 0xbf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xc0 - 0xcf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xd0 - 0xdf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xe0 - 0xef
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xf0 - 0xff
        ];
        return numberTable.At(rawByte);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsBinary(byte rawByte)
    {
        ReadOnlySpan<bool> binaryTable =
        [
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x00 - 0x0f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x10 - 0x1f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x20 - 0x2f
            true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false,   // 0x30 - 0x3f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x40 - 0x4f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x50 - 0x5f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x60 - 0x6f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x70 - 0x7f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x80 - 0x8f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x90 - 0x9f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xa0 - 0xaf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xb0 - 0xbf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xc0 - 0xcf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xd0 - 0xdf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xe0 - 0xef
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xf0 - 0xff
        ];
        return binaryTable.At(rawByte);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsOctal(byte rawByte)
    {
        ReadOnlySpan<bool> octalTable =
        [
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x00 - 0x0f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x10 - 0x1f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x20 - 0x2f
            true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false,         // 0x30 - 0x3f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x40 - 0x4f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x50 - 0x5f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x60 - 0x6f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x70 - 0x7f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x80 - 0x8f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x90 - 0x9f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xa0 - 0xaf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xb0 - 0xbf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xc0 - 0xcf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xd0 - 0xdf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xe0 - 0xef
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xf0 - 0xff
        ];
        return octalTable.At(rawByte);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsHex(byte rawByte)
    {
        ReadOnlySpan<bool> hexTable =
        [
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x00 - 0x0f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x10 - 0x1f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x20 - 0x2f
            true, true, true, true, true, true, true, true, true, true, false, false, false, false, false, false,           // 0x30 - 0x3f
            false, true, true, true, true, true, true, false, false, false, false, false, false, false, false, false,       // 0x40 - 0x4f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x50 - 0x5f
            false, true, true, true, true, true, true, false, false, false, false, false, false, false, false, false,       // 0x60 - 0x6f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x70 - 0x7f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x80 - 0x8f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x90 - 0x9f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xa0 - 0xaf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xb0 - 0xbf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xc0 - 0xcf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xd0 - 0xdf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xe0 - 0xef
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xf0 - 0xff
        ];
        return hexTable.At(rawByte);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsUpperHexAlphabet(byte rawByte)
    {
        // Alphabet.A <= rawByte && rawByte <= Alphabet.F;
        ReadOnlySpan<bool> upperHexAlphabetTable =
        [
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x00 - 0x0f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x10 - 0x1f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x20 - 0x2f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x30 - 0x3f
            false, true, true, true, true, true, true, false, false, false, false, false, false, false, false, false,       // 0x40 - 0x4f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x50 - 0x5f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x60 - 0x6f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x70 - 0x7f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x80 - 0x8f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x90 - 0x9f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xa0 - 0xaf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xb0 - 0xbf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xc0 - 0xcf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xd0 - 0xdf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xe0 - 0xef
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xf0 - 0xff
        ];
        return upperHexAlphabetTable.At(rawByte);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsLowerHexAlphabet(byte rawByte)
    {
        // Alphabet.a <= rawByte && rawByte <= Alphabet.f;
        ReadOnlySpan<bool> lowerHexAlphabetTable =
        [
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x00 - 0x0f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x10 - 0x1f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x20 - 0x2f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x30 - 0x3f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x40 - 0x4f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x50 - 0x5f
            false, true, true, true, true, true, true, false, false, false, false, false, false, false, false, false,       // 0x60 - 0x6f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x70 - 0x7f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x80 - 0x8f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x90 - 0x9f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xa0 - 0xaf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xb0 - 0xbf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xc0 - 0xcf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xd0 - 0xdf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xe0 - 0xef
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xf0 - 0xff
        ];
        return lowerHexAlphabetTable.At(rawByte);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsAlphabet(byte rawByte)
    {
        // IsUpperAlphabet(rawByte) || IsLowerAlphabet(rawByte);
        ReadOnlySpan<bool> alphabetTable =
        [
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x00 - 0x0f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x10 - 0x1f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x20 - 0x2f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x30 - 0x3f
            false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,                // 0x40 - 0x4f
            true, true, true, true, true, true, true, true, true, true, true, false, false, false, false, false,            // 0x50 - 0x5f
            false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,                // 0x60 - 0x6f
            true, true, true, true, true, true, true, true, true, true, true, false, false, false, false, false,            // 0x70 - 0x7f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x80 - 0x8f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x90 - 0x9f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xa0 - 0xaf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xb0 - 0xbf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xc0 - 0xcf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xd0 - 0xdf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xe0 - 0xef
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xf0 - 0xff
        ];
        return alphabetTable.At(rawByte);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsUpperAlphabet(byte rawByte)
    {
        ReadOnlySpan<bool> upperAlphabetTable =
        [
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x00 - 0x0f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x10 - 0x1f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x20 - 0x2f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x30 - 0x3f
            false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,                // 0x40 - 0x4f
            true, true, true, true, true, true, true, true, true, true, true, false, false, false, false, false,            // 0x50 - 0x5f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x60 - 0x6f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x70 - 0x7f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x80 - 0x8f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x90 - 0x9f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xa0 - 0xaf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xb0 - 0xbf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xc0 - 0xcf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xd0 - 0xdf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xe0 - 0xef
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xf0 - 0xff
        ];
        return upperAlphabetTable.At(rawByte);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsLowerAlphabet(byte rawByte)
    {
        ReadOnlySpan<bool> lowerAlphabetTable =
        [
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x00 - 0x0f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x10 - 0x1f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x20 - 0x2f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x30 - 0x3f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x40 - 0x4f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x50 - 0x5f
            false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,                // 0x60 - 0x6f
            true, true, true, true, true, true, true, true, true, true, true, false, false, false, false, false,            // 0x70 - 0x7f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x80 - 0x8f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0x90 - 0x9f
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xa0 - 0xaf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xb0 - 0xbf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xc0 - 0xcf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xd0 - 0xdf
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xe0 - 0xef
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, // 0xf0 - 0xff
        ];
        return lowerAlphabetTable.At(rawByte);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsPlusSign(byte rawByte)
        => rawByte == Symbol.PLUS;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsMinusSign(byte rawByte)
        => rawByte == Symbol.MINUS;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsPlusOrMinusSign(byte rawByte)
        => IsPlusSign(rawByte) || IsMinusSign(rawByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsBackSlash(byte rawByte)
        => rawByte == Symbol.BACKSLASH;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsHyphen(byte rawByte)
        => IsMinusSign(rawByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsColon(byte rawByte)
        => rawByte == Symbol.COLON;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsDot(byte rawByte)
        => rawByte == Symbol.DOT;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsExpSymbol(byte rawByte)
        => rawByte == Alphabet.e || rawByte == Alphabet.E;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsComma(byte rawByte)
        => rawByte == Symbol.COMMA;

    internal static EscapeSequenceResult TryParseEscapeSequence(ref Utf8SequenceReader sequenceReader, ArrayPoolBufferWriter<byte> bufferWriter, bool multiLine, bool throwError)
    {
        if (!sequenceReader.TryPeek(out var ch)) ExceptionHelper.ThrowEndOfFileReached();

        if (IsEscapeSequence(ch))
        {
            switch (ch)
            {
                case TomlCodes.Symbol.DOUBLEQUOTED:
                    sequenceReader.Advance(1);
                    bufferWriter.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                    return EscapeSequenceResult.Success;
                case TomlCodes.Symbol.SINGLEQUOTED:
                    sequenceReader.Advance(1);
                    bufferWriter.Write(TomlCodes.Symbol.SINGLEQUOTED);
                    return EscapeSequenceResult.Success;
                case TomlCodes.Symbol.BACKSLASH:
                    sequenceReader.Advance(1);
                    bufferWriter.Write(TomlCodes.Symbol.BACKSLASH);
                    return EscapeSequenceResult.Success;
                case TomlCodes.Alphabet.b:
                    sequenceReader.Advance(1);
                    bufferWriter.Write(TomlCodes.Symbol.BACKSPACE);
                    return EscapeSequenceResult.Success;
                case TomlCodes.Alphabet.t:
                    sequenceReader.Advance(1);
                    bufferWriter.Write(TomlCodes.Symbol.TAB);
                    return EscapeSequenceResult.Success;
                case TomlCodes.Alphabet.n:
                    sequenceReader.Advance(1);
                    bufferWriter.Write(TomlCodes.Symbol.LINEFEED);
                    return EscapeSequenceResult.Success;
                case TomlCodes.Alphabet.f:
                    sequenceReader.Advance(1);
                    bufferWriter.Write(TomlCodes.Symbol.FORMFEED);
                    return EscapeSequenceResult.Success;
                case TomlCodes.Alphabet.r:
                    sequenceReader.Advance(1);
                    bufferWriter.Write(TomlCodes.Symbol.CARRIAGE);
                    return EscapeSequenceResult.Success;
                case TomlCodes.Alphabet.u:
                    sequenceReader.Advance(1);
                    try
                    {
                        if (sequenceReader.TryFullSpan(4, out var source))
                        {
                            Span<byte> destination = stackalloc byte[4];
                            Utf8Helper.ParseFrom16bitCodePointToUtf8(destination, source, out int writtenCount);
                            bufferWriter.Write(destination.Slice(0, writtenCount));
                            return EscapeSequenceResult.Success;
                        }

                        Span<byte> sourceSpan = stackalloc byte[4];
                        if (sequenceReader.TryGetbytes(4, sourceSpan))
                        {
                            Span<byte> destination = stackalloc byte[4];
                            Utf8Helper.ParseFrom16bitCodePointToUtf8(destination, sourceSpan, out int writtenCount);
                            bufferWriter.Write(destination.Slice(0, writtenCount));
                            return EscapeSequenceResult.Success;
                        }
                        ExceptionHelper.ThrowIncorrect16bitCodePoint();
                    }
                    catch (CsTomlException)
                    {
                        if (throwError) throw;
                        return EscapeSequenceResult.Failure;
                    }
                    return EscapeSequenceResult.Success;
                case TomlCodes.Alphabet.U:
                    sequenceReader.Advance(1);
                    try
                    {
                        if (sequenceReader.TryFullSpan(8, out var source))
                        {
                            Span<byte> destination = stackalloc byte[8];
                            Utf8Helper.ParseFrom32bitCodePointToUtf8(destination, source, out int writtenCount);
                            bufferWriter.Write(destination.Slice(0, writtenCount));
                            return EscapeSequenceResult.Success;
                        }

                        Span<byte> sourceSpan = stackalloc byte[8];
                        if (sequenceReader.TryGetbytes(8, sourceSpan))
                        {
                            Span<byte> destination = stackalloc byte[8];
                            Utf8Helper.ParseFrom32bitCodePointToUtf8(destination, sourceSpan, out int writtenCount);
                            bufferWriter.Write(destination.Slice(0, writtenCount));
                            return EscapeSequenceResult.Success;
                        }
                        ExceptionHelper.ThrowIncorrect32bitCodePoint();
                    }
                    catch (CsTomlException)
                    {
                        if (throwError) throw;
                        return EscapeSequenceResult.Failure;
                    }
                    return EscapeSequenceResult.Success;
                default:
                    return EscapeSequenceResult.Failure;
            }
        }
        else
        {
            return multiLine ? EscapeSequenceResult.Unescaped : EscapeSequenceResult.Failure;
        }
    }
}

