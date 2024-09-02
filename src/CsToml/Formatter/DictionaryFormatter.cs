using CsToml.Error;
using CsToml.Formatter.Resolver;
using CsToml.Values;
using System.Buffers;

namespace CsToml.Formatter;

internal sealed class DictionaryFormatter : ITomlValueFormatter<Dictionary<string, object?>>
{
    public static readonly DictionaryFormatter Instance = new DictionaryFormatter();

    public Dictionary<string, object?> Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        return options.Resolver.GetFormatter<IDictionary<string, object?>>()!.
            Deserialize(ref rootNode, options) as Dictionary<string, object?> ?? default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Dictionary<string, object?> target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        options.Resolver.GetFormatter<IDictionary<string, object?>>()!.Serialize(ref writer, target, options);
    }
}

internal sealed class IDictionaryFormatter : ITomlValueFormatter<IDictionary<string, object?>>
{
    public static readonly IDictionaryFormatter Instance = new IDictionaryFormatter();

    public IDictionary<string, object?> Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetDictionary(out var value))
        {
            return new Dictionary<string, object?>(value);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(IDictionary<string, object?>));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, IDictionary<string, object?> target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        var count = 0;
        writer.BeginInlineTable();
        foreach (var (key, value) in target)
        {
            TomlDotKey.ParseKey(key).ToTomlString(ref writer);
            writer.WriteEqual();
            PrimitiveObjectFormatter.Instance.Serialize(ref writer, value!, options);

            if (++count != target.Count)
            {
                writer.Write(TomlCodes.Symbol.COMMA);
                writer.WriteSpace();
            }
        }
        writer.EndInlineTable();
    }
}