
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace CsToml.Formatter;

internal sealed class ImmutableArrayFormatter<T> : ArrayBaseFormatter<ImmutableArray<T>, T>
{
    protected override ReadOnlySpan<T> AsSpan(ImmutableArray<T> array)
    {
        return array.AsSpan();
    }

    protected override ImmutableArray<T> Complete(T[] array)
    {
        return ImmutableCollectionsMarshal.AsImmutableArray(array);
    }
}
