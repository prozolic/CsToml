using CsToml.Extension;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Utility;

internal ref struct SpanWriter(Span<byte> buffer)
{
    private readonly Span<byte> source = buffer;
    private readonly ref byte refSource = ref MemoryMarshal.GetReference(buffer);
    private int written = 0;

    public Span<byte> WrittenSpan => source[..written];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte ch)
    {
        ref var v = ref Unsafe.Add(ref refSource, written++);
        v = ch;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryWrite(byte ch)
    {
        if (written >= source.Length) return false;

        Write(ch);
        return true;
    }

}