using CsToml.Error;

namespace CsToml.Formatter;

internal sealed class HalfFormatter : ITomlValueFormatter<Half>
{
    public static readonly HalfFormatter Instance = new HalfFormatter();

    public Half Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetDouble(out var value))
        {
            var result = (Half)value;
            if (Half.IsInfinity(result) && double.IsFinite(value))
            {
                ExceptionHelper.ThrowOverflow(typeof(Half));
            }
            return result;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(Half), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, Half target, CsTomlSerializerOptions options)
    {
        writer.WriteDouble((double)target);
    }
}

internal sealed class NullableHalfFormatter : ITomlValueFormatter<Half?>
{
    public static readonly NullableHalfFormatter Instance = new NullableHalfFormatter();

    public Half? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetDouble(out var value))
        {
            var result = (Half)value;
            if (Half.IsInfinity(result) && double.IsFinite(value))
            {
                ExceptionHelper.ThrowOverflow(typeof(Half));
            }
            return result;
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(Half), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, Half? target, CsTomlSerializerOptions options)
    {
        if (target.HasValue)
        {
            writer.WriteDouble((double)target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(Half?));
        }
    }
}
