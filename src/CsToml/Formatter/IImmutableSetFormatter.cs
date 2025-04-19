﻿using System.Collections.Immutable;

namespace CsToml.Formatter;

public sealed class IImmutableSetFormatter<T> : CollectionBaseFormatter<IImmutableSet<T>, T, HashSet<T>>
{
    protected override void AddValue(HashSet<T> mediator, T element)
    {
        mediator.Add(element);
    }
    protected override IImmutableSet<T> Complete(HashSet<T> collection)
    {
        return collection.ToImmutableHashSet();
    }
    protected override HashSet<T> CreateCollection(int capacity)
    {
        return new HashSet<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, IImmutableSet<T> target, CsTomlSerializerOptions options)
    {
        if (target is ImmutableHashSet<T> immQueueTarget)
        {
            if (immQueueTarget.IsEmpty)
            {
                writer.BeginArray();
                writer.EndArray();
                return;
            }

            var formatter = options.Resolver.GetFormatter<T>()!;

            writer.BeginArray();

            // Use ImmutableHashSet<T>.GetEnumerator directly instead of IEnumerable<T>.GetEnumerator.
            var en = immQueueTarget.GetEnumerator();
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
