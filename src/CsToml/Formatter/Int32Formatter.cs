using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class Int32Formatter : ITomlValueFormatter<int>
{
    public static readonly Int32Formatter Instance = new Int32Formatter();

    public int Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return checked((int)value);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(int));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, int target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteInt64(target);
    }
}

internal sealed class NullableInt32Formatter : ITomlValueFormatter<int?>
{
    public static readonly NullableInt32Formatter Instance = new NullableInt32Formatter();

    public int? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetInt64(out var value))
        {
            return checked((int)value);
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, int? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteInt64(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(int?));
        }
    }
}
