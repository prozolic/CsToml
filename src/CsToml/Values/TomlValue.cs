using CsToml.Error;
using System.Buffers;
using System.Diagnostics;

namespace CsToml.Values;

public abstract partial class TomlValue :
    ISpanFormattable,
    IUtf8SpanFormattable
{
    public static readonly TomlValue Empty = new CsTomlEmpty();

    public virtual bool HasValue => false;

    protected TomlValue(){}

    internal virtual bool ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer) // Write TOML format.
        where TBufferWriter : IBufferWriter<byte>
    {
        ExceptionHelper.ThrowNotSupported(nameof(ToTomlString));
        return false;
    }

    public virtual bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        charsWritten = 0;
        ExceptionHelper.ThrowNotSupported(nameof(TryFormat));
        return false;
    }

    public virtual string ToString(string? format, IFormatProvider? formatProvider)
    {
        return base.ToString()!;
    }

    public virtual bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        bytesWritten = 0;
        ExceptionHelper.ThrowNotSupported(nameof(TryFormat));
        return false;
    }

    [DebuggerDisplay("Empty")]
    private sealed class CsTomlEmpty : TomlValue
    {
        public CsTomlEmpty() : base() { }
    }
}
