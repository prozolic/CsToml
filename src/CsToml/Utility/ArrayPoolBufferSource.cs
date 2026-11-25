using CsToml.Error;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal struct ArrayPoolBufferSource
{
    private byte[]? buffer;

    public ArrayPoolBufferSource(int initialCapacity)
    {
        buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
    }

    public readonly bool IsActive => buffer is not null;

    public readonly Span<byte> Buffer => buffer;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Span<byte> Grow(int written, int sizeHint)
    {
        var oldBuffer = buffer;
        if (oldBuffer is null)
        {
            ExceptionHelper.ThrowBufferTooSmallFailed();
        }

        if ((uint)sizeHint > (uint)(Array.MaxLength - written))
        {
            ExceptionHelper.ThrowBufferCapacityExceeded();
        }

        var newSize = (int)Math.Min((uint)oldBuffer.Length * 2, (uint)Array.MaxLength);
        if (newSize < written + sizeHint)
        {
            newSize = written + sizeHint;
        }

        var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
        oldBuffer.AsSpan(0, written).CopyTo(newBuffer);
        buffer = newBuffer;

        if (oldBuffer.Length > 0)
        {
            ArrayPool<byte>.Shared.Return(oldBuffer);
        }

        return newBuffer;
    }

    public void Dispose()
    {
        var array = buffer;
        buffer = null;
        if (array is not null && array.Length > 0)
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }
}
