using CsToml.Error;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal static class IntegerRangeHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ConvertFromInt64<T>(long value)
        where T : IBinaryInteger<T>, IMinMaxValue<T>
    {
        if (value < long.CreateSaturating(T.MinValue) || value > long.CreateSaturating(T.MaxValue))
        {
            ExceptionHelper.ThrowOverflow(typeof(T));
        }
        return T.CreateTruncating(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertToInt64<T>(T value)
        where T : IBinaryInteger<T>
    {
        if (value < T.CreateSaturating(long.MinValue) || value > T.CreateSaturating(long.MaxValue))
        {
            ExceptionHelper.ThrowOverflow(typeof(long));
        }
        return long.CreateTruncating(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertToInt64(BigInteger value)
    {
        if (value < long.MinValue || value > long.MaxValue)
        {
            ExceptionHelper.ThrowOverflow(typeof(long));
        }
        return (long)value;
    }
}
