using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

public abstract class CollectionBaseFormatter<TCollection, TElement, TMediator> : ITomlValueFormatter<TCollection?>
    where TCollection : IEnumerable<TElement>
{
    public TCollection? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetArray(out var value))
        {
            var formatter = options.Resolver.GetFormatter<TElement>()!;

            var collection = CreateCollection(value.Count);
            for (int i = 0; i < value.Count; i++)
            {
                var arrayValueNode = rootNode[i];
                AddValue(collection, formatter.Deserialize(ref arrayValueNode, options));
            }
            return Complete(collection);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(TCollection));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TCollection? target, CsTomlSerializerOptions options) 
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target == null)
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(TCollection));
            return;
        }
        var count = GetCount(target);
        if (!count.HasValue)
        {
            var collection = CreateCollection(0);
            foreach (var i in target)
            {
                AddValue(collection, i);
            }

            options.Resolver.GetFormatter<TMediator>()!.Serialize(ref writer, collection, options);
            return;
        }

        var formatter = options.Resolver.GetFormatter<TElement>()!;
        writer.BeginArray();
        using (IEnumerator<TElement?> en = target.GetEnumerator())
        {
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
        }
        writer.WriteSpace();
        writer.EndArray();
    }

    private int? GetCount(TCollection collection)
    {
        if (Enumerable.TryGetNonEnumeratedCount(collection, out var count))
        {
            return count;
        }

        return null;
    }

    protected abstract TMediator CreateCollection(int capacity);
    protected abstract void AddValue(TMediator mediator, TElement element);
    protected abstract TCollection Complete(TMediator collection);
}

