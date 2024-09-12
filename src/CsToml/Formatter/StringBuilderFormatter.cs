using CsToml.Error;
using System.Buffers;
using System.Text;


namespace CsToml.Formatter;

internal sealed class StringBuilderFormatter : ITomlValueFormatter<StringBuilder>
{
    public static readonly StringBuilderFormatter Instance = new StringBuilderFormatter();

    public StringBuilder Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetString(out var value))
        {
            return new StringBuilder(value);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(StringBuilder));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, StringBuilder target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
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
