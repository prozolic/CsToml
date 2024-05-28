
using System.Runtime.CompilerServices;

namespace CsToml.Extensions.Extension;

internal static partial class ValueTaskExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<TValue> FromResult<TValue>(this TValue value)
        => new(value);
}