using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class UInt32Formatter : ITomlValueFormatter<uint>
{
    public static readonly UInt32Formatter Instance = new UInt32Formatter();

    public uint Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return checked((uint)value);
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(uint));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, uint target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteInt64(target);
    }
}

internal sealed class NullableUInt32Formatter : ITomlValueFormatter<uint?>
{
    public static readonly NullableUInt32Formatter Instance = new NullableUInt32Formatter();

    public uint? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetInt64(out var value))
        {
            return checked((uint)value);
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, uint? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteInt64(target.Value);
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(uint?));
        }
    }
}
