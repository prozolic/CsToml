
using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal ref struct SpanWriter(Span<byte> buffer)
{
    private readonly Span<byte> source = buffer;
    private int written = 0;

    public Span<byte> WrittenSpan => source[..written];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte ch)
    {
        source[written++] = ch;
    }

}