using CsToml.Error;
using CsToml.Extension;
using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Utility;

internal static class RecycleArrayPoolBufferWriter<T>
    where T : unmanaged
{
    private static readonly ConcurrentQueue<ArrayPoolBufferWriter<T>> writerQueue = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ArrayPoolBufferWriter<T> Rent(int initialCapacity = 1024)
    {
        if (writerQueue.TryDequeue(out var buffer))
        {
            buffer.ReserveDangerous(initialCapacity);
            return buffer;
        }
        return new ArrayPoolBufferWriter<T>(initialCapacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Return(ArrayPoolBufferWriter<T> buffer)
    {
        buffer.Return();
        writerQueue.Enqueue(buffer);
    }
}


internal sealed class ArrayPoolBufferWriter<T> : IBufferWriter<T>, IDisposable
    where T : unmanaged
{
    private T[] buffer = [];
    private int index;

    public ReadOnlySpan<T> WrittenSpan => buffer.AsSpan(0, index);

    public ArrayPoolBufferWriter(int initialCapacity)
    {
        ReserveDangerous(initialCapacity);
        index = 0;
    }

    public void Advance(int count)
    {
        if (index > buffer.Length - count)
            ExceptionHelper.ThrowInvalidAdvance();

        index += count;
    }

    public Memory<T> GetMemory(int sizeHint = 0)
    {
        Reserve(sizeHint);
        return buffer.AsMemory(index);
    }

    public Span<T> GetSpan(int sizeHint = 0)
    {
        Reserve(sizeHint);
        return buffer.AsSpan(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(T value)
    {
        if ((uint)index >= (uint)buffer.Length)
        {
            Reserve(1);
        }

        buffer.At(index++) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ReadOnlySpan<T> value)
    {
        if ((uint)value.Length > (uint)(buffer.Length - index))
        {
            this.Reserve(value.Length);
        }

        value.CopyTo(buffer.AsSpan(index, value.Length));
        index += value.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reserve(int sizeHint)
    {
        if (sizeHint > buffer.Length - index)
        {
            ReserveCore(sizeHint);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReserveDangerous(int sizeHint)
    {
        buffer = ArrayPool<T>.Shared.Rent(sizeHint);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Return()
    {
        index = 0;
        ArrayPool<T>.Shared.Return(buffer);
        buffer = [];
    }

    public void Dispose()
        => Return();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ReserveCore(int sizeHint)
    {
        var newSize = Math.Max(buffer.Length, 1) * 2;
        if (sizeHint > newSize - index)
        {
            newSize = ArrayPoolBufferWriter<T>.CalculateBufferSize(sizeHint + index);
        }

        var newBuffer = ArrayPool<T>.Shared.Rent(Math.Min(newSize, Array.MaxLength));
        if (buffer.Length > 0)
        {
            var byteCount = Unsafe.SizeOf<T>() * index;
            ref var source = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetArrayDataReference(buffer)!);
            ref var dest = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetArrayDataReference(newBuffer)!);
            Unsafe.CopyBlockUnaligned(ref dest, ref source, (uint)byteCount);

            ArrayPool<T>.Shared.Return(buffer);
        }

        buffer = newBuffer;
    }

    private static int CalculateBufferSize(int capacity)
    {
        capacity -= 1;
        capacity |= capacity >> 1;
        capacity |= capacity >> 2;
        capacity |= capacity >> 4;
        capacity |= capacity >> 8;
        capacity |= capacity >> 16;

        return capacity + 1;
    }
}
