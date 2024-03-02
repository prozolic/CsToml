using CsToml.Error;
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

public abstract partial class CsTomlValue :
    ISpanFormattable,
    IUtf8SpanFormattable
{
    public static readonly CsTomlValue Empty = new CsTomlEmpty();

    public CsTomlType Type { get;}

    public bool HasValue
        => this.Type != CsTomlType.None;

    protected CsTomlValue(CsTomlType type)
    {
        this.Type = type;
    }

    internal virtual bool ToTomlString(ref Utf8Writer writer) // Write TOML format.
        => false;

    public virtual bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        charsWritten = 0;
        return false;
    }

    public virtual string ToString(string? format, IFormatProvider? formatProvider)
        => ExceptionHelper.NotReturnThrow<string, string>(ExceptionHelper.ThrowNotSupported, nameof(ToString));

    public virtual bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        bytesWritten = 0;
        return false;
    }

    [DebuggerDisplay("None")]
    private sealed class CsTomlEmpty : CsTomlValue
    {
        public CsTomlEmpty() : base(CsTomlType.None) { }
    }
}
