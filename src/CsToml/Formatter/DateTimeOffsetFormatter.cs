using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class DateTimeOffsetFormatter : ITomlValueFormatter<DateTimeOffset>
{
    public static readonly DateTimeOffsetFormatter Instance = new DateTimeOffsetFormatter();

    public DateTimeOffset Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetDateTimeOffset(out var value))
        {
            return value;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(DateTimeOffset));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, DateTimeOffset target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteDateTimeOffset(target);
    }
}

internal sealed class NullableDateTimeOffsetFormatter : ITomlValueFormatter<DateTimeOffset?>
{
    public static readonly NullableDateTimeOffsetFormatter Instance = new NullableDateTimeOffsetFormatter();

    public DateTimeOffset? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetDateTimeOffset(out var value))
        {
            return value;
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, DateTimeOffset? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteDateTimeOffset(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(DateTimeOffset?));
        }
    }
}
