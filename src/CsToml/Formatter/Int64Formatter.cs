using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class Int64Formatter : ITomlValueFormatter<long>
{
    public static readonly Int64Formatter Instance = new Int64Formatter();

    public long Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return value;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(long));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, long target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteInt64(target);
    }
}

internal sealed class NullableInt64Formatter : ITomlValueFormatter<long?>
{
    public static readonly NullableInt64Formatter Instance = new NullableInt64Formatter();

    public long? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetInt64(out var value))
        {
            return value;
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, long? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteInt64(target.Value);
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(long?));
        }
    }
}

