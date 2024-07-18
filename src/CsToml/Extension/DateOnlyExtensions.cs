
using System.Runtime.CompilerServices;

namespace CsToml.Extension;

internal static class DateOnlyExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime ToLocalDateTime(this DateOnly target)
        => new(target, TimeOnly.MinValue, DateTimeKind.Local);

}

