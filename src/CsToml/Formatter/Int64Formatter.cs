using CsToml.Error;

namespace CsToml.Formatter;

internal sealed class Int64Formatter : ITomlValueFormatter<long>
{
    public static readonly Int64Formatter Instance = new Int64Formatter();

    public long Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return value;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(long), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, long target, CsTomlSerializerOptions options)
    {
        writer.WriteInt64(target);
    }
}

internal sealed class NullableInt64Formatter : ITomlValueFormatter<long?>
{
    public static readonly NullableInt64Formatter Instance = new NullableInt64Formatter();

    public long? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return value;
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(long), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, long? target, CsTomlSerializerOptions options)
    {
        if (target.HasValue)
        {
            writer.WriteInt64(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(long?));
        }
    }
}

