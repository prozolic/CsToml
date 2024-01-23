

using CsToml.Utility;

namespace CsToml.Values;

internal partial class CsTomlDouble : ISpanFormattable
{
    public override string GetString()
    {
        Span<char> destination = stackalloc char[32];
        TryFormat(destination, out var charsWritten, null, null);
        return destination[..charsWritten].ToString();
    }

    public override long GetInt64() => (long)Value;

    public override double GetDouble() => Value;

    public override bool GetBool() => Value != 0d;

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => Value.TryFormat(destination, out charsWritten, format, provider);

    public string ToString(string? format, IFormatProvider? formatProvider)
        => Value.ToString(format, formatProvider);
}

