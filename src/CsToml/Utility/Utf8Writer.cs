
using CsToml.Extension;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal ref struct Utf8Writer<TBufferWriter>(ref TBufferWriter bufferWriter)
    where TBufferWriter : IBufferWriter<byte>
{
    private ref TBufferWriter bufferWriter = ref bufferWriter;
    private int written = 0;

    public readonly int WrittenSize => written;

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte @byte)
    {
        GetSpan(1).At(0) = @byte;
        Advance(1);
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0) return;

        bytes.CopyTo(GetSpan(bytes.Length)[..bytes.Length]);
        Advance(bytes.Length);
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetSpan(int writtingLength)
    {
        return bufferWriter.GetSpan(writtingLength);
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetWrittenSpan(int writtingLength)
    {
        var span = GetSpan(writtingLength)[..writtingLength];
        Advance(writtingLength);
        return span;
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        bufferWriter.Advance(count);
        written += count;
    }
}

