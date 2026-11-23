using CsToml.Utility;
using CsToml.Error;

namespace CsToml.Formatter;

internal sealed class SByteFormatter : ITomlValueFormatter<sbyte>
{
    public static readonly SByteFormatter Instance = new SByteFormatter();

    public sbyte Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return IntegerRangeHelper.ConvertFromInt64<sbyte>(value);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(sbyte), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, sbyte target, CsTomlSerializerOptions options)
    {
        writer.WriteInt64(target);
    }
}

internal sealed class NullableSByteFormatter : ITomlValueFormatter<sbyte?>
{
    public static readonly NullableSByteFormatter Instance = new NullableSByteFormatter();

    public sbyte? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return IntegerRangeHelper.ConvertFromInt64<sbyte>(value);
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(sbyte), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, sbyte? target, CsTomlSerializerOptions options)
    {
        if (target.HasValue)
        {
            writer.WriteInt64(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(sbyte?));
        }
    }
}