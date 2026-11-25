using CsToml.Error;

namespace CsToml.Formatter;

internal sealed class NullableStringFormatter : ITomlValueFormatter<string?>
{
    public static readonly NullableStringFormatter Instance = new NullableStringFormatter();

    public string? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetString(out var value))
        {
            return value;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(string), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, string? target, CsTomlSerializerOptions options)
    {
        if (target != null)
        {
            writer.WriteString(target);
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(string));
        }
    }
}
