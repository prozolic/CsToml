using CsToml.Error;
using CsToml.Values;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class GuidFormatter : ITomlValueFormatter<Guid>
{
    public static readonly GuidFormatter Instance = new GuidFormatter();

    public Guid Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.Value is TomlString tomlString)
        {
            if (Guid.TryParse(tomlString.GetString().AsSpan(), out var id))
            {
                return id;
            }
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(Guid));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Guid target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteString(target.ToString());
    }
}

internal sealed class NullableGuidFormatter : ITomlValueFormatter<Guid?>
{
    public static readonly NullableGuidFormatter Instance = new NullableGuidFormatter();

    public Guid? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.Value is TomlString tomlString)
        {
            if (Guid.TryParse(tomlString.GetString().AsSpan(), out var id))
            {
                return id;
            }
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Guid? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteString(target.Value.ToString());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(Guid?));
        }
    }
}