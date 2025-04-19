using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace CsToml.Formatter;

public sealed class ReadOnlyCollectionFormatter<T> : CollectionBaseFormatter<ReadOnlyCollection<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override ReadOnlyCollection<T> Complete(List<T> collection)
    {
        return new ReadOnlyCollection<T>(collection);
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlyCollection<T> target, CsTomlSerializerOptions options)
    {
        writer.BeginArray();
        if (target.Count == 0)
        {
            writer.EndArray();
            return;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        formatter.Serialize(ref writer, target[0], options);
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
            formatter.Serialize(ref writer, target[i], options);
        }
        writer.WriteSpace();
        writer.EndArray();
    }
}