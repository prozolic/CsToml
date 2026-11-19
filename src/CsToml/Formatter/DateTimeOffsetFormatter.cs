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

        if (TryConvert(ref rootNode, out var value))
        {
            return value;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(DateTimeOffset), rootNode);
        return default;
    }

    internal static bool TryConvert(ref TomlDocumentNode rootNode, out DateTimeOffset value)
    {
        if (rootNode.TryGetDateTimeOffset(out value))
        {
            return true;
        }
        if (rootNode.TryGetDateTime(out var dateTime))
        {
            value = Create(dateTime);
            return true;
        }
        if (rootNode.TryGetDateOnly(out var date))
        {
            value = Create(date.ToDateTime(TimeOnly.MinValue));
            return true;
        }
        return false;
    }

    private static DateTimeOffset Create(DateTime dateTime)
    {
        try
        {
            return new DateTimeOffset(dateTime);
        }
        catch (ArgumentOutOfRangeException e)
        {
            ExceptionHelper.ThrowArgumentOutOfRangeExceptionWhenCreating<DateTimeOffset>(e);
            return default;
        }
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
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (DateTimeOffsetFormatter.TryConvert(ref rootNode, out var value))
        {
            return value;
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(DateTimeOffset), rootNode);
        return default;
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
