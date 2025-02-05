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

    public abstract TomlValueType Type { get; }

    internal virtual void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer) // Write TOML format.
        where TBufferWriter : IBufferWriter<byte>
    {
        ExceptionHelper.ThrowNotSupported(nameof(ToTomlString));
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

    [DebuggerDisplay("{DisplayValue}")]
    private sealed class CsTomlEmpty : TomlValue
    {
        internal static readonly string DisplayValue = "Empty value";

        public override TomlValueType Type => TomlValueType.Empty;

        public CsTomlEmpty() : base() { }

        public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            var displayValueSpan = DisplayValue.AsSpan();
            if (destination.Length < displayValueSpan.Length)
            {
                charsWritten = 0;
                return false;
            }
            displayValueSpan.CopyTo(destination);
            charsWritten = displayValueSpan.Length;
            return true;
        }

        public override string ToString(string? format, IFormatProvider? formatProvider)
            => DisplayValue;

        public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            var displayValueSpan = "Empty"u8;
            if (utf8Destination.Length < displayValueSpan.Length)
            {
                bytesWritten = 0;
                return false;
            }
            displayValueSpan.CopyTo(utf8Destination);
            bytesWritten = displayValueSpan.Length;
            return true;
        }

        public override string ToString()
            => DisplayValue;
    }
}
