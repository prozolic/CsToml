using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class UInt16Formatter : ITomlValueFormatter<ushort>
{
    public static readonly UInt16Formatter Instance = new UInt16Formatter();

    public ushort Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return checked((ushort)value);
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(ushort));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ushort target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteInt64(target);
    }
}

internal sealed class NullableUInt16Formatter : ITomlValueFormatter<ushort?>
{
    public static readonly NullableUInt16Formatter Instance = new NullableUInt16Formatter();

    public ushort? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetInt64(out var value))
        {
            return checked((ushort)value);
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ushort? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteInt64(target.Value);
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(ushort?));
        }
    }
}
