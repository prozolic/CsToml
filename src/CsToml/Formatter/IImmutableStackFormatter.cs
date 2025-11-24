using System.Collections.Immutable;

namespace CsToml.Formatter;

public sealed class IImmutableStackFormatter<T> : CollectionBaseFormatter<IImmutableStack<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override IImmutableStack<T> Complete(List<T> collection)
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

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, IImmutableStack<T> target, CsTomlSerializerOptions options)
    {
        if (target is ImmutableStack<T> immStackTarget)
        {
            writer.BeginArray();
            if (immStackTarget.IsEmpty)
            {
                writer.EndArray();
                return;
            }

            // Use ImmutableStack<T>.GetEnumerator directly instead of IEnumerable<T>.GetEnumerator.
            var en = immStackTarget.GetEnumerator();
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
        else
        {

            writer.BeginArray();
            if (target.IsEmpty)
            {
                writer.EndArray();
                return;
            }

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

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, IImmutableStack<T> target, CsTomlSerializerOptions options)
    {
        if (target.IsEmpty)
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