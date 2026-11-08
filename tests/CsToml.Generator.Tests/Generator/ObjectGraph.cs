using System.Collections;

namespace CsToml.Generator.Tests;

/// <summary>
/// Walks the untyped object graph a TOML table deserializes into: dictionaries for tables, lists or arrays for arrays.
/// Stands in for <c>dynamic</c> indexing, which needs runtime code generation and fails in a NativeAOT build.
/// </summary>
internal static class ObjectGraph
{
    public static T Get<T>(object? source, params object[] path)
    {
        var current = source;
        foreach (var segment in path)
        {
            current = current switch
            {
                IList list when segment is int index => list[index],
                IDictionary dictionary => dictionary.Contains(segment)
                    ? dictionary[segment]
                    : throw new KeyNotFoundException($"Key '{segment}' was not found."),
                _ => throw new InvalidOperationException($"Cannot index {current?.GetType().FullName ?? "null"} with '{segment}'."),
            };
        }
        return (T)current!;
    }
}
