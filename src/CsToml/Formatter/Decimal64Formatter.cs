#if NET11_0_OR_GREATER

using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Numerics;

namespace CsToml.Formatter;

internal sealed class Decimal64Formatter : ITomlValueFormatter<Decimal64>
{
    public static readonly Decimal64Formatter Instance = new Decimal64Formatter();

    public Decimal64 Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
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
                return (Decimal64)rawTomlValue.GetInt64();
            }

            if (rawTomlValue.TryGetDouble(out var doubleValue))
            {
                return DecimalFloatingPointHelper.ConvertFromDouble<Decimal64>(doubleValue);
            }
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(Decimal64));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Decimal64 target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteDecimalFloatingPoint(target);
    }
}

internal sealed class NullableDecimal64Formatter : ITomlValueFormatter<Decimal64?>
{
    public static readonly NullableDecimal64Formatter Instance = new NullableDecimal64Formatter();

    public Decimal64? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue) return default;

        if (rootNode.HasValueOnly)
        {
            // If toml value is an integer, get it as Int64 to avoid precision loss when converting from double.
            var rawTomlValue = rootNode.Value;
            if (rawTomlValue.Type == TomlValueType.Integer)
            {
                return (Decimal64)rawTomlValue.GetInt64();
            }

            if (rawTomlValue.TryGetDouble(out var doubleValue))
            {
                return DecimalFloatingPointHelper.ConvertFromDouble<Decimal64>(doubleValue);
            }
        }

        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Decimal64? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteDecimalFloatingPoint(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(Decimal64?));
        }
    }
}

#endif
