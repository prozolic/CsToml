using CsToml.Error;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Utility;

internal ref struct Utf8ArrayPoolWriter(int initialCapacity) : IUtf8Writer, IDisposable
{
    private byte[] buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
    private int index = 0;

    public readonly long WrittenLength => index;

    public readonly ReadOnlySpan<byte> WrittenSpan => buffer.AsSpan(0, index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        var index = this.index;
        if ((uint)count > (uint)(buffer.Length - index))
        {
            ExceptionHelper.ThrowInvalidAdvance();
        }

        this.index = index + count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetSpan(int sizeHint = 0)
    {
        if (sizeHint == 0)
        {
            sizeHint = 1;
        }

        var buffer = this.buffer;
        var index = this.index;
        if ((uint)sizeHint > (uint)(buffer.Length - index))
        {
            Grow(sizeHint);
            buffer = this.buffer;
        }

        return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(buffer), (nint)(uint)index), buffer.Length - index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetWrittenSpan(int writtingLength = 1)
    {
        var buffer = this.buffer;
        var index = this.index;
        if ((uint)writtingLength > (uint)(buffer.Length - index))
        {
            Grow(writtingLength);
            buffer = this.buffer;
        }

        this.index = index + writtingLength;
        return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(buffer), (nint)(uint)index), writtingLength);
    }

    public void Dispose()
    {
        var buffer = this.buffer;
        this.buffer = [];
        index = 0;

        if (buffer.Length > 0)
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int sizeHint)
    {
        var buffer = this.buffer;
        var index = this.index;
        if ((uint)sizeHint > (uint)(Array.MaxLength - index))
        {
            ExceptionHelper.ThrowBufferCapacityExceeded();
        }

        var newSize = (int)Math.Min((uint)buffer.Length * 2, (uint)Array.MaxLength);
        if (newSize < index + sizeHint)
        {
            newSize = index + sizeHint;
        }

        var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
        buffer.AsSpan(0, index).CopyTo(newBuffer);
        this.buffer = newBuffer;

        if (buffer.Length > 0)
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
