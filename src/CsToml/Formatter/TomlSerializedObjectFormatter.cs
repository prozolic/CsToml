using System.Buffers;

namespace CsToml.Formatter;

public sealed class TomlSerializedObjectFormatter<T> : ITomlValueFormatter<T>
    where T : ITomlSerializedObject<T>
{
    public T Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        return T.Deserialize(ref rootNode, options);
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, T target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        T.Serialize(ref writer, target, options);
    }
}