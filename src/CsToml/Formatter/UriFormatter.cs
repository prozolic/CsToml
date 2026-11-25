using CsToml.Error;

namespace CsToml.Formatter;

internal sealed class UriFormatter : ITomlValueFormatter<Uri?>
{
    public static readonly UriFormatter Instance = new UriFormatter();

    public Uri? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetString(out var value))
        {
            return new Uri(value, UriKind.RelativeOrAbsolute);
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(Uri), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, Uri? target, CsTomlSerializerOptions options)
    {
        if (target != null)
        {
            writer.WriteString(target?.OriginalString);
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(Uri));
        }
    }
}
