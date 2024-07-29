
using System.Buffers;
using System.Text.Unicode;
using CsToml.Error;
using CsToml.Utility;

namespace CsToml.Formatter;

internal class StringFormatter : ICsTomlFormatter<string>, ICsTomlSpanFormatter<char>
{
    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, string value)
        where TBufferWriter : IBufferWriter<byte>
    {
        Serialize(ref writer, value.AsSpan());
    }

    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, ReadOnlySpan<char> value)
        where TBufferWriter : IBufferWriter<byte>
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

    public static string Deserialize(ref Utf8Reader reader, int length)
    {
        if (length == 0) return string.Empty;

        return DeserializeCore(reader.ReadBytes(length));
    }

    private static string DeserializeCore(ReadOnlySpan<byte> utf8Bytes)
    {
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

            return new string(bufferBytesSpan[..charsWritten]);
        }
        else
        {
            using var bufferWriter = new ArrayPoolBufferWriter<char>();
            var bufferBytesSpan = bufferWriter.GetSpan(maxBufferSize);

            var status = Utf8.ToUtf16(utf8Bytes, bufferBytesSpan, out var bytesRead, out var charsWritten, replaceInvalidSequences: false);
            if (status != OperationStatus.Done)
            {
                if (status == OperationStatus.InvalidData)
                    ExceptionHelper.ThrowInvalidByteIncluded();
                ExceptionHelper.ThrowBufferTooSmallFailed();
            }

            bufferWriter.Advance(charsWritten);
            return new string(bufferWriter.WrittenSpan);
        }
    }

}
