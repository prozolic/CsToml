
using CsToml.Utility;
using CsToml.Error;
using CsToml.Values;
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

        ExceptionHelper.ThrowDeserializationFailed(typeof(BigInteger), rootNode);
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, BigInteger target, CsTomlSerializerOptions options) where TBufferWriter : IUtf8Writer, allows ref struct
    {
        writer.WriteInt64(IntegerRangeHelper.ConvertToInt64(target));
    }
}

internal sealed class NullableBigIntegerFormatter : ITomlValueFormatter<BigInteger?>
{
    public static readonly NullableBigIntegerFormatter Instance = new NullableBigIntegerFormatter();

    public BigInteger? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return new BigInteger(value);
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(BigInteger), rootNode);
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, BigInteger? target, CsTomlSerializerOptions options)
        where TBufferWriter : IUtf8Writer, allows ref struct
    {
        if (target.HasValue)
        {
            writer.WriteInt64(IntegerRangeHelper.ConvertToInt64(target.GetValueOrDefault()));
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(BigInteger?));
        }
    }
}