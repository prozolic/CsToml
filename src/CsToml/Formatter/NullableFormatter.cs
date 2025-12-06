using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

public sealed class NullableFormatter<T> : ITomlValueFormatter<T?>, ITomlArrayHeaderFormatter<T?>
    where T : struct
{
    public T? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return null;
        }
        return options.Resolver.GetFormatter<T>()!.Deserialize(ref rootNode, options);
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, T? target, CsTomlSerializerOptions options) where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            options.Resolver.GetFormatter<T>()!.Serialize(ref writer, target.GetValueOrDefault(), options);
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(T));
        }
    }

    bool ITomlArrayHeaderFormatter<T?>.TrySerialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, T? target, CsTomlSerializerOptions options)
    {
        if (target.HasValue)
        {
            if (options.Resolver.GetFormatter<T>() is ITomlArrayHeaderFormatter<T> tomlArrayHeaderFormatter)
            {
                // NullableFormatter<ImmutableArrayFormatter> is reached here.
                return tomlArrayHeaderFormatter.TrySerialize(ref writer, header, target.GetValueOrDefault(), options);
            }
            return false;
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(T));
            return false;
        }
    }
}
