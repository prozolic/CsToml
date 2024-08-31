using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;
using System.Buffers;
using System.Text.Unicode;

namespace CsToml.Values;

internal partial class TomlString
{
    public static TomlString Parse(ReadOnlySpan<byte> utf16String)
    {
        if (Utf8Helper.ContainInvalidSequences(utf16String))
            ExceptionHelper.ThrowInvalidCodePoints();

        // check newline
        if (utf16String.Contains(TomlCodes.Symbol.LINEFEED))
        {
            if (Utf8Helper.ContainsEscapeChar(utf16String, true))
            {
                return TomlString.Parse(utf16String, CsTomlStringType.MultiLineLiteral);
            }
            return TomlString.Parse(utf16String, CsTomlStringType.MultiLineBasic);
        }

        // check escape
        if (Utf8Helper.ContainsEscapeChar(utf16String, true))
        {
            return TomlString.Parse(utf16String, CsTomlStringType.Literal);
        }

        if (utf16String.Contains(TomlCodes.Symbol.BACKSLASH) && !utf16String.Contains(TomlCodes.Symbol.SINGLEQUOTED))
        {
            return TomlString.Parse(utf16String, CsTomlStringType.Basic);
        }

        if (utf16String.Contains(TomlCodes.Symbol.DOUBLEQUOTED))
        {
            if (!utf16String.Contains(TomlCodes.Symbol.SINGLEQUOTED))
            {
                return TomlString.Parse(utf16String, CsTomlStringType.Literal);
            }
            return TomlString.Parse(utf16String, CsTomlStringType.MultiLineLiteral);
        }

        return TomlString.Parse(utf16String, CsTomlStringType.Basic);
    }

    public static TomlString Parse(ReadOnlySpan<char> utf16String)
    {
        if (utf16String.Length == 0) return new TomlString(string.Empty, CsTomlStringType.Basic);

        var writer = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            Utf8Helper.FromUtf16(writer, utf16String);
            return Parse(writer.WrittenSpan);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(writer);
        }
    }

    public static TomlString Parse(ReadOnlySpan<byte> bytes, CsTomlStringType type)
    {
        if (bytes.Length == 0)
        {
            return new TomlString(string.Empty, type);
        }

        return new TomlString(Utf8Helper.ToUtf16(bytes), type);
    }


    //internal static string ParseUnsafe(ReadOnlySpan<byte> utf8Bytes)
    //{
    //    var maxBufferSize = utf8Bytes.Length * 2;
    //    if (maxBufferSize <= 1024)
    //    {
    //        Span<char> bufferBytesSpan = stackalloc char[maxBufferSize];
    //        var status = Utf8.ToUtf16(utf8Bytes, bufferBytesSpan, out var bytesRead, out var charsWritten, replaceInvalidSequences: false);
    //        if (status != OperationStatus.Done)
    //        {
    //            if (status == OperationStatus.InvalidData)
    //                ExceptionHelper.ThrowInvalidByteIncluded();
    //            ExceptionHelper.ThrowBufferTooSmallFailed();
    //        }

    //        return new string(bufferBytesSpan[..charsWritten]);
    //    }
    //    else
    //    {
    //        var bufferWriter = RecycleArrayPoolBufferWriter<char>.Rent();
    //        var bufferBytesSpan = bufferWriter.GetSpan(maxBufferSize);
    //        try
    //        {
    //            var status = Utf8.ToUtf16(utf8Bytes, bufferBytesSpan, out var bytesRead, out var charsWritten, replaceInvalidSequences: false);
    //            if (status != OperationStatus.Done)
    //            {
    //                if (status == OperationStatus.InvalidData)
    //                    ExceptionHelper.ThrowInvalidByteIncluded();
    //                ExceptionHelper.ThrowBufferTooSmallFailed();
    //            }

    //            bufferWriter.Advance(charsWritten);
    //            return new string(bufferWriter.WrittenSpan);
    //        }
    //        finally
    //        {
    //            RecycleArrayPoolBufferWriter<char>.Return(bufferWriter);
    //        }
    //    }
    //}

}

