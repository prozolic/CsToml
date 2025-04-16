using CsToml.Error;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;

namespace CsToml.Utility;

internal static class Utf8Helper
{
    public static bool ContainInvalidSequences(ReadOnlySpan<byte> bytes)
    {
        ref var refBytes = ref MemoryMarshal.GetReference(bytes);

        if (Vector.IsHardwareAccelerated && bytes.Length >= Vector<byte>.Count)
        {
            var vectorSize = Vector<byte>.Count;
            var vector0 = Vector<byte>.Zero;
            var vector80 = new Vector<byte>(0x80);

            for (int i = 0; i < bytes.Length;)
            {
                ref var b1 = ref Unsafe.Add(ref refBytes, i);
                if ((b1 & 0x80) == 0x00) // 1
                {
                    // Only partially SIMD.
                    if ((uint)(bytes.Length - i) >= (uint)vectorSize)
                    {
                        var vector = Vector.LoadUnsafe<byte>(ref refBytes, (uint)(i));
                        if ((vector & vector80) == vector0)
                        {
                            i += vectorSize;
                            continue;
                        }
                    }

                    if (bytes.Length - i >= 8)
                    {
                        var block = Unsafe.ReadUnaligned<ulong>(ref b1);
                        if ((block & 0x80808080_80808080) == 0x00)
                        {
                            i += 8;
                            continue;
                        }
                    }

                    if (bytes.Length - i >= 4)
                    {
                        var block = Unsafe.ReadUnaligned<uint>(ref b1);
                        if ((block & 0x80808080) == 0x00)
                        {
                            i += 4;
                            continue;
                        }
                    }
                    i++;
                }
                else if (((b1 & 0xe0) == 0xc0)) // 2
                {
                    if ((uint)bytes.Length <= (uint)(i + 1)) return true;

                    var b2 = Unsafe.Add(ref refBytes, i + 1);
                    if ((b2 & 0xc0) != 0x80) return true;

                    // check codePoint
                    var codePoint = (uint)(b1 & 0x1f) << 6 | (uint)(b2 & 0x3f);
                    if ((codePoint < 0x80) || (0x7ff < codePoint))
                    {
                        return true;
                    }
                    i += 2;
                }
                else if (((b1 & 0xf0) == 0xe0)) // 3
                {
                    if ((uint)bytes.Length <= (uint)(i + 2)) return true;

                    var b2 = Unsafe.Add(ref refBytes, i + 1);
                    var b3 = Unsafe.Add(ref refBytes, i + 2);
                    // check codePoint
                    var codePoint = (uint)(b1 & 0x0f) << 12 | (uint)(b2 & 0x3f) << 6 | (uint)(b3 & 0x3f);
                    if ((codePoint < 0x800) || (0xffff < codePoint) || (0xd7ff < codePoint && codePoint < 0xe000))
                    {
                        return true;
                    }

                    if (b1 == 0xe0)
                    {
                        if (0x7f < b2 && b2 < 0xa0) return true;
                    }
                    else if (b1 == 0xed) // surrogate pair
                    {
                        if (0x9f < b2) return true;
                    }
                    if ((b2 & 0xc0) != 0x80) return true;
                    if ((b3 & 0xc0) != 0x80) return true;

                    i += 3;
                }
                else if (((b1 & 0xf8) == 0xf0)) // 4
                {
                    if ((uint)bytes.Length <= (uint)(i + 3)) return true;

                    var b2 = Unsafe.Add(ref refBytes, i + 1);
                    var b3 = Unsafe.Add(ref refBytes, i + 2);
                    var b4 = Unsafe.Add(ref refBytes, i + 3);
                    if (b1 == 0xf0)
                    {
                        if (0x7f < b2 && b2 < 0x90) return true;
                    }
                    if ((b2 & 0xc0) != 0x80) return true;
                    if ((b3 & 0xc0) != 0x80) return true;
                    if ((b4 & 0xc0) != 0x80) return true;

                    // check codePoint
                    var codePoint = (uint)(b1 & 0x07) << 18 | (uint)(b2 & 0x3f) << 12 | (uint)(b3 & 0x3f) << 6 | (uint)(b4 & 0x3f);
                    if (codePoint <= 0xffff || 0x10ffff < codePoint)
                    {
                        return true;
                    }

                    i += 4;
                }
                else
                {
                    return true;
                }
            }
        }
        else
        {
            for (int i = 0; i < bytes.Length;)
            {
                ref var b1 = ref Unsafe.Add(ref refBytes, i);
                if ((b1 & 0x80) == 0x00) // 1
                {
                    if (bytes.Length - i >= 8)
                    {
                        var block = Unsafe.ReadUnaligned<ulong>(ref b1);
                        if ((block & 0x80808080_80808080) == 0x00)
                        {
                            i += 8;
                            continue;
                        }
                    }

                    if (bytes.Length - i >= 4)
                    {
                        var block = Unsafe.ReadUnaligned<uint>(ref b1);
                        if ((block & 0x80808080) == 0x00)
                        {
                            i += 4;
                            continue;
                        }
                    }
                    i++;
                }
                else if (((b1 & 0xe0) == 0xc0)) // 2
                {
                    if ((uint)bytes.Length <= (uint)(i + 1)) return true;

                    var b2 = Unsafe.Add(ref refBytes, i + 1);
                    if ((b2 & 0xc0) != 0x80) return true;

                    // check codePoint
                    var codePoint = (uint)(b1 & 0x1f) << 6 | (uint)(b2 & 0x3f);
                    if ((codePoint < 0x80) || (0x7ff < codePoint))
                    {
                        return true;
                    }
                    i += 2;
                }
                else if (((b1 & 0xf0) == 0xe0)) // 3
                {
                    if ((uint)bytes.Length <= (uint)(i + 2)) return true;

                    var b2 = Unsafe.Add(ref refBytes, i + 1);
                    var b3 = Unsafe.Add(ref refBytes, i + 2);
                    // check codePoint
                    var codePoint = (uint)(b1 & 0x0f) << 12 | (uint)(b2 & 0x3f) << 6 | (uint)(b3 & 0x3f);
                    if ((codePoint < 0x800) || (0xffff < codePoint) || (0xd7ff < codePoint && codePoint < 0xe000))
                    {
                        return true;
                    }

                    if (b1 == 0xe0)
                    {
                        if (0x7f < b2 && b2 < 0xa0) return true;
                    }
                    else if (b1 == 0xed) // surrogate pair
                    {
                        if (0x9f < b2) return true;
                    }
                    if ((b2 & 0xc0) != 0x80) return true;
                    if ((b3 & 0xc0) != 0x80) return true;

                    i += 3;
                }
                else if (((b1 & 0xf8) == 0xf0)) // 4
                {
                    if ((uint)bytes.Length <= (uint)(i + 3)) return true;

                    var b2 = Unsafe.Add(ref refBytes, i + 1);
                    var b3 = Unsafe.Add(ref refBytes, i + 2);
                    var b4 = Unsafe.Add(ref refBytes, i + 3);
                    if (b1 == 0xf0)
                    {
                        if (0x7f < b2 && b2 < 0x90) return true;
                    }
                    if ((b2 & 0xc0) != 0x80) return true;
                    if ((b3 & 0xc0) != 0x80) return true;
                    if ((b4 & 0xc0) != 0x80) return true;

                    // check codePoint
                    var codePoint = (uint)(b1 & 0x07) << 18 | (uint)(b2 & 0x3f) << 12 | (uint)(b3 & 0x3f) << 6 | (uint)(b4 & 0x3f);
                    if (codePoint <= 0xffff || 0x10ffff < codePoint)
                    {
                        return true;
                    }

                    i += 4;
                }
                else
                {
                    return true;
                }
            }
        }


        return false;
    }

    public static bool ContainInvalidSequencesInUnquotedKey(ReadOnlySpan<byte> bytes)
    {
        ref var refBytes = ref MemoryMarshal.GetReference(bytes);
        for (int i = 0; i < bytes.Length;)
        {
            ref var b1 = ref Unsafe.Add(ref refBytes, i);
            if ((b1 & 0x80) == 0x00) // U+0000 - U+007F 1byte
            {
                if (!TomlCodes.IsBareKey(b1))
                {
                    return true;
                }
                i++;
            }
            else if (((b1 & 0xe0) == 0xc0)) // U+0080 - U+07FF 2byte
            {
                if ((uint)bytes.Length <= (uint)(i + 1)) return true;

                var b2 = Unsafe.Add(ref refBytes, i + 1);
                if ((b2 & 0xc0) != 0x80) return true;

                // check codePoint
                var codePoint = (uint)(b1 & 0x1f) << 6 | (uint)(b2 & 0x3f);
                if ((codePoint < 0x80) || (0x7ff < codePoint))
                {
                    return true;
                }

                if (!(codePoint == 0xb2 || codePoint == 0xb3 || codePoint == 0xb9 || (codePoint >= 0xbc && codePoint <= 0xbe) ||
                    (codePoint >= 0xc0 && codePoint <= 0xd6) || (codePoint >= 0xd8 && codePoint <= 0xf6) ||
                    (codePoint >= 0xf8 && codePoint <= 0x37d) || (codePoint >= 0x37f && codePoint <= 0x1fff)))
                {
                    return true;
                }

                i += 2;
            }
            else if (((b1 & 0xf0) == 0xe0)) // U+0800 - U+FFFF 3byte
            {
                if ((uint)bytes.Length <= (uint)(i + 2)) return true;

                var b2 = Unsafe.Add(ref refBytes, i + 1);
                var b3 = Unsafe.Add(ref refBytes, i + 2);
                // check codePoint
                var codePoint = (uint)(b1 & 0x0f) << 12 | (uint)(b2 & 0x3f) << 6 | (uint)(b3 & 0x3f);
                if ((codePoint < 0x800) || (0xffff < codePoint) || (0xd7ff < codePoint && codePoint < 0xe000))
                {
                    return true;
                }

                if (b1 == 0xe0)
                {
                    if (0x7f < b2 && b2 < 0xa0) return true;
                }
                else if (b1 == 0xed) // surrogate pair
                {
                    if (0x9f < b2) return true;
                }
                if ((b2 & 0xc0) != 0x80) return true;
                if ((b3 & 0xc0) != 0x80) return true;

                if (!((codePoint >= 0x800 && codePoint <= 0x1fff) || (codePoint >= 0x200c && codePoint <= 0x200d) ||
                    (codePoint >= 0x203f && codePoint <= 0x2040) || (codePoint >= 0x203f && codePoint <= 0x2040) ||
                    (codePoint >= 0x2070 && codePoint <= 0x218f) || (codePoint >= 0x2460 && codePoint <= 0x24ff) ||
                    (codePoint >= 0x2c00 && codePoint <= 0x2fef) || (codePoint >= 0x3001 && codePoint <= 0xd7ff) ||
                    (codePoint >= 0xf900 && codePoint <= 0xfdcf) || (codePoint >= 0xfdf0 && codePoint <= 0xfffd)))
                {
                    return true;
                }

                i += 3;
            }
            else if (((b1 & 0xf8) == 0xf0)) // U+10000 - U+10FFFF 4byte
            {
                if ((uint)bytes.Length <= (uint)(i + 3)) return true;

                var b2 = Unsafe.Add(ref refBytes, i + 1);
                var b3 = Unsafe.Add(ref refBytes, i + 2);
                var b4 = Unsafe.Add(ref refBytes, i + 3);
                if (b1 == 0xf0)
                {
                    if (0x7f < b2 && b2 < 0x90) return true;
                }
                if ((b2 & 0xc0) != 0x80) return true;
                if ((b3 & 0xc0) != 0x80) return true;
                if ((b4 & 0xc0) != 0x80) return true;

                // check codePoint
                var codePoint = (uint)(b1 & 0x07) << 18 | (uint)(b2 & 0x3f) << 12 | (uint)(b3 & 0x3f) << 6 | (uint)(b4 & 0x3f);
                if (codePoint <= 0xffff || 0x10ffff < codePoint)
                {
                    return true;
                }

                if (!(codePoint >= 0x10000 && codePoint <= 0xeffff))
                {
                    return true;
                }

                i += 4;
            }
            else
            {
                return true;
            }
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

    public static void ParseFrom8bitCodePointToUtf8(Span<byte> destination, ReadOnlySpan<byte> source, out int writtenCount)
    {
        if (destination.Length < 2)
        {
            writtenCount = 0;
            ExceptionHelper.ThrowException("Number of elements in the destination Span<byte> is not 2.");
        }
        if (source.Length != 2)
        {
            writtenCount = 0;
            ExceptionHelper.ThrowException("Number of elements in the source ReadOnlySpan<byte> is not 2.");
        }

        for (var i = 0; i < source.Length; i++)
        {
            if (!TomlCodes.IsHex(source[i]))
            {
                writtenCount = 0;
                ExceptionHelper.ThrowIncorrectCompactEscapeCharacters(source[i]);
            }
        }

        var codePoint = 0;
        codePoint += (TomlCodes.Number.ParseHex(source[0]) << 4);
        codePoint += TomlCodes.Number.ParseHex(source[1]);

        Utf8Helper.ParseFromCodePointToUtf8(codePoint, destination, out writtenCount);
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

        for (var i = 0; i < source.Length; i++)
        {
            if (!TomlCodes.IsHex(source[i]))
            {
                writtenCount = 0;
                ExceptionHelper.ThrowIncorrectCompactEscapeCharacters(source[i]);
            }
        }

        var codePoint = 0;
        codePoint += (TomlCodes.Number.ParseHex(source[0]) << 12);
        codePoint += (TomlCodes.Number.ParseHex(source[1]) << 8);
        codePoint += (TomlCodes.Number.ParseHex(source[2]) << 4);
        codePoint +=  TomlCodes.Number.ParseHex(source[3]);

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
            if (!TomlCodes.IsHex(source[i]))
            {
                writtenCount = 0;
                ExceptionHelper.ThrowIncorrectCompactEscapeCharacters(source[i]);
            }
        }

        var codePoint = 0;
        codePoint += (TomlCodes.Number.ParseHex(source[0]) << 28);
        codePoint += (TomlCodes.Number.ParseHex(source[1]) << 24);
        codePoint += (TomlCodes.Number.ParseHex(source[2]) << 20);
        codePoint += (TomlCodes.Number.ParseHex(source[3]) << 16);
        codePoint += (TomlCodes.Number.ParseHex(source[4]) << 12);
        codePoint += (TomlCodes.Number.ParseHex(source[5]) << 8);
        codePoint += (TomlCodes.Number.ParseHex(source[6]) << 4);
        codePoint += TomlCodes.Number.ParseHex(source[7]);

        Utf8Helper.ParseFromCodePointToUtf8(codePoint, destination, out writtenCount);
    }

    public static bool ContainsEscapeChar(ReadOnlySpan<byte> bytes, bool newline)
    {
        if (bytes.ContainsAnyInRange((byte)0x0, (byte)0x8))
            return true;
        if (bytes.ContainsAnyInRange((byte)0xa, (byte)0x1f))
        {
            if (newline && (bytes.Contains((byte)0xa) || bytes.Contains((byte)0xd)))
            {
                return false;
            }
            return true;
        }
        if (bytes.Contains((byte)0x7F))
            return true;

        return false;
    }

    public static void FromUtf16(IBufferWriter<byte> writer, ReadOnlySpan<char> value)
    {
        // buffer size to 3 times worst-case (UTF16 -> UTF8)
        var maxBufferSize = (value.Length + 1) * 3;

        var status = Utf8.FromUtf16(value, writer.GetSpan(maxBufferSize),
            out int charsRead, out int bytesWritten, replaceInvalidSequences: false);

        if (status != OperationStatus.Done)
        {
            if (status == OperationStatus.InvalidData)
                ExceptionHelper.ThrowInvalidByteIncluded();
            ExceptionHelper.ThrowBufferTooSmallFailed();
        }
        writer.Advance(bytesWritten);
    }

    public static string ToUtf16(ReadOnlySpan<byte> utf8Bytes)
    {
        if (utf8Bytes.Length == 0)
        {
            return string.Empty;
        }

        var maxBufferSize = utf8Bytes.Length * 2;
        if (maxBufferSize <= 1024)
        {
            Span<char> bufferBytesSpan = stackalloc char[maxBufferSize];
            var status = Utf8.ToUtf16(utf8Bytes, bufferBytesSpan, out var bytesRead, out var charsWritten, replaceInvalidSequences: false);
            if (status != OperationStatus.Done)
            {
                if (status == OperationStatus.InvalidData)
                    ExceptionHelper.ThrowInvalidByteIncluded();
                ExceptionHelper.ThrowBufferTooSmallFailed();
            }

            ref char bufferReference = ref MemoryMarshal.GetReference(bufferBytesSpan);
            unsafe
            {
                fixed (char* ptr = &bufferReference)
                {
                    var str = string.Create(charsWritten, ((IntPtr)ptr, charsWritten), static (destination, state) =>
                    {
                        var source = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<char>((char*)state.Item1), state.charsWritten);
                        source.CopyTo(destination);
                    });
                    return str;
                }
            }
        }
        else
        {
            var bufferWriter = RecycleArrayPoolBufferWriter<char>.Rent();
            var bufferBytesSpan = bufferWriter.GetSpan(maxBufferSize);
            try
            {
                var status = Utf8.ToUtf16(utf8Bytes, bufferBytesSpan, out var bytesRead, out var charsWritten, replaceInvalidSequences: false);
                if (status != OperationStatus.Done)
                {
                    if (status == OperationStatus.InvalidData)
                        ExceptionHelper.ThrowInvalidByteIncluded();
                    ExceptionHelper.ThrowBufferTooSmallFailed();
                }

                bufferWriter.Advance(charsWritten);
                ref char bufferReference = ref MemoryMarshal.GetReference(bufferBytesSpan);
                unsafe
                {
                    fixed (char* ptr = &bufferReference)
                    {
                        var str = string.Create(charsWritten, ((IntPtr)ptr, charsWritten), static (destination, state) =>
                        {
                            var source = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<char>((char*)state.Item1), state.charsWritten);
                            source.CopyTo(destination);
                        });
                        return str;
                    }
                }
            }
            finally
            {
                RecycleArrayPoolBufferWriter<char>.Return(bufferWriter);
            }
        }
    }
}
