using CsToml.Error;
using System;
using System.Buffers;
using System.Collections;

namespace CsToml.Formatter;

internal sealed class ArrayListFormatter : ITomlValueFormatter<ArrayList>
{
    public static readonly ArrayListFormatter Instance = new ArrayListFormatter();

    public ArrayList Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetArray(out var value))
        {
            var formatter = options.Resolver.GetFormatter<object>()!;
            var arrayList = new ArrayList(value.Count);
            for (int i = 0; i < value.Count; i++)
            {
                var arrayValueNode = rootNode[i];
                arrayList.Add(formatter.Deserialize(ref arrayValueNode, options));
            }
            return arrayList;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(ArrayList));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ArrayList target, CsTomlSerializerOptions options) where TBufferWriter : IBufferWriter<byte>
    {
        if (target == null)
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(ArrayList));
            return;
        }

        writer.BeginArray();
        if (target.Count == 0)
        {
            writer.EndArray();
            return;
        }

        var formatter = options.Resolver.GetFormatter<object>()!;
        formatter.Serialize(ref writer, target[0]!, options);
        if (target.Count == 1)
        {
            writer.WriteSpace();
            writer.EndArray();
            return;
        }

        for (int i = 1; i < target.Count; i++)
        {
            writer.Write(TomlCodes.Symbol.COMMA);
            writer.WriteSpace();
            formatter.Serialize(ref writer, target[i]!, options);
        }
        writer.WriteSpace();
        writer.EndArray();
    }
}