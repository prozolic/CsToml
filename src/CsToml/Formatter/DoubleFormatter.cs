using CsToml.Error;

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

        ExceptionHelper.ThrowDeserializationFailed(typeof(double), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, double target, CsTomlSerializerOptions options)
    {
        writer.WriteDouble(target);
    }
}

internal sealed class NullableDoubleFormatter : ITomlValueFormatter<double?>
{
    public static readonly NullableDoubleFormatter Instance = new NullableDoubleFormatter();

    public double? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetDouble(out var value))
        {
            return value;
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(double), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, double? target, CsTomlSerializerOptions options)
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

