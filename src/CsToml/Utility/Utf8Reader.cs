
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal ref struct Utf8Reader(ReadOnlySpan<byte> buffer)
{
    private readonly ReadOnlySpan<byte> source = buffer;

    public readonly int Length => source.Length;

    public int Position { get; set; } = 0;

    public byte this[int index] => source[index];

    [DebuggerStepThrough]
    public ReadOnlySpan<byte> ReadBytes(int length)
    {
        var position = Position;
        Position += length;

        if (!Peek())
        {
            var end = source.Length;
            return source.Slice(position, end - position);
        }
        return source.Slice(position, length);
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Skip(int length)
        => Position += length;

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Peek()
        => source.Length > Position;

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeek(out byte ch)
    {
        var value = Peek();
        ch = value ? source[Position] : default;
        return value;
    }
}

