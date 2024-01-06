
using System.Runtime.CompilerServices;

namespace CsToml.Extension;

internal static class TimeOnlyExtensions
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime ToLocalDateTime(this TimeOnly target)
        => new(DateOnly.MinValue, target, DateTimeKind.Local);
}
