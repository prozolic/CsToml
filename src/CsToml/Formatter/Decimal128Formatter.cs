#if NET11_0_OR_GREATER

using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Numerics;

namespace CsToml.Formatter;

internal sealed class Decimal128Formatter : ITomlValueFormatter<Decimal128>
{
    public static readonly Decimal128Formatter Instance = new Decimal128Formatter();

    public Decimal128 Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.HasValueOnly)
        {
            // If toml value is an integer, get it as Int64 to avoid precision loss when converting from double.
            var rawTomlValue = rootNode.Value;
            if (rawTomlValue.Type == TomlValueType.Integer)
            {
                return (Decimal128)rawTomlValue.GetInt64();
            }

            if (rawTomlValue.TryGetDouble(out var doubleValue))
            {
                return DecimalFloatingPointHelper.ConvertFromDouble<Decimal128>(doubleValue);
            }
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(Decimal128));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Decimal128 target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteDecimalFloatingPoint(target);
    }
}

internal sealed class NullableDecimal128Formatter : ITomlValueFormatter<Decimal128?>
{
    public static readonly NullableDecimal128Formatter Instance = new NullableDecimal128Formatter();

    public Decimal128? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue) return default;

        if (rootNode.HasValueOnly)
        {
            // If toml value is an integer, get it as Int64 to avoid precision loss when converting from double.
            var rawTomlValue = rootNode.Value;
            if (rawTomlValue.Type == TomlValueType.Integer)
            {
                return (Decimal128)rawTomlValue.GetInt64();
            }

            if (rawTomlValue.TryGetDouble(out var doubleValue))
            {
                return DecimalFloatingPointHelper.ConvertFromDouble<Decimal128>(doubleValue);
            }
        }

        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Decimal128? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteDecimalFloatingPoint(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(Decimal128?));
        }
    }
}

#endif
