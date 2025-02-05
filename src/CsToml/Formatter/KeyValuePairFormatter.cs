using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

public sealed class KeyValuePairFormatter<TKey, TValue> : ITomlValueFormatter<KeyValuePair<TKey, TValue>>
{
    public KeyValuePair<TKey, TValue> Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetArray(out var array))
        {
            if (array.Count != 2)
            {
                ExceptionHelper.ThrowInvalidTupleCount();
                return default!;
            }

            var keyNode = rootNode[0];
            var key = options.Resolver.GetFormatter<TKey>()!.Deserialize(ref keyNode, options);
            var valueNode = rootNode[1];
            var value = options.Resolver.GetFormatter<TValue>()!.Deserialize(ref valueNode, options);

            return new KeyValuePair<TKey, TValue>(key, value);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(KeyValuePair<TKey, TValue>));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, KeyValuePair<TKey, TValue> target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.BeginArray();
        options.Resolver.GetFormatter<TKey>()!.Serialize(ref writer, target.Key, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<TValue>()!.Serialize(ref writer, target.Value, options);
        writer.WriteSpace();
        writer.EndArray();
    }
}
