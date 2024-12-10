using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class FloatFormatter : ITomlValueFormatter<float>
{
    public static readonly FloatFormatter Instance = new FloatFormatter();

    public float Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetDouble(out var value))
        {
            return (float)value;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(float));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, float target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteDouble(target);
    }
}

internal sealed class NullableFloatFormatter : ITomlValueFormatter<float?>
{
    public static readonly NullableFloatFormatter Instance = new NullableFloatFormatter();

    public float? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetDouble(out var value))
        {
            return (float)value;
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, float? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteDouble(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(float?));
        }
    }
}
