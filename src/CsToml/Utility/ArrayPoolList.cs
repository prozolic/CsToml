
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Utility;

internal sealed class ArrayPoolList<T> : IDisposable
{
    private T[] array;
    private int index;
    private bool isRent = true; 

    public ReadOnlySpan<T> WrittenSpan => array.AsSpan(0, index);

    public int Count => index;

    public bool IsRent => isRent;

    public ArrayPoolList()
    {
        array = ArrayPool<T>.Shared.Rent(4);
        index = 0;
        isRent = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T value)
    {
        var index = this.index;
        var array = this.array;
        if ((uint)index < (uint)array.Length)
        {
            this.index = index + 1;
            array[index] = value;
        }
        else
        {
            this.AddAndEnsureCapacity(value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
        => index = 0;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddAndEnsureCapacity(T value)
    {
        var index = this.index;
        var oldArray = array;
        var newArray = ArrayPool<T>.Shared.Rent(Math.Min(oldArray.Length * 2, Array.MaxLength));

        var byteCount = Unsafe.SizeOf<T>() * index;
        ref var source = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetArrayDataReference(oldArray)!);
        ref var dest = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetArrayDataReference(newArray)!);
        Unsafe.CopyBlockUnaligned(ref dest, ref source, (uint)byteCount);

        ArrayPool<T>.Shared.Return(oldArray, RuntimeHelpers.IsReferenceOrContainsReferences<T>());

        newArray[index] = value;
        this.index = index + 1;
        this.array = newArray;
    }

    public void Dispose()
    {
        if (isRent)
        {
            isRent = false;
            index = 0;
            ArrayPool<T>.Shared.Return(array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }
    }

}

