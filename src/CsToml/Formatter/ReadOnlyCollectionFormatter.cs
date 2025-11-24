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

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, ReadOnlyCollection<T> target, CsTomlSerializerOptions options)
    {
        if (target.Count == 0)
        {
            // Header-style table arrays must have at least one element
            // If there are 0 elements, return false to serialize in inline table format.
            return false;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        for (int i = 0; i < target.Count; i++)
        {
            writer.BeginArrayOfTablesHeader();
            writer.WriteKey(header);
            writer.EndArrayOfTablesHeader();
            writer.WriteNewLine();
            writer.BeginCurrentState(TomlValueState.ArrayOfTableForMulitiLine);
            formatter.Serialize(ref writer, target[i], options);
            writer.EndCurrentState();
            writer.EndKeyValue(false);
        }

        // Return true if serialized in header style.
        return true;
    }
}