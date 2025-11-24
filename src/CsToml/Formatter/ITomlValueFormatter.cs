using System.Buffers;

namespace CsToml.Formatter;

public interface ITomlValueFormatter<T>
{
    T Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options);

    void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, T target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>;
}

public interface ITomlArrayHeaderFormatter<TArray>
{
    bool TrySerialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, TArray target, CsTomlSerializerOptions options) where TBufferWriter : IBufferWriter<byte>;
}
