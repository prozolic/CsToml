
namespace CsToml;

public interface ITomlSerializedObject<T> : ITomlSerializedObjectRegister
{
    static abstract void Serialize(ref Utf8TomlDocumentWriter writer, T target, CsTomlSerializerOptions options);

    static abstract T Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options);
}

public interface ITomlSerializedObjectRegister
{
    static abstract void Register();
}


