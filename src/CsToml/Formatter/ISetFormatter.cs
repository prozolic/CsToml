
namespace CsToml.Formatter;

public sealed class ISetFormatter<T> : CollectionBaseFormatter<ISet<T>, T, HashSet<T>>
{
    protected override void AddValue(HashSet<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override ISet<T> Complete(HashSet<T> collection)
    {
        return collection;
    }

    protected override HashSet<T> CreateCollection(int capacity)
    {
        return new HashSet<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ISet<T> target, CsTomlSerializerOptions options)
    {
        if (target is HashSet<T> hashsetTarget)
        {
            if (hashsetTarget.Count == 0)
            {
                writer.BeginArray();
                writer.EndArray();
                return;
            }

            var formatter = options.Resolver.GetFormatter<T>()!;

            writer.BeginArray();

            // Use HashSet<T>.GetEnumerator directly instead of IEnumerable<T>.GetEnumerator.
            var en = hashsetTarget.GetEnumerator();
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
            writer.WriteSpace();
            writer.EndArray();
            return;
        }

        base.SerializeCollection(ref writer, target, options);
    }
}