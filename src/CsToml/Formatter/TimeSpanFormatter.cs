using CsToml.Error;

namespace CsToml.Formatter;

internal sealed class TimeSpanFormatter : ITomlValueFormatter<TimeSpan>
{
    public static readonly TimeSpanFormatter Instance = new TimeSpanFormatter();

    public TimeSpan Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return new TimeSpan(value);
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(TimeSpan), rootNode);
        return default!;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, TimeSpan target, CsTomlSerializerOptions options)
    {
        writer.WriteInt64(target.Ticks);
    }
}

internal sealed class NullableTimeSpanFormatter : ITomlValueFormatter<TimeSpan?>
{
    public static readonly NullableTimeSpanFormatter Instance = new NullableTimeSpanFormatter();

    public TimeSpan? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return new TimeSpan(value);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(TimeSpan), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, TimeSpan? target, CsTomlSerializerOptions options)
    {
        if (target.HasValue)
        {
            writer.WriteInt64(target.GetValueOrDefault().Ticks);
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(TimeSpan?));
        }
    }
}