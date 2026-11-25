using CsToml.Error;
using System.Text;


namespace CsToml.Formatter;

internal sealed class StringBuilderFormatter : ITomlValueFormatter<StringBuilder?>
{
    public static readonly StringBuilderFormatter Instance = new StringBuilderFormatter();

    public StringBuilder? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetString(out var value))
        {
            return new StringBuilder(value);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(StringBuilder), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, StringBuilder? target, CsTomlSerializerOptions options)
    {
        if (target != null)
        {
            writer.WriteString(target.ToString());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(StringBuilder));
        }
    }
}
