using CsToml.Error;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace CsToml.Formatter;

public sealed class ListFormatter<T> : CollectionBaseFormatter<List<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override List<T> Complete(List<T> collection)
    {
        return collection;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, List<T> target, CsTomlSerializerOptions options)
    {
        writer.BeginArray();
        if (target.Count == 0)
        {
            writer.EndArray();
            return;
        }

        var targetSpan = CollectionsMarshal.AsSpan(target);
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

    }

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, List<T> target, CsTomlSerializerOptions options)
    {
        var targetSpan = CollectionsMarshal.AsSpan(target);
        if (targetSpan.Length == 0)
        {
            // Header-style table arrays must have at least one element
            // If there are 0 elements, return false to serialize in inline table format.
            return false;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        for (int i = 0; i < targetSpan.Length; i++)
        {
            writer.BeginArrayOfTablesHeader();
            writer.WriteKey(header);
            writer.EndArrayOfTablesHeader();
            writer.WriteNewLine();
            writer.BeginCurrentState(TomlValueState.ArrayOfTableForMulitiLine);
            formatter.Serialize(ref writer, targetSpan[i], options);
            writer.EndCurrentState();
            writer.EndKeyValue(false);
        }

        // Return true if serialized in header style.
        return true;
    }
}
