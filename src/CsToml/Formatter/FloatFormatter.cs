using CsToml.Error;

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
            var result = (float)value;
            if (float.IsInfinity(result) && double.IsFinite(value))
            {
                ExceptionHelper.ThrowOverflow(typeof(float));
            }
            return result;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(float), rootNode);
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, float target, CsTomlSerializerOptions options)
        where TBufferWriter : IUtf8Writer, allows ref struct
    {
        writer.WriteDouble(target);
    }
}

internal sealed class NullableFloatFormatter : ITomlValueFormatter<float?>
{
    public static readonly NullableFloatFormatter Instance = new NullableFloatFormatter();

    public float? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetDouble(out var value))
        {
            var result = (float)value;
            if (float.IsInfinity(result) && double.IsFinite(value))
            {
                ExceptionHelper.ThrowOverflow(typeof(float));
            }
            return result;
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(float), rootNode);
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, float? target, CsTomlSerializerOptions options)
        where TBufferWriter : IUtf8Writer, allows ref struct
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
