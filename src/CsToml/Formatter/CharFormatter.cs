using CsToml.Error;
using System.Buffers;

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
            return checked((char)value);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(char));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, char target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteInt64(target);
    }
}

internal sealed class NullableCharFormatter : ITomlValueFormatter<char?>
{
    public static readonly NullableCharFormatter Instance = new NullableCharFormatter();

    public char? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetInt64(out var value))
        {
            return checked((char)value);
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, char? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
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