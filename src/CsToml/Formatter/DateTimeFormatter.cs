using CsToml.Error;

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

        if (TryConvert(ref rootNode, out var value))
        {
            return value;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(DateTime), rootNode);
        return default;
    }

    internal static bool TryConvert(ref TomlDocumentNode rootNode, out DateTime value)
    {
        if (rootNode.TryGetDateTime(out value))
        {
            return true;
        }
        if (rootNode.TryGetDateTimeOffset(out var dateTimeOffset))
        {
            value = dateTimeOffset.UtcDateTime;
            return true;
        }
        if (rootNode.TryGetDateOnly(out var date))
        {
            value = date.ToDateTime(TimeOnly.MinValue);
            return true;
        }
        return false;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, DateTime target, CsTomlSerializerOptions options)
        where TBufferWriter : IUtf8Writer, allows ref struct
    {
        writer.WriteDateTime(target);
    }
}

internal sealed class NullableDateTimeFormatter : ITomlValueFormatter<DateTime?>
{
    public static readonly NullableDateTimeFormatter Instance = new NullableDateTimeFormatter();

    public DateTime? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (DateTimeFormatter.TryConvert(ref rootNode, out var value))
        {
            return value;
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(DateTime), rootNode);
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, DateTime? target, CsTomlSerializerOptions options)
        where TBufferWriter : IUtf8Writer, allows ref struct
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
