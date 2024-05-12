using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("{Value}")]
internal partial class CsTomlLocalDate(DateOnly value) : CsTomlValue()
{
    public DateOnly Value { get; private set; } = value;

    public override bool HasValue => true;

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

}
