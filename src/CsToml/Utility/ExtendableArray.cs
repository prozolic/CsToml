
using System.Buffers;
using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal ref struct ExtendableArray<T>
{
    private T[] array;
    private int count;

    public readonly int Count => count;

    public ExtendableArray()
    {
        array = ArrayPool<T>.Shared.Rent(16);
        count = 0;
    }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Return()
    {
        count = 0;
        ArrayPool<T>.Shared.Return(array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        array = [];
    }

}
