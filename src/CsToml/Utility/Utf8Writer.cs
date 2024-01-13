
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal ref struct Utf8Writer(IBufferWriter<byte> writer)
{
    private IBufferWriter<byte> bufferWriter = writer;

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte buffer)
    {
        GetSpan(1)[0] = buffer;
        Advance(1);
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0) return;

        var span = GetSpan(bytes.Length);

        for (int i = 0; i < bytes.Length; i++)
        {
            span[i] = bytes[i];
        }

        Advance(bytes.Length);
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetSpan(int writtingLength)
    {
        return bufferWriter.GetSpan(writtingLength)[..writtingLength];
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetWriteSpan(int writtingLength)
    {
        var span = GetSpan(writtingLength);
        Advance(writtingLength);
        return span;
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        bufferWriter.Advance(count);
    }
}

