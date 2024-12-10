using CsToml;
using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class TimeOnlyFormatter : ITomlValueFormatter<TimeOnly>
{
    public static readonly TimeOnlyFormatter Instance = new TimeOnlyFormatter();

    public TimeOnly Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetTimeOnly(out var value))
        {
            return value;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(TimeOnly));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TimeOnly target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteTimeOnly(target);
    }
}

internal sealed class NullableTimeOnlyFormatter : ITomlValueFormatter<TimeOnly?>
{
    public static readonly NullableTimeOnlyFormatter Instance = new NullableTimeOnlyFormatter();

    public TimeOnly? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.TryGetTimeOnly(out var value))
        {
            return value;
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TimeOnly? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteTimeOnly(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(TimeOnly?));
        }
    }
}

