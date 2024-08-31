using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class DateOnlyFormatter : ITomlValueFormatter<DateOnly>
{
    public static readonly DateOnlyFormatter Instance = new DateOnlyFormatter();

    public DateOnly Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetDateOnly(out var value))
        {
            return value;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(DateOnly));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, DateOnly target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteDateOnly(target);
    }
}

internal sealed class NullableDateOnlyFormatter : ITomlValueFormatter<DateOnly?>
{
    public static readonly NullableDateOnlyFormatter Instance = new NullableDateOnlyFormatter();

    public DateOnly? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetDateOnly(out var value))
        {
            return value;
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, DateOnly? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteDateOnly(target.Value);
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(DateOnly?));
        }
    }
}