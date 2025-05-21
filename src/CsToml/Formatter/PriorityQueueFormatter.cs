using CsToml.Error;
using CsToml.Values;
using System.Buffers;

namespace CsToml.Formatter;

public sealed class PriorityQueueFormatter<TElement, TPriority> : ITomlValueFormatter<PriorityQueue<TElement, TPriority>>
{
    public PriorityQueue<TElement, TPriority> Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.CanGetValue(TomlValueFeature.Array) && rootNode.Value is TomlArray tomlArray)
        {
            var formatter = options.Resolver.GetFormatter<ValueTuple<TElement, TPriority>>()!;
            var priorityQueue = new PriorityQueue<TElement, TPriority>();
            for (int i = 0; i < tomlArray.Count; i++)
            {
                var arrayValueNode = rootNode[i];
                var item = formatter.Deserialize(ref arrayValueNode, options);
                priorityQueue.Enqueue(item.Item1, item.Item2);
            }

            return priorityQueue;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(PriorityQueue<TElement, TPriority>));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, PriorityQueue<TElement, TPriority> target, CsTomlSerializerOptions options) where TBufferWriter : IBufferWriter<byte>
    {
        if (target == null)
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(PriorityQueue<TElement, TPriority>));
            return;
        }

        var formatter = options.Resolver.GetFormatter<ValueTuple<TElement, TPriority>>()!;
        writer.BeginArray();
        using (IEnumerator<(TElement, TPriority)> en = target.UnorderedItems.GetEnumerator())
        {
            if (!en.MoveNext())
            {
                writer.EndArray();
                return;
            }

            formatter.Serialize(ref writer, en.Current!, options);
            if (!en.MoveNext())
            {
                writer.WriteSpace();
                writer.EndArray();
                return;
            }

            do
            {
                writer.Write(TomlCodes.Symbol.COMMA);
                writer.WriteSpace();
                formatter.Serialize(ref writer, en.Current!, options);

            } while (en.MoveNext());
        }
        writer.WriteSpace();
        writer.EndArray();
    }
}
