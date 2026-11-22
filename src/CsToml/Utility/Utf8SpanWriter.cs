using CsToml.Error;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Utility;

internal ref struct Utf8SpanWriter(Span<byte> destination) : IUtf8Writer
{
    private readonly Span<byte> destination = destination;
    private int written = 0;

    public readonly long WrittenLength => written;

    public readonly ReadOnlySpan<byte> WrittenSpan => destination[..written];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        var written = this.written;
        if ((uint)count > (uint)(destination.Length - written))
        {
            ExceptionHelper.ThrowInvalidAdvance();
        }

        this.written = written + count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<byte> GetSpan(int sizeHint = 0)
    {
        var written = this.written;
        var freeSize = destination.Length - written;
        if ((uint)sizeHint > (uint)freeSize)
        {
            ExceptionHelper.ThrowBufferTooSmallFailed();
        }

        return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(destination), (nint)(uint)written), freeSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetWrittenSpan(int writtingLength = 1)
    {
        var written = this.written;
        if ((uint)writtingLength > (uint)(destination.Length - written))
        {
            ExceptionHelper.ThrowBufferTooSmallFailed();
        }

        this.written = written + writtingLength;
        return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(destination), (nint)(uint)written), writtingLength);
    }
}
