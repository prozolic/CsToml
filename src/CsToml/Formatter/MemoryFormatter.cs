
namespace CsToml.Formatter;

public sealed class MemoryFormatter<T> : ArrayBaseFormatter<Memory<T>, T>
{
    protected override ReadOnlySpan<T> AsSpan(Memory<T> array)
    {
        return array.Span;
    }

    protected override Memory<T> Complete(T[] array)
    {
        return new Memory<T>(array);
    }
}
