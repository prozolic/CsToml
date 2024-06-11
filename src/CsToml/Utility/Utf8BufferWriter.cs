using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal ref struct Utf8BufferWriter<TBufferWriter>(ref TBufferWriter writer)
    where TBufferWriter : IBufferWriter<byte>
{
    private readonly ref TBufferWriter bufferWriter = ref writer;
    private Span<byte> currentBufferSpan = [];
    private int writtingBufferIndex = 0;

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Write(byte buffer)
    {
        if (currentBufferSpan.Length <= writtingBufferIndex)
        {
            currentBufferSpan = bufferWriter.GetSpan(1);
            writtingBufferIndex = 0;
        }
        currentBufferSpan[writtingBufferIndex++] = buffer;
        bufferWriter.Advance(1);
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Flush()
    {
        currentBufferSpan = bufferWriter.GetSpan();
        writtingBufferIndex = 0;
    }

}

