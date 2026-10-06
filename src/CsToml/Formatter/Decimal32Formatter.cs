#if NET11_0_OR_GREATER

using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Numerics;

namespace CsToml.Formatter;

internal sealed class Decimal32Formatter : ITomlValueFormatter<Decimal32>
{
    public static readonly Decimal32Formatter Instance = new Decimal32Formatter();

    public Decimal32 Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.HasValueOnly)
        {
            // If toml value is an integer, get it as Int64 to avoid going through double.
            var rawTomlValue = rootNode.Value;
            if (rawTomlValue.Type == TomlValueType.Integer)
            {
                return (Decimal32)rawTomlValue.GetInt64();
            }

            if (rawTomlValue.TryGetDouble(out var doubleValue))
            {
                return DecimalFloatingPointHelper.ConvertFromDouble<Decimal32>(doubleValue);
            }
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(Decimal32));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Decimal32 target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteDecimalFloatingPoint(target);
    }
}

internal sealed class NullableDecimal32Formatter : ITomlValueFormatter<Decimal32?>
{
    public static readonly NullableDecimal32Formatter Instance = new NullableDecimal32Formatter();

    public Decimal32? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue) return default;

        if (rootNode.HasValueOnly)
        {
            // If toml value is an integer, get it as Int64 to avoid going through double.
            var rawTomlValue = rootNode.Value;
            if (rawTomlValue.Type == TomlValueType.Integer)
            {
                return (Decimal32)rawTomlValue.GetInt64();
            }

            if (rawTomlValue.TryGetDouble(out var doubleValue))
            {
                return DecimalFloatingPointHelper.ConvertFromDouble<Decimal32>(doubleValue);
            }
        }

        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Decimal32? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteDecimalFloatingPoint(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(Decimal32?));
        }
    }
}

#endif
