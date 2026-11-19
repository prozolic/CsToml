using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal static class UnsafeHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TTo BitCast<TFrom, TTo>(TFrom source)
    {
        return Unsafe.BitCast<TFrom, TTo>(source);
    }
}
