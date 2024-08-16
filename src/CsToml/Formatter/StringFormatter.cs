
using System.Buffers;
using System.Text.Unicode;
using CsToml.Error;
using CsToml.Utility;

namespace CsToml.Formatter;

internal class StringFormatter : ITomlValueFormatter<string>, ITomlValueSpanFormatter<char>
{
    public static readonly StringFormatter Default = new StringFormatter();

    public void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, string value)
        where TBufferWriter : IBufferWriter<byte>
    {
        Serialize(ref writer, value.AsSpan());
    }

    public void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, ReadOnlySpan<char> value)
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

    public void Deserialize(ReadOnlySpan<byte> bytes, ref string value)
    {
        if (bytes.Length == 0)
        {
            value = string.Empty;
            return;
        }

        DeserializeCore(bytes, ref value);
    }

    private void DeserializeCore(ReadOnlySpan<byte> utf8Bytes, ref string value)
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

            value =  new string(bufferBytesSpan[..charsWritten]);
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
                value = new string(bufferWriter.WrittenSpan);
            }
            finally
            {
                RecycleArrayPoolBufferWriter<char>.Return(bufferWriter);
            }
        }
    }

}
