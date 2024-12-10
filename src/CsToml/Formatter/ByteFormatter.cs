using CsToml.Error;
using System.Buffers;
using System.Numerics;

namespace CsToml.Formatter;

internal sealed class ByteFormatter : ITomlValueFormatter<byte>
{
    public static readonly ByteFormatter Instance = new ByteFormatter();

    public byte Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return checked((byte)value);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(byte));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, byte target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteInt64(target);
    }
}

internal sealed class NullableByteFormatter : ITomlValueFormatter<byte?>
{
    public static readonly NullableByteFormatter Instance = new NullableByteFormatter();

    public byte? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetInt64(out var value))
        {
            return checked((byte)value);
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, byte? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteInt64(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(byte?));
        }
    }
}
