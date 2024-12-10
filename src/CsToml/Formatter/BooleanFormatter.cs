using CsToml.Error;
using System.Buffers;
using System.Numerics;

namespace CsToml.Formatter;

internal sealed class BooleanFormatter : ITomlValueFormatter<bool>
{
    public static readonly BooleanFormatter Instance = new BooleanFormatter();

    public bool Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetBool(out var value))
        {
            return value;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(bool));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, bool target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteBoolean(target);
    }
}

internal sealed class NullableBooleanFormatter : ITomlValueFormatter<bool?>
{
    public static readonly NullableBooleanFormatter Instance = new NullableBooleanFormatter();

    public bool? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetBool(out var value))
        {
            return value;
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, bool? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteBoolean(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(bool?));
        }
    }
}


