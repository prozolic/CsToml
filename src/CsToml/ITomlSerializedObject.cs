
using System.Buffers;

namespace CsToml;

public interface ITomlSerializedObject<T>
{
    static abstract void Serialize<TBufferWriter, TSerializer>(ref TBufferWriter writer, T? target, CsTomlSerializerOptions? options = null)
        where TBufferWriter : IBufferWriter<byte>
        where TSerializer : ITomlValueSerializer;

    static abstract T Deserialize<TSerializer>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options = null)
        where TSerializer : ITomlValueSerializer;

    static abstract T Deserialize<TSerializer>(in ReadOnlySequence<byte> tomlTextSequence, CsTomlSerializerOptions? options = null)
        where TSerializer : ITomlValueSerializer;
}
