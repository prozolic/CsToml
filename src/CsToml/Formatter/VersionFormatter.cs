using CsToml.Error;
using CsToml.Values;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class VersionFormatter : ITomlValueFormatter<Version?>
{
    public static readonly VersionFormatter Instance = new VersionFormatter();

    public Version? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.Value is TomlString tomlString)
        {
            if (Version.TryParse(tomlString.GetString().AsSpan(), out var version))
            {
                return version;
            }
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(Version));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Version? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target != null)
        {
            writer.WriteString(target?.ToString());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(Version));
        }
    }
}
