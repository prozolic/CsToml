using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class DoubleFormatter : ITomlValueFormatter<double>
{
    public static readonly DoubleFormatter Instance = new DoubleFormatter();

    public double Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetDouble(out var value))
        {
            return value;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(double));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, double target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteDouble(target);
    }
}

internal sealed class NullableDoubleFormatter : ITomlValueFormatter<double?>
{
    public static readonly NullableDoubleFormatter Instance = new NullableDoubleFormatter();

    public double? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetDouble(out var value))
        {
            return value;
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, double? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteDouble(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(double?));
        }
    }
}

