﻿using CsToml.Error;
using System.Buffers;
using System.Collections;

namespace CsToml.Formatter;


internal sealed class BitArrayFormatter : ITomlValueFormatter<BitArray>
{
    public static readonly BitArrayFormatter Instance = new BitArrayFormatter();

    public BitArray Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetArray(out var value))
        {
            var bitArray = new BitArray(value.Count);
            var formatter = options.Resolver.GetFormatter<bool>()!;
            for (int i = 0; i < bitArray.Length; i++)
            {
                var arrayValueNode = rootNode[i];
                bitArray[i] = formatter.Deserialize(ref arrayValueNode, options);
            }
            return bitArray;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(BitArray));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, BitArray target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target == null)
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(BitArray));
            return;
        }

        writer.BeginArray();
        if (target.Length == 0)
        {
            writer.EndArray();
            return;
        }

        var formatter = options.Resolver.GetFormatter<bool>()!;
        formatter.Serialize(ref writer, target[0], options);
        if (target.Length == 1)
        {
            writer.WriteSpace();
            writer.EndArray();
            return;
        }

        for (int i = 1; i < target.Length; i++)
        {
            writer.Write(TomlCodes.Symbol.COMMA);
            writer.WriteSpace();
            formatter.Serialize(ref writer, target[i], options);
        }
        writer.WriteSpace();
        writer.EndArray();
    }
}
