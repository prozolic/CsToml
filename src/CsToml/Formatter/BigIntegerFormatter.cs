
using CsToml.Error;
using CsToml.Values;
using System.Buffers;
using System.Globalization;
using System.Numerics;

namespace CsToml.Formatter;

internal sealed class BigIntegerFormatter : ITomlValueFormatter<BigInteger>
{
    public static readonly BigIntegerFormatter Instance = new BigIntegerFormatter();

    public BigInteger Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return new BigInteger(value);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(BigInteger));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, BigInteger target, CsTomlSerializerOptions options) where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteInt64(checked((long)target));
    }
}

internal sealed class NullableBigIntegerFormatter : ITomlValueFormatter<BigInteger?>
{
    public static readonly NullableBigIntegerFormatter Instance = new NullableBigIntegerFormatter();

    public BigInteger? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetInt64(out var value))
        {
            return new BigInteger(value);
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, BigInteger? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteInt64(checked((long)target));
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(BigInteger?));
        }
    }
}