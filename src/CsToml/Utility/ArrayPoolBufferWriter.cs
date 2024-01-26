
using CsToml.Error;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Utility;

internal sealed class ArrayPoolBufferWriter<T> : IBufferWriter<T>, IDisposable
    where T : struct
{
    private const int ArrayMaxLength = 0x7FFFFFC7;

    private T[] buffer;
    private int index;
    private bool isRent = true;

    public ReadOnlySpan<T> WrittenSpan => buffer.AsSpan(0, index);

    public ArrayPoolBufferWriter()
    {
        buffer = [];
        index = 0;
        isRent = false;
    }

    public ArrayPoolBufferWriter(int initialCapacity)
    {
        buffer = ArrayPool<T>.Shared.Rent(initialCapacity);
        index = 0;
        isRent = true;
    }

    public void Advance(int count)
    {
        if (index > buffer.Length - count)
            ExceptionHelper.ThrowInvalidAdvance();

        index += count;
    }

    public Memory<T> GetMemory(int sizeHint = 0)
    {
        throw new NotSupportedException();
    }

    public Span<T> GetSpan(int sizeHint = 0)
    {
        Reserve(sizeHint);
        return buffer.AsSpan(index);
    }

    public void Clear()
        => index = 0;

    public void Reserve(int sizeHint)
    {
        if (sizeHint <= buffer.Length - index) return;

        var newSize = buffer.Length * 2;
        if (sizeHint > newSize)
        {
            newSize = CalculateBufferSize(sizeHint);
        }
        newSize = Math.Min(newSize, ArrayMaxLength);

        var newBuffer = ArrayPool<T>.Shared.Rent(newSize);
        try
        {
            var byteCount = Unsafe.SizeOf<T>() * index;
            ref var source = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetArrayDataReference(buffer)!);
            ref var dest = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetArrayDataReference(newBuffer)!);
            Unsafe.CopyBlockUnaligned(ref dest, ref source, (uint)byteCount);
        }
        finally
        {
            Return();
            buffer = newBuffer;
            isRent = true;
        }

    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private int CalculateBufferSize(int capacity)
    {
        capacity -= 1;
        capacity |= capacity >> 1;
        capacity |= capacity >> 2;
        capacity |= capacity >> 4;
        capacity |= capacity >> 8;
        capacity |= capacity >> 16;

        return capacity + 1;
    }

    public void Return()
    {
        if (isRent)
        {
            isRent = false;
            ArrayPool<T>.Shared.Return(buffer);
        }
    }

    public void Dispose()
        => Return();
}