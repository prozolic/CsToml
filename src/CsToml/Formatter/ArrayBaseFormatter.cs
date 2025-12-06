using CsToml.Error;
using CsToml.Values;
using System.Buffers;
using System.ComponentModel;

namespace CsToml.Formatter;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class ArrayBaseFormatter<TArray, TElement> : ITomlValueFormatter<TArray?>, ITomlArrayHeaderFormatter<TArray?>
{
    public TArray? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.CanGetValue(TomlValueFeature.Array) && rootNode.Value is TomlArray tomlArray)
        {
            var formatter = options.Resolver.GetFormatter<TElement>()!;
            var array = new TElement[tomlArray.Count];
            var arraySpan = array.AsSpan();
            for (int i = 0; i < arraySpan.Length; i++)
            {
                var arrayValueNode = rootNode[i];
                arraySpan[i] = formatter.Deserialize(ref arrayValueNode, options);
            }
            return Complete(array);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(TArray));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TArray? target, CsTomlSerializerOptions options) where TBufferWriter : IBufferWriter<byte>
    {
        if (target == null)
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(TArray));
            return;
        }

        ArraySerializer<TElement>.Serialize(ref writer, new CollectionContent(target), options);
    }

    bool ITomlArrayHeaderFormatter<TArray?>.TrySerialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, TArray? target, CsTomlSerializerOptions options)
    {
        if (target == null)
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(TArray));
            return false; // not reached.
        }

        return ArraySerializer<TElement>.TrySerializeTomlArrayHeaderStyle(ref writer, header, new CollectionContent(target), options);
    }

    protected abstract ReadOnlySpan<TElement> AsSpan(TArray array);

    protected abstract TArray Complete(TElement[] array);
}
