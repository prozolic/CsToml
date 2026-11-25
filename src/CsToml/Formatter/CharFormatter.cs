using CsToml.Utility;
using CsToml.Error;

namespace CsToml.Formatter;

internal sealed class CharFormatter : ITomlValueFormatter<char>
{
    public static readonly CharFormatter Instance = new CharFormatter();

    public char Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return IntegerRangeHelper.ConvertFromInt64<char>(value);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(char), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, char target, CsTomlSerializerOptions options)
    {
        writer.WriteInt64(target);
    }
}

internal sealed class NullableCharFormatter : ITomlValueFormatter<char?>
{
    public static readonly NullableCharFormatter Instance = new NullableCharFormatter();

    public char? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return IntegerRangeHelper.ConvertFromInt64<char>(value);
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(char), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, char? target, CsTomlSerializerOptions options)
    {
        if (target.HasValue)
        {
            writer.WriteInt64(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(char?));
        }
    }
}