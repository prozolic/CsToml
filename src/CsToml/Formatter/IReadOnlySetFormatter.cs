
using System.Collections.ObjectModel;

namespace CsToml.Formatter;

public sealed class IReadOnlySetFormatter<T> : CollectionBaseFormatter<IReadOnlySet<T>, T, HashSet<T>>
{
    protected override void AddValue(HashSet<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override IReadOnlySet<T> Complete(HashSet<T> collection)
    {
        return collection;
    }

    protected override HashSet<T> CreateCollection(int capacity)
    {
        return new HashSet<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, IReadOnlySet<T> target, CsTomlSerializerOptions options)
    {
        if (target is HashSet<T> hashsetTarget)
        {
            writer.BeginArray();
            if (hashsetTarget.Count == 0)
            {
                writer.EndArray();
                return;
            }

            // Use HashSet<T>.GetEnumerator directly instead of IEnumerable<T>.GetEnumerator.
            var en = hashsetTarget.GetEnumerator();
            en.MoveNext();

            var formatter = options.Resolver.GetFormatter<T>()!;
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
            writer.WriteSpace();
            writer.EndArray();
            return;
        }

        IEnumerableSerializer<T>.Serialize(ref writer, new CollectionContent(target), options);
    }

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, IReadOnlySet<T> target, CsTomlSerializerOptions options)
    {
        if (target.Count == 0)
        {
            return false;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        foreach (var item in target)
        {
            writer.BeginArrayOfTablesHeader();
            writer.WriteKey(header);
            writer.EndArrayOfTablesHeader();
            writer.WriteNewLine();
            writer.BeginCurrentState(TomlValueState.ArrayOfTableForMulitiLine);
            formatter.Serialize(ref writer, item, options);
            writer.EndCurrentState();
            writer.EndKeyValue(false);
        }

        // Return true if serialized in header style.
        return true;
    }
}
