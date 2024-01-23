using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsToml.Values;

internal partial class CsTomlLocalDateTime : ISpanFormattable
{
    public override string GetString()
    {
        Span<char> destination = stackalloc char[32];
        TryFormat(destination, out var charsWritten, null, null);
        return destination[..charsWritten].ToString();
    }

    public override DateTime GetDateTime() => Value;

    public override DateTimeOffset GetDateTimeOffset() => Value;

    public override DateOnly GetDateOnly() => DateOnly.FromDateTime(Value);

    public override TimeOnly GetTimeOnly() => TimeOnly.FromDateTime(Value);

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    => Value.TryFormat(destination, out charsWritten, format, provider);

    public string ToString(string? format, IFormatProvider? formatProvider)
        => Value.ToString(format, formatProvider);
}

