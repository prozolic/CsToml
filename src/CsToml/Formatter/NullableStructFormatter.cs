﻿using CsToml.Error;
using CsToml.Formatter.Resolver;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class NullableStructFormatter<T> : ITomlValueFormatter<T?>
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
}
