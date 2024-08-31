
using System.Buffers;

namespace CsToml;

public interface ITomlSerializedObject<T> : ITomlSerializedObjectRegister
{
    static abstract void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, T target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract T Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options);
}

public interface ITomlSerializedObjectRegister
{
    static abstract void Register();
}


