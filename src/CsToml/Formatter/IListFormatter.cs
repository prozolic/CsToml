
using System.Runtime.InteropServices;

namespace CsToml.Formatter;

public sealed class IListFormatter<T> : CollectionBaseFormatter<IList<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override IList<T> Complete(List<T> collection)
    {
        return collection;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, IList<T> target, CsTomlSerializerOptions options)
    {
        if (target is T[] arrayTarget)
        {
            writer.BeginArray();
            if (arrayTarget.Length == 0)
            {
                writer.EndArray();
                return;
            }

            var targetSpan = arrayTarget.AsSpan();
            var formatter = options.Resolver.GetFormatter<T>()!;
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
            return;
        }
        if (target is List<T> listTarget)
        {
            writer.BeginArray();
            if (listTarget.Count == 0)
            {
                writer.EndArray();
                return;
            }

            var targetSpan = CollectionsMarshal.AsSpan(listTarget);
            var formatter = options.Resolver.GetFormatter<T>()!;
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
            return;
        }

        writer.BeginArray();
        if (target.Count == 0)
        {
            writer.EndArray();
            return;
        }

        var formatter2 = options.Resolver.GetFormatter<T>()!;
        formatter2.Serialize(ref writer, target[0], options);
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
            formatter2.Serialize(ref writer, target[i], options);
        }
        writer.WriteSpace();
        writer.EndArray();
    }
}
