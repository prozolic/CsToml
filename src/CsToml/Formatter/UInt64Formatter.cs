using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class UInt64Formatter : ITomlValueFormatter<ulong>
{
    public static readonly UInt64Formatter Instance = new UInt64Formatter();

    public ulong Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return checked((ulong)value);
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(ulong));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ulong target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteInt64(checked((long)target));
    }
}

internal sealed class NullableUInt64Formatter : ITomlValueFormatter<ulong?>
{
    public static readonly NullableUInt64Formatter Instance = new NullableUInt64Formatter();

    public ulong? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetInt64(out var value))
        {
            return checked((ulong)value);
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ulong? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteInt64(checked((long)target.GetValueOrDefault()));
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(ulong?));
        }
    }
}
