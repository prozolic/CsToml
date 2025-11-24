using System.Collections.Immutable;

namespace CsToml.Formatter;

public sealed class IImmutableListFormatter<T> : CollectionBaseFormatter<IImmutableList<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override IImmutableList<T> Complete(List<T> collection)
    {
        return collection.ToImmutableList();
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }

    protected override void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, IImmutableList<T> target, CsTomlSerializerOptions options)
    {
        if (target is ImmutableList<T> immListTarget)
        {
            writer.BeginArray();
            if (immListTarget.Count == 0)
            {
                writer.EndArray();
                return;
            }

            var formatter = options.Resolver.GetFormatter<T>()!;
            formatter.Serialize(ref writer, immListTarget[0], options);
            if (immListTarget.Count == 1)
            {
                writer.WriteSpace();
                writer.EndArray();
                return;
            }

            for (int i = 1; i < immListTarget.Count; i++)
            {
                writer.Write(TomlCodes.Symbol.COMMA);
                writer.WriteSpace();
                formatter.Serialize(ref writer, immListTarget[i], options);
            }
            writer.WriteSpace();
            writer.EndArray();
            return;
        }
        if (target is ImmutableArray<T> immArrayTarget)
        {
            writer.BeginArray();
            if (immArrayTarget.Length == 0)
            {
                writer.EndArray();
                return;
            }

            var formatter = options.Resolver.GetFormatter<T>()!;
            formatter.Serialize(ref writer, immArrayTarget[0], options);
            if (immArrayTarget.Length == 1)
            {
                writer.WriteSpace();
                writer.EndArray();
                return;
            }

            for (int i = 1; i < immArrayTarget.Length; i++)
            {
                writer.Write(TomlCodes.Symbol.COMMA);
                writer.WriteSpace();
                formatter.Serialize(ref writer, immArrayTarget[i], options);
            }
            writer.WriteSpace();
            writer.EndArray();
            return;
        }

        writer.BeginArray();
        if (target.Count == 0)
        {
            writer.EndArray();
            return;
        }

        var formatter2 = options.Resolver.GetFormatter<T>()!;
        formatter2.Serialize(ref writer, target[0], options);
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
            formatter2.Serialize(ref writer, target[i], options);
        }
        writer.WriteSpace();
        writer.EndArray();
    }

    protected override bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, IImmutableList<T> target, CsTomlSerializerOptions options)
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
