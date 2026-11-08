
namespace CsToml.Formatter;

public sealed class ArrayFormatter<T> : ArrayBaseFormatter<T[], T>
{
    protected override ReadOnlySpan<T> AsSpan(T[] array)
    {
        return array;
    }

    protected override T[] Complete(T[] array)
    {
        return array;
    }
}

