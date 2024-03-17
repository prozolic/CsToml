using CsToml.Utility;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml.Formatter;

internal sealed class ValueFormatter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(ref Utf8Writer writer, bool value)
    {
        SerializeCore<BoolFormatter, bool>(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(ref Utf8Writer writer, long value)
    {
        SerializeCore<Int64Formatter, long>(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(ref Utf8Writer writer, double value)
    {
        SerializeCore<DoubleFormatter, double>(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(ref Utf8Writer writer, DateTime value)
    {
        SerializeCore<DateTimeFormatter, DateTime>(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(ref Utf8Writer writer, DateTimeOffset value)
    {
        SerializeCore<DateTimeOffsetFormatter, DateTimeOffset>(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(ref Utf8Writer writer, DateOnly value)
    {
        SerializeCore<DateOnlyFormatter, DateOnly>(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(ref Utf8Writer writer, TimeOnly value)
    {
        SerializeCore<TimeOnlyFormatter, TimeOnly>(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(ref Utf8Writer writer, string value)
    {
        SerializeCore<StringFormatter, string>(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(ref Utf8Writer writer, ReadOnlySpan<char> value)
    {
        SerializeCore<StringFormatter, char>(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Deserialize(ref Utf8Reader reader, int length, out bool value)
    {
        DeserializeCore<BoolFormatter, bool>(ref reader, length, out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Deserialize(ref Utf8Reader reader, int length, out long value)
    {
        DeserializeCore<Int64Formatter, long>(ref reader, length, out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Deserialize(ref Utf8Reader reader, int length, out double value)
    {
        DeserializeCore<DoubleFormatter, double>(ref reader, length, out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Deserialize(ref Utf8Reader reader, int length, out DateTime value)
    {
        DeserializeCore<DateTimeFormatter, DateTime>(ref reader, length, out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Deserialize(ref Utf8Reader reader, int length, out DateTimeOffset value)
    {
        DeserializeCore<DateTimeOffsetFormatter, DateTimeOffset>(ref reader, length, out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Deserialize(ref Utf8Reader reader, int length, out DateOnly value)
    {
        DeserializeCore<DateOnlyFormatter, DateOnly>(ref reader, length, out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Deserialize(ref Utf8Reader reader, int length, out TimeOnly value)
    {
        DeserializeCore<TimeOnlyFormatter, TimeOnly>(ref reader, length, out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Deserialize(ref Utf8Reader reader, int length, out string value)
    {
        DeserializeCore<StringFormatter, string>(ref reader, length, out value);
    }

    private static void SerializeCore<TFormatter, TValue>(ref Utf8Writer writer, TValue value)
        where TFormatter : ICsTomlFormatter<TValue>
    {
        TFormatter.Serialize(ref writer, value);
    }

    private static void SerializeCore<TSpanFormatter, TValue>(ref Utf8Writer writer, ReadOnlySpan<TValue> value)
        where TSpanFormatter : ICsTomlSpanFormatter<TValue>
        where TValue : struct
    {
        TSpanFormatter.Serialize(ref writer, value);
    }

    private static void DeserializeCore<TFormatter, TValue>(ref Utf8Reader reader, int length, out TValue value)
        where TFormatter : ICsTomlFormatter<TValue>
    {
        value = TFormatter.Deserialize(ref reader, length);
    }
}

