
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Utility;

internal ref struct SpanWriter(Span<byte> buffer)
{
    private readonly Span<byte> source = buffer;
    private int written = 0;

    public Span<byte> WrittenSpan => source[..written];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte ch)
    {
        ref var v = ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(source), written++);
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