using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace CsToml.Extension;

internal static class CollectionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T At<T>(this Span<T> span, int index)
    {
        // Avoid range check.
        return ref Unsafe.Add<T>(ref MemoryMarshal.GetReference(span), index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T At<T>(this ReadOnlySpan<T> span, int index)
    {
        // Avoid range check.
        return ref Unsafe.Add<T>(ref MemoryMarshal.GetReference(span), index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T At<T>(this T[] array, int index)
    {
        // Avoid range check.
        return ref Unsafe.Add<T>(ref MemoryMarshal.GetArrayDataReference(array), index);
    }
}

