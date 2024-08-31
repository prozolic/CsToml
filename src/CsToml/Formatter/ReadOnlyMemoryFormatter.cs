
namespace CsToml.Formatter;

internal sealed class ReadOnlyMemoryFormatter<T> : ArrayBaseFormatter<ReadOnlyMemory<T>, T>
{
    protected override ReadOnlySpan<T> AsSpan(ReadOnlyMemory<T> array)
    {
        return array.Span;
    }

    protected override ReadOnlyMemory<T> Complete(T[] array)
    {
        return new ReadOnlyMemory<T>(array);
    }
}
