
using System.Runtime.InteropServices;

namespace CsToml.Formatter;

public sealed class ICollectionFormatter<T> : CollectionBaseFormatter<ICollection<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override ICollection<T> Complete(List<T> collection)
    {
        return collection;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ICollection<T> target, CsTomlSerializerOptions options)
    {
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

        base.SerializeCollection(ref writer, target, options);
    }
}
