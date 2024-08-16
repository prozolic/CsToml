using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace CsToml.Formatter;

internal sealed class FormatterCache
{
    public static ITomlValueFormatter<T>? GetTomlValueFormatter<T>()
        => ValueFormatterCache<T>.formatter;

    public static ITomlValueSpanFormatter<T>? GetTomlValueSpanFormatter<T>()
        where T : struct
    {
        return ValueSpanFormatterCache<T>.formatter;
    }

    private sealed class ValueFormatterCache<T>
    {
        public static ITomlValueFormatter<T>? formatter;
    }

    private sealed class ValueSpanFormatterCache<T>
        where T : struct
    {
        public static ITomlValueSpanFormatter<T>? formatter;
    }

    static FormatterCache()
    {
        ValueFormatterCache<bool>.formatter = BoolFormatter.Default;
        ValueFormatterCache<long>.formatter = Int64Formatter.Default;
        ValueFormatterCache<double>.formatter = DoubleFormatter.Default;
        ValueFormatterCache<string>.formatter = StringFormatter.Default;
        ValueFormatterCache<DateTime>.formatter = DateTimeFormatter.Default;
        ValueFormatterCache<DateTimeOffset>.formatter = DateTimeOffsetFormatter.Default;
        ValueFormatterCache<DateOnly>.formatter = DateOnlyFormatter.Default;
        ValueFormatterCache<TimeOnly>.formatter = TimeOnlyFormatter.Default;

        ValueSpanFormatterCache<char>.formatter = StringFormatter.Default;
    }
}

