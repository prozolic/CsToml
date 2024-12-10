using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class HalfFormatter : ITomlValueFormatter<Half>
{
    public static readonly HalfFormatter Instance = new HalfFormatter();

    public Half Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetDouble(out var value))
        {
            return (Half)value;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(Half));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Half target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteDouble((double)target);
    }
}

internal sealed class NullableHalfFormatter : ITomlValueFormatter<Half?>
{
    public static readonly NullableHalfFormatter Instance = new NullableHalfFormatter();

    public Half? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetDouble(out var value))
        {
            return (Half)value;
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Half? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteDouble((double)target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(Half?));
        }
    }
}
