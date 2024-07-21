#pragma warning disable CS8618

using System.Runtime.CompilerServices;

namespace CsToml.Generator.Internal;

internal static class CollectionsMarshal
{
    private class ListStruct<T>
    {
        internal T[] items;
        internal int size;
        internal int version;
    }

    public static Span<T> AsSpan<T>(List<T> list)
        => Unsafe.As<ListStruct<T>>(list).items.AsSpan(0, list.Count);
}
