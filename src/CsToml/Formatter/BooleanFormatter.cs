using CsToml.Error;
using System.Numerics;

namespace CsToml.Formatter;

internal sealed class BooleanFormatter : ITomlValueFormatter<bool>
{
    public static readonly BooleanFormatter Instance = new BooleanFormatter();

    public bool Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetBool(out var value))
        {
            return value;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(bool), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, bool target, CsTomlSerializerOptions options)
    {
        writer.WriteBoolean(target);
    }
}

internal sealed class NullableBooleanFormatter : ITomlValueFormatter<bool?>
{
    public static readonly NullableBooleanFormatter Instance = new NullableBooleanFormatter();

    public bool? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetBool(out var value))
        {
            return value;
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(bool), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, bool? target, CsTomlSerializerOptions options)
    {
        if (target.HasValue)
        {
            writer.WriteBoolean(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(bool?));
        }
    }
}


