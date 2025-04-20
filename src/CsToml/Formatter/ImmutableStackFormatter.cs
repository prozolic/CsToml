using System.Collections.Immutable;

namespace CsToml.Formatter;

public sealed class ImmutableStackFormatter<T> : CollectionBaseFormatter<ImmutableStack<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override ImmutableStack<T> Complete(List<T> collection)
    {
        var stack = ImmutableStack<T>.Empty;
        for (var i = collection.Count - 1; i >= 0; i--)
        {
            stack = stack.Push(collection[i]);
        }
        return stack;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ImmutableStack<T> target, CsTomlSerializerOptions options)
    {
        writer.BeginArray();
        if (target.IsEmpty)
        {
            writer.EndArray();
            return;
        }

        // Use ImmutableStack<T>.GetEnumerator directly instead of IEnumerable<T>.GetEnumerator.
        var en = target.GetEnumerator();
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
    }
}
