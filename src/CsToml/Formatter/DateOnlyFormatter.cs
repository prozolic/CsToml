using CsToml.Error;

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

        ExceptionHelper.ThrowDeserializationFailed(typeof(DateOnly), rootNode);
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, DateOnly target, CsTomlSerializerOptions options)
        where TBufferWriter : IUtf8Writer, allows ref struct
    {
        writer.WriteDateOnly(target);
    }
}

internal sealed class NullableDateOnlyFormatter : ITomlValueFormatter<DateOnly?>
{
    public static readonly NullableDateOnlyFormatter Instance = new NullableDateOnlyFormatter();

    public DateOnly? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetDateOnly(out var value))
        {
            return value;
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(DateOnly), rootNode);
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, DateOnly? target, CsTomlSerializerOptions options)
        where TBufferWriter : IUtf8Writer, allows ref struct
    {
        if (target.HasValue)
        {
            writer.WriteDateOnly(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(DateOnly?));
        }
    }
}