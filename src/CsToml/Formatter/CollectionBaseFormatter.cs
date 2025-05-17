using CsToml.Error;
using CsToml.Extension;
using CsToml.Values;
using System.Buffers;
using System.ComponentModel;

namespace CsToml.Formatter;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class CollectionBaseFormatter<TCollection, TElement, TMediator> : ITomlValueFormatter<TCollection?>
    where TCollection : IEnumerable<TElement>
{
    public TCollection? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.CanGetValue(TomlValueFeature.Array) && rootNode.Value is TomlArray tomlArray)
        {
            var formatter = options.Resolver.GetFormatter<TElement>()!;

            var collection = CreateCollection(tomlArray.Count);
            for (int i = 0; i < tomlArray.Count; i++)
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

        SerializeCollection(ref writer, target, options);
    }

    protected virtual void SerializeCollection<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TCollection target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.TryGetNonEnumeratedCount2(out var count))
        {
            if (count == 0)
            {
                writer.BeginArray();
                writer.EndArray();
                return;
            }
        }

        var formatter = options.Resolver.GetFormatter<TElement>()!;
        writer.BeginArray();
        using (var en = target.GetEnumerator())
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


    protected abstract TMediator CreateCollection(int capacity);
    protected abstract void AddValue(TMediator mediator, TElement element);
    protected abstract TCollection Complete(TMediator collection);
}

