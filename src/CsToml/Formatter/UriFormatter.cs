using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class UriFormatter : ITomlValueFormatter<Uri>
{
    public static readonly UriFormatter Instance = new UriFormatter();

    public Uri Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetString(out var value))
        {
            return new Uri(value, UriKind.RelativeOrAbsolute);
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(Uri));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Uri target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteString(target.OriginalString);
    }
}
