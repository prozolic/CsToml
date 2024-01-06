
using System.Runtime.CompilerServices;

namespace CsToml.Extension;

internal static class DateOnlyExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime ToLocalDateTime(this DateOnly target)
        => new(target, TimeOnly.MinValue, DateTimeKind.Local);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset ToDateTimeOffset(this DateOnly target)
    {
        var dateTime = target.ToDateTime(TimeOnly.MinValue);
        return new(dateTime, TimeZoneInfo.Local.GetUtcOffset(dateTime));
    }
}

