using CsToml.Error;
using CsToml.Values;
using System.ComponentModel;

namespace CsToml.Formatter;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class CollectionBaseFormatter<TCollection, TElement, TMediator> : ITomlValueFormatter<TCollection?>, ITomlArrayHeaderFormatter<TCollection?>
    where TCollection : IEnumerable<TElement>
{
    public TCollection? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.Value is TomlArray tomlArray)
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

        ExceptionHelper.ThrowDeserializationFailed(typeof(TCollection), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, TCollection? target, CsTomlSerializerOptions options)
    {
        if (target == null)
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(TCollection));
            return;
        }

        SerializeCollection(ref writer, target, options);
    }

    protected abstract void SerializeCollection(ref Utf8TomlDocumentWriter writer, TCollection target, CsTomlSerializerOptions options);

    public bool TrySerialize(ref Utf8TomlDocumentWriter writer, ReadOnlySpan<byte> header, TCollection? target, CsTomlSerializerOptions options)
    {
        if (target == null)
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(TCollection?));
            return false; // not reached.
        }

        return TrySerializeTomlArrayHeaderStyle(ref writer, header, target, options);
    }

    protected abstract bool TrySerializeTomlArrayHeaderStyle(ref Utf8TomlDocumentWriter writer, ReadOnlySpan<byte> header, TCollection target, CsTomlSerializerOptions options);

    protected abstract TMediator CreateCollection(int capacity);
    protected abstract void AddValue(TMediator mediator, TElement element);
    protected abstract TCollection Complete(TMediator collection);
}
