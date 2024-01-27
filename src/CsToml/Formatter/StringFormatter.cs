
using System.Buffers;
using System.ComponentModel.DataAnnotations;
using System.Text.Unicode;
using CsToml.Utility;

namespace CsToml.Formatter;

internal class StringFormatter : ICsTomlFormatter<string>
{
    public static void Serialize(ref Utf8Writer writer, string value)
    {
        var maxBufferSize = value.Length * 2;

        var status = Utf8.FromUtf16(value.AsSpan(), writer.GetSpan(maxBufferSize),
            out int charsRead, out int bytesWritten, replaceInvalidSequences: false);

        if (status != OperationStatus.Done)
        {
            throw new Exception();
        }
        writer.Advance(bytesWritten);
    }

    public static string Deserialize(ref Utf8Reader reader, int length)
    {
        var bytes = reader.ReadBytes(length);

        return bytes.Length > 0 ? DeserializeCore(bytes) : string.Empty;
    }

    private static string DeserializeCore(ReadOnlySpan<byte> utf8Bytes)
    {
        // buffer size to 3 times worst-case (UTF8 -> UTF16)
        var maxBufferSize = (utf8Bytes.Length * 3) / 2 + 1;

        if (maxBufferSize <= 128)
        {
            Span<char> bufferBytesSpan = stackalloc char[maxBufferSize];
            var status = Utf8.ToUtf16(utf8Bytes, bufferBytesSpan, out var bytesRead, out var charsWritten, replaceInvalidSequences: false);
            if (status != OperationStatus.Done)
            {
                throw new Exception();
            }
            return new string(bufferBytesSpan[..charsWritten]);
        }
        else
        {
            var bufferBytes = ArrayPool<char>.Shared.Rent(maxBufferSize);
            try
            {
                var bufferBytesSpan = bufferBytes.AsSpan();
                var status = Utf8.ToUtf16(utf8Bytes, bufferBytesSpan, out var bytesRead, out var charsWritten, replaceInvalidSequences: false);
                if (status != OperationStatus.Done)
                {
                    throw new Exception();
                }
                return new string(bufferBytesSpan[..charsWritten]);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(bufferBytes);
            }
        }
    }

}