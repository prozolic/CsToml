
using System.Buffers;
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

        using var writer = new ArrayPoolBufferWriter<char>(maxBufferSize);
        var status = Utf8.ToUtf16(utf8Bytes, writer.GetSpan(maxBufferSize), out var bytesRead, out var charsWritten, replaceInvalidSequences: false);

        if (status != OperationStatus.Done)
        {
            throw new Exception();
        }
        writer.Advance(charsWritten);
        return new string(writer.WrittenSpan);
    }

}