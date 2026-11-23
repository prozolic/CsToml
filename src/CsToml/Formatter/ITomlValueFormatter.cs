namespace CsToml.Formatter;

public interface ITomlValueFormatter<T>
{
    T Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options);

    void Serialize(ref Utf8TomlDocumentWriter writer, T target, CsTomlSerializerOptions options);
}

public interface ITomlArrayHeaderFormatter<TArray>
{
    bool TrySerialize(ref Utf8TomlDocumentWriter writer, ReadOnlySpan<byte> header, TArray target, CsTomlSerializerOptions options);
}
