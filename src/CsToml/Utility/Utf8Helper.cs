using CsToml.Error;
using Microsoft.Win32.SafeHandles;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal static class Utf8Helper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainBOM(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 3) return false;

        return bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf;
    }

    public static bool ContainInvalidSequences(ReadOnlySpan<byte> bytes)
    {
        for (int i = 0; i < bytes.Length; i++)
        {
            if ((bytes[i] & 0x80) == 0x00) continue;
            else if (((bytes[i] & 0xe0) == 0xc0)) goto Two;
            else if (((bytes[i] & 0xf0) == 0xe0)) goto Three;
            else if (((bytes[i] & 0xf8) == 0xf0)) goto Four;
            else return true;

        Two:
            if (bytes.Length <= ++i) return true;
            if ((bytes[i] & 0xc0) != 0x80) return true;
            continue;

        Three:
            if (bytes.Length <= i + 2) return true;

            if (bytes[i] == 0xe0)
            {
                if (0x7f < bytes[i + 1] && bytes[i + 1] < 0xa0) return true;
            }
            else if (bytes[i] == 0xed) // surrogate pair
            {
                if (0x9f < bytes[i + 1]) return true;
            }
            if ((bytes[++i] & 0xc0) != 0x80) return true;
            if ((bytes[++i] & 0xc0) != 0x80) return true;
            continue;

        Four:
            if (bytes.Length <= i + 3) return true;

            if (bytes[i] == 0xf0)
            {
                if (0x7f < bytes[i + 1] && bytes[i + 1] < 0x90) return true;
            }
            if ((bytes[++i] & 0xc0) != 0x80) return true;
            if ((bytes[++i] & 0xc0) != 0x80) return true;
            if ((bytes[++i] & 0xc0) != 0x80) return true;
            continue;

        }

        return false;
    }

    public static void ParseFromCodePointToUtf8(int utf32CodePoint, Span<byte> utf8Bytes, out int writtenCount)
    {
        // unicode -> utf8
        if (utf32CodePoint < 0x80)
        {
            utf8Bytes[0] = (byte)utf32CodePoint;
            writtenCount = 1;
            return;
        }
        else if (utf32CodePoint < 0x800)
        {
            utf8Bytes[0] = (byte)(0xc0 | utf32CodePoint >> 6);
            utf8Bytes[1] = (byte)(0x80 | utf32CodePoint & 0x3f);
            writtenCount = 2;
            return;
        }
        else if (utf32CodePoint < 0x10000)
        {
            utf8Bytes[0] = (byte)(0xe0 | utf32CodePoint >> 12);
            utf8Bytes[1] = (byte)(0x80 | (utf32CodePoint & 0xfc0) >> 6);
            utf8Bytes[2] = (byte)(0x80 | utf32CodePoint & 0x3f);
            writtenCount = 3;
            return;
        }
        else if (utf32CodePoint < 0x110000)
        {
            utf8Bytes[0] = (byte)(0xF0 | utf32CodePoint >> 18);
            utf8Bytes[1] = (byte)(0x80 | (utf32CodePoint & 0x3F000) >> 12);
            utf8Bytes[2] = (byte)(0x80 | (utf32CodePoint & 0xFC0) >> 6);
            utf8Bytes[3] = (byte)(0x80 | utf32CodePoint & 0x3F);
            writtenCount = 4;
            return;
        }

        writtenCount = 0;
        ExceptionHelper.NotReturnThrow<int>(ExceptionHelper.ThrowInvalidUnicodeScalarValue);
    }

    public static void ParseFrom16bitCodePointToUtf8(Span<byte> destination, ReadOnlySpan<byte> source, out int writtenCount)
    {
        if (destination.Length < 4)
        {
            writtenCount = 0;
            ExceptionHelper.ThrowException("Number of elements in the destination Span<byte> is not 4.");
        }
        if (source.Length != 4)
        {
            writtenCount = 0;
            ExceptionHelper.ThrowException("Number of elements in the source ReadOnlySpan<byte> is not 4.");
        }

        for (var i = 0; i < source.Length - 1; i++)
        {
            if (!CsTomlSyntax.IsHex(source[i]))
            {
                writtenCount = 0;
                ExceptionHelper.ThrowIncorrectCompactEscapeCharacters(source[i]);
            }
        }

        var codePoint = 0;
        codePoint += (CsTomlSyntax.Number.ParseHex(source[0]) << 12);
        codePoint += (CsTomlSyntax.Number.ParseHex(source[1]) << 8);
        codePoint += (CsTomlSyntax.Number.ParseHex(source[2]) << 4);
        codePoint +=  CsTomlSyntax.Number.ParseHex(source[3]);

        Utf8Helper.ParseFromCodePointToUtf8(codePoint, destination, out writtenCount);
    }

    public static void ParseFrom32bitCodePointToUtf8(Span<byte> destination, ReadOnlySpan<byte> source, out int writtenCount)
    {
        if (destination.Length < 4)
        {
            writtenCount = 0;
            ExceptionHelper.ThrowException("Number of elements in the destination Span<byte> is not 4.");
        }
        if (source.Length != 8)
        {
            writtenCount = 0;
            ExceptionHelper.ThrowException("Number of elements in the source ReadOnlySpan<byte> is not 8.");
        }

        for (var i = 0; i < source.Length - 1; i++)
        {
            if (!CsTomlSyntax.IsHex(source[i]))
            {
                writtenCount = 0;
                ExceptionHelper.ThrowIncorrectCompactEscapeCharacters(source[i]);
            }
        }

        var codePoint = 0;
        codePoint += (CsTomlSyntax.Number.ParseHex(source[0]) << 28);
        codePoint += (CsTomlSyntax.Number.ParseHex(source[1]) << 24);
        codePoint += (CsTomlSyntax.Number.ParseHex(source[2]) << 20);
        codePoint += (CsTomlSyntax.Number.ParseHex(source[3]) << 16);
        codePoint += (CsTomlSyntax.Number.ParseHex(source[4]) << 12);
        codePoint += (CsTomlSyntax.Number.ParseHex(source[5]) << 8);
        codePoint += (CsTomlSyntax.Number.ParseHex(source[6]) << 4);
        codePoint += CsTomlSyntax.Number.ParseHex(source[7]);

        Utf8Helper.ParseFromCodePointToUtf8(codePoint, destination, out writtenCount);
    }

}