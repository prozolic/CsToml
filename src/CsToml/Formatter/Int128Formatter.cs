using CsToml.Error;
using CsToml.Values;
using System.Buffers;
using System.Globalization;
using System.Numerics;

namespace CsToml.Formatter;

internal sealed class Int128Formatter : ITomlValueFormatter<Int128>
{
    public static readonly Int128Formatter Instance = new Int128Formatter();

    public Int128 Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return (Int128)value;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(Int128));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Int128 target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteInt64(checked((long)target));
    }
}

internal sealed class NullableInt128Formatter : ITomlValueFormatter<Int128?>
{
    public static readonly NullableInt128Formatter Instance = new NullableInt128Formatter();

    public Int128? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetInt64(out var value))
        {
            return (Int128)value;
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Int128? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteInt64(checked((long)target));
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(Int128?));
        }
    }
}