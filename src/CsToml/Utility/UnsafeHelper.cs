using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal static class UnsafeHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TTo BitCast<TFrom, TTo>(TFrom source)
    {
#if NET9_0_OR_GREATER
        // the struct constraint is removed from .NET 9 , it is used as is.
        return Unsafe.BitCast<TFrom, TTo>(source);
#else
        return Unsafe.ReadUnaligned<TTo>(ref Unsafe.As<TFrom, byte>(ref source));
#endif
    }
}
