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

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, string? target, CsTomlSerializerOptions options)
        where TBufferWriter : IUtf8Writer, allows ref struct
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
