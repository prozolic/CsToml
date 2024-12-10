using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class SByteFormatter : ITomlValueFormatter<sbyte>
{
    public static readonly SByteFormatter Instance = new SByteFormatter();

    public sbyte Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return checked((sbyte)value);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(sbyte));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, sbyte target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteInt64(target);
    }
}

internal sealed class NullableSByteFormatter : ITomlValueFormatter<sbyte?>
{
    public static readonly NullableSByteFormatter Instance = new NullableSByteFormatter();

    public sbyte? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetInt64(out var value))
        {
            return checked((sbyte)value);
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, sbyte? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteInt64(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(sbyte?));
        }
    }
}