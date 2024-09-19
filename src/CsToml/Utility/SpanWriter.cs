using CsToml.Error;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Utility;

internal ref struct SpanWriter(Span<byte> buffer)
{
    private readonly Span<byte> source = buffer;
    private readonly ref byte refSource = ref MemoryMarshal.GetReference(buffer);
    private int written = 0;

    public readonly int Written => written;

    public readonly Span<byte> WrittenSpan => source[..written];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte ch)
    {
        if (written >= source.Length)
        {
            ExceptionHelper.ThrowOverflowDuringParsingOfNumericTypes();
            return;
        }

        ref var v = ref Unsafe.Add(ref refSource, written++);
        v = ch;
    }

}