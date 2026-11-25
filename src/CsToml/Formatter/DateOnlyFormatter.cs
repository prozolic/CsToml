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

    public void Serialize(ref Utf8TomlDocumentWriter writer, DateOnly target, CsTomlSerializerOptions options)
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

    public void Serialize(ref Utf8TomlDocumentWriter writer, DateOnly? target, CsTomlSerializerOptions options)
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