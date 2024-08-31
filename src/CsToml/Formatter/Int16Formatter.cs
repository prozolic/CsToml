using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class Int16Formatter : ITomlValueFormatter<short>
{
    public static readonly Int16Formatter Instance = new Int16Formatter();

    public short Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return checked((short)value);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(short));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, short target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteInt64(target);
    }
}

internal sealed class NullableInt16Formatter : ITomlValueFormatter<short?>
{
    public static readonly NullableInt16Formatter Instance = new NullableInt16Formatter();

    public short? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetInt64(out var value))
        {
            return checked((short)value);
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, short? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteInt64(target.Value);
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(short?));
        }
    }
}
