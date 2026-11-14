using CsToml.Error;
using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal ref struct SpanWriter(Span<byte> source)
{
    private readonly Span<byte> source = source;
    private int written = 0;

    public SpanWriter(Span<byte> source, int written)
        : this(source)
    {
        this.written = written;
    }

    public readonly int Written => written;

    public readonly Span<byte> WrittenSpan => source[..written];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte ch)
    {
        if ((uint)written >= (uint)source.Length)
        {
            ExceptionHelper.ThrowOverflowDuringParsingOfNumericTypes();
            return;
        }

        source[written++] = ch;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryWrite(byte ch)
    {
        var index = written;
        if ((uint)index < (uint)source.Length)
        {
            source[index] = ch;
            written = index + 1;
            return true;
        }

        return false;
    }

}