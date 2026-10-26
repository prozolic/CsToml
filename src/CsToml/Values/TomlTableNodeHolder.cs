
namespace CsToml.Values;

internal static class TomlTableNodeHolder
{
    extension(TomlTableNode? node)
    {
        public T GetKey<T>(ReadOnlySpan<byte> key)
            where T : TomlValue, ITomlStringParser<T>
        {
            if (node == null)
            {
                return T.Parse(key);
            }

            if (node.TryGetKey(key, out var dottedKey) && dottedKey is T typedDottedKey)
            {
                return typedDottedKey!;
            }

            return T.Parse(key);
        }
    }
}
