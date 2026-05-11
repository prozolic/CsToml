using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace CsToml.Extension;

internal static class CollectionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T At<T>(this Span<T> span, int index)
    {
        Debug.Assert((uint)index < (uint)span.Length, $"Index {index} out of range [0, {span.Length})");
        return ref Unsafe.Add<T>(ref MemoryMarshal.GetReference(span), index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T At<T>(this ReadOnlySpan<T> span, int index)
    {
        Debug.Assert((uint)index < (uint)span.Length, $"Index {index} out of range [0, {span.Length})");
        return ref Unsafe.Add<T>(ref MemoryMarshal.GetReference(span), index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T At<T>(this T[] array, int index)
    {
        Debug.Assert((uint)index < (uint)array.Length, $"Index {index} out of range [0, {array.Length})");
        return ref Unsafe.Add<T>(ref MemoryMarshal.GetArrayDataReference(array), index);
    }
}

