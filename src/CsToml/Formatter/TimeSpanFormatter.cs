using CsToml.Error;
using System.Buffers;

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
        ExceptionHelper.ThrowDeserializationFailed(typeof(TimeSpan));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TimeSpan target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteInt64(target.Ticks);
    }
}

internal sealed class NullableTimeSpanFormatter : ITomlValueFormatter<TimeSpan?>
{
    public static readonly NullableTimeSpanFormatter Instance = new NullableTimeSpanFormatter();

    public TimeSpan? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetInt64(out var value))
        {
            return new TimeSpan(value);
        }

        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TimeSpan? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteInt64(target.Value.Ticks);
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(TimeSpan?));
        }
    }
}