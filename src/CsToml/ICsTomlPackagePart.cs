
using System.Buffers;

namespace CsToml;

public interface ICsTomlPackagePart<T>
{
    static abstract void Serialize<TBufferWriter, TSerializer>(ref TBufferWriter writer, ref T target)
        where TBufferWriter : IBufferWriter<byte>
        where TSerializer : ICsTomlValueSerializer;
}
