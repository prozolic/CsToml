
namespace CsToml.Values;

internal ref struct TomlTableNodeHolder
{
    public TomlTableNode? node;

    public T GetKey<T>(ReadOnlySpan<byte> key)
        where T : TomlValue, ITomlStringParser<T>
    {
        if (node == null)
        {
            return T.Parse(key);
        }

        // Use TomlDottedKey from TomlTableNode if the key is already parsed and stored in the node.
        if (node.TryGetKey(key, out var dottedKey) && dottedKey is T typedDottedKey)
        {
            return typedDottedKey!;
        }

        return T.Parse(key);
    }

}