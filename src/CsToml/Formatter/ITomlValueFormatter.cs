using System.Buffers;

namespace CsToml.Formatter;

public interface ITomlValueFormatter<T>
{
    T Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options);

    void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, T target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>;
}