
namespace CsToml.Extension;

internal static class EnumerableExtensions
{
    public static bool TryGetNonEnumeratedCount2<TSource>(this IEnumerable<TSource> source, out int count)
    {
        // Enumerable.TryGetNonEnumeratedCount does not check IReadOnlyCollection, so check it manually.
        // https://github.com/dotnet/runtime/issues/42254
        if (source.TryGetNonEnumeratedCount(out count))
        {
            return true;
        }

        if (source is IReadOnlyCollection<TSource> collection)
        {
            count = collection.Count;
            return true;
        }

        count = 0;
        return false;
    }
}