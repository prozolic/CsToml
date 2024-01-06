using CsToml.Error;
using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal static class Utf8Helper
{
    public static int ParseUtf8(int utf32CodePoint, Span<byte> utf8Bytes)
    {
        // unicode -> utf8
        if (utf32CodePoint < 0x80)
        {
            utf8Bytes[0] = (byte)utf32CodePoint;
            return 1;
        }
        else if (utf32CodePoint < 0x800)
        {
            utf8Bytes[0] = (byte)(0xc0 | utf32CodePoint >> 6);
            utf8Bytes[1] = (byte)(0x80 | utf32CodePoint & 0x3f);
            return 2;
        }
        else if (utf32CodePoint < 0x10000)
        {
            utf8Bytes[0] = (byte)(0xe0 | utf32CodePoint >> 12);
            utf8Bytes[1] = (byte)(0x80 | (utf32CodePoint & 0xfc0) >> 6);
            utf8Bytes[2] = (byte)(0x80 | utf32CodePoint & 0x3f);
            return 3;
        }
        else if (utf32CodePoint < 0x110000)
        {
            utf8Bytes[0] = (byte)(0xF0 | utf32CodePoint >> 18);
            utf8Bytes[1] = (byte)(0x80 | (utf32CodePoint & 0x3F000) >> 12);
            utf8Bytes[2] = (byte)(0x80 | (utf32CodePoint & 0xFC0) >> 6);
            utf8Bytes[3] = (byte)(0x80 | utf32CodePoint & 0x3F);
            return 4;
        }

        return ExceptionHelper.NotReturnThrow<int>(ExceptionHelper.ThrowInvalidUnicodeScalarValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainBOM(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 3) return false;

        return bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf;
    }
}
