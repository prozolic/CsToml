
using System.Buffers;
using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal struct ExtendableArray<T>
{
    private T[] array;
    private int count;
    private bool isRent;

    public readonly int Count => count;

    public readonly bool IsRent => isRent;

    public ExtendableArray(int capacity)
    {
        array = ArrayPool<T>.Shared.Rent(capacity);
        count = 0;
        isRent = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<T> AsSpan()
        => array.AsSpan(0, count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T value)
    {
        var count = this.count;
        var array = this.array;
        if ((uint)count < (uint)array.Length)
        {
            array[count++] = value;
            this.count = count;
        }
        else
        {
            this.AddAndEnsureCapacity(value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Return()
    {
        count = 0;
        ArrayPool<T>.Shared.Return(array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        array = [];
        isRent = false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddAndEnsureCapacity(T value)
    {
        var count = this.count;
        var oldArray = array;

        var newArray = ArrayPool<T>.Shared.Rent(Math.Min(oldArray.Length * 2, Array.MaxLength));
        Array.Copy(oldArray, newArray, count);
        ArrayPool<T>.Shared.Return(oldArray, RuntimeHelpers.IsReferenceOrContainsReferences<T>());

        newArray[count] = value;
        this.count = count + 1;
        this.array = newArray;
    }

}
