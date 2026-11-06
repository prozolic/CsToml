
namespace CsToml.Values;

internal static class TomlTableHolder
{
    extension(TomlTable? table)
    {
        public T GetKey<T>(ReadOnlySpan<byte> key)
            where T : TomlValue, ITomlStringParser<T>
        {
            if (table == null)
            {
                return T.Parse(key);
            }

            if (table.TryGetKey(key, out var dottedKey) && dottedKey is T typedDottedKey)
            {
                return typedDottedKey!;
            }

            return T.Parse(key);
        }
    }
}
