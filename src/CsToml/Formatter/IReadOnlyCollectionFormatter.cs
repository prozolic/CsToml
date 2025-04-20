
using System.Runtime.InteropServices;

namespace CsToml.Formatter;

public sealed class IReadOnlyCollectionFormatter<T> : CollectionBaseFormatter<IReadOnlyCollection<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override IReadOnlyCollection<T> Complete(List<T> collection)
    {
        return collection;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, IReadOnlyCollection<T> target, CsTomlSerializerOptions options)
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
        if (target is IList<T> iListTarget)
        {
            writer.BeginArray();
            if (iListTarget.Count == 0)
            {
                writer.EndArray();
                return;
            }

            var formatter = options.Resolver.GetFormatter<T>()!;
            formatter.Serialize(ref writer, iListTarget[0], options);
            if (iListTarget.Count == 1)
            {
                writer.WriteSpace();
                writer.EndArray();
                return;
            }

            for (int i = 1; i < iListTarget.Count; i++)
            {
                writer.Write(TomlCodes.Symbol.COMMA);
                writer.WriteSpace();
                formatter.Serialize(ref writer, iListTarget[i], options);
            }
            writer.WriteSpace();
            writer.EndArray();
            return;
        }
        if (target is IReadOnlyList<T> iReadOnlyListTarget)
        {
            writer.BeginArray();
            if (iReadOnlyListTarget.Count == 0)
            {
                writer.EndArray();
                return;
            }

            var formatter = options.Resolver.GetFormatter<T>()!;
            formatter.Serialize(ref writer, iReadOnlyListTarget[0], options);
            if (iReadOnlyListTarget.Count == 1)
            {
                writer.WriteSpace();
                writer.EndArray();
                return;
            }

            for (int i = 1; i < iReadOnlyListTarget.Count; i++)
            {
                writer.Write(TomlCodes.Symbol.COMMA);
                writer.WriteSpace();
                formatter.Serialize(ref writer, iReadOnlyListTarget[i], options);
            }
            writer.WriteSpace();
            writer.EndArray();
            return;
        }

        base.SerializeCollection(ref writer, target, options);
    }
}