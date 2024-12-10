using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class DateTimeFormatter : ITomlValueFormatter<DateTime>
{
    public static readonly DateTimeFormatter Instance = new DateTimeFormatter();

    public DateTime Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetDateTime(out var value))
        {
            return value;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(DateTime));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, DateTime target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteDateTime(target);
    }
}

internal sealed class NullableDateTimeFormatter : ITomlValueFormatter<DateTime?>
{
    public static readonly NullableDateTimeFormatter Instance = new NullableDateTimeFormatter();

    public DateTime? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetDateTime(out var value))
        {
            return value;
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, DateTime? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteDateTime(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(DateTime?));
        }
    }
}
