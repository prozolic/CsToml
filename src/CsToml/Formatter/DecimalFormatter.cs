using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class DecimalFormatter : ITomlValueFormatter<decimal>
{
    public static readonly DecimalFormatter Instance = new DecimalFormatter();

    public decimal Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetDouble(out var doubleValue))
        {
            return (decimal)doubleValue;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(decimal));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, decimal target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteDecimal(target);
    }
}

internal sealed class NullableDecimalFormatter : ITomlValueFormatter<decimal?>
{
    public static readonly NullableDecimalFormatter Instance = new NullableDecimalFormatter();

    public decimal? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue) return default;

        if (rootNode.TryGetDouble(out var doubleValue))
        {
            return (decimal)doubleValue;
        }

        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, decimal? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteDecimal(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(decimal?));
        }
    }
}


