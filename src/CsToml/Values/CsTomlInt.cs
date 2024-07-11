using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml.Values;

[DebuggerDisplay("{Value}")]
internal partial class CsTomlInt : CsTomlValue
{
    internal static readonly CsTomlInt[] cache = CreateCacheValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CsTomlInt Create(long value)
    {
        if ((ulong)(value + 1) < (ulong)cache.Length)
        {
            return cache[value + 1];
        }
        return new CsTomlInt(value);
    }

    public static CsTomlInt Zero => cache[1];

    public long Value { get; init; } 

    public override bool HasValue => true;

    private CsTomlInt(long value) : base()
    {
        this.Value = value;
    }

    internal override bool ToTomlString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer)
    {
        ValueFormatter.Serialize(ref writer, Value);
        return true;
    }

    public override string ToString()
        => GetString();

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        => Value.TryFormat(destination, out charsWritten, format, provider);

    public override string ToString(string? format, IFormatProvider? formatProvider)
        => Value.ToString(format, formatProvider);

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        => Value.TryFormat(utf8Destination, out bytesWritten, format, provider);

    private static CsTomlInt[] CreateCacheValue()
    {
        var intCacheValues = new CsTomlInt[10];
        for (int i = 0; i < intCacheValues.Length; i++)
        {
            intCacheValues[i] = new CsTomlInt(i - 1);
        }

        return intCacheValues;
    }
}

