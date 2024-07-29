
using System.Buffers;
using System.Runtime.CompilerServices;

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
        array = ArrayPool<T>.Shared.Rent(16);
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
        Array.Copy(oldArray, newArray, index);

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

