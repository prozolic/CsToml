using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

public abstract class ArrayBaseFormatter<TArray, TElement> : ITomlValueFormatter<TArray?>
{
    public TArray? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetArray(out var value))
        {
            var formatter = options.Resolver.GetFormatter<TElement>()!;
            var array = new TElement[value.Count];
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
        var targetSpan = AsSpan(target);
        writer.BeginArray();
        if (targetSpan.Length == 0)
        {
            writer.EndArray();
            return;
        }

        var formatter = options.Resolver.GetFormatter<TElement>()!;
        formatter.Serialize(ref writer, targetSpan[0], options);
        if (targetSpan.Length == 1)
        {
            writer.WriteSpace();
            writer.EndArray();
            return;
        }

        for (int i = 1; i < targetSpan.Length; i++)
        {
            writer.Write(TomlCodes.Symbol.COMMA);
            writer.WriteSpace();
            formatter.Serialize(ref writer, targetSpan[i], options);
        }
        writer.WriteSpace();
        writer.EndArray();
    }

    protected abstract ReadOnlySpan<TElement> AsSpan(TArray array);

    protected abstract TArray Complete(TElement[] array);
}

