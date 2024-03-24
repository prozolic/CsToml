using CsToml.Utility;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace CsToml.Formatter;

internal sealed class ValueFormatter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, bool value)
        where TBufferWriter : IBufferWriter<byte>
    {
        SerializeCore<BoolFormatter, TBufferWriter, bool>(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, long value)
        where TBufferWriter : IBufferWriter<byte>
    {
        SerializeCore<Int64Formatter, TBufferWriter, long>(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, double value)
        where TBufferWriter : IBufferWriter<byte>
    {
        SerializeCore<DoubleFormatter, TBufferWriter, double>(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, DateTime value)
        where TBufferWriter : IBufferWriter<byte>
    {
        SerializeCore<DateTimeFormatter, TBufferWriter, DateTime>(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, DateTimeOffset value)
        where TBufferWriter : IBufferWriter<byte>
    {
        SerializeCore<DateTimeOffsetFormatter, TBufferWriter, DateTimeOffset>(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, DateOnly value)
        where TBufferWriter : IBufferWriter<byte>
    {
        SerializeCore<DateOnlyFormatter, TBufferWriter, DateOnly>(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, TimeOnly value)
        where TBufferWriter : IBufferWriter<byte>
    {
        SerializeCore<TimeOnlyFormatter, TBufferWriter, TimeOnly>(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, string value)
        where TBufferWriter : IBufferWriter<byte>
    {
        SerializeCore<StringFormatter, TBufferWriter, string>(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, ReadOnlySpan<char> value)
        where TBufferWriter : IBufferWriter<byte>
    {
        SerializeCore<StringFormatter, TBufferWriter, char>(ref writer, value);
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

    private static void SerializeCore<TFormatter, TBufferWriter, TValue>(ref Utf8Writer<TBufferWriter> writer, TValue value)
        where TFormatter : ICsTomlFormatter<TValue>
        where TBufferWriter : IBufferWriter<byte>
    {
        TFormatter.Serialize(ref writer, value);
    }

    private static void SerializeCore<TSpanFormatter, TBufferWriter, TValue>(ref Utf8Writer<TBufferWriter> writer, ReadOnlySpan<TValue> value)
        where TSpanFormatter : ICsTomlSpanFormatter<TValue>
        where TBufferWriter : IBufferWriter<byte>
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

