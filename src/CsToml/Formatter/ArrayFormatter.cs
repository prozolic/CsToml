
namespace CsToml.Formatter;

internal sealed class ArrayFormatter<T> : ArrayBaseFormatter<T[], T>
{
    protected override ReadOnlySpan<T> AsSpan(T[] array)
    {
        return array.AsSpan();
    }

    protected override T[] Complete(T[] array)
    {
        return array;
    }
}

