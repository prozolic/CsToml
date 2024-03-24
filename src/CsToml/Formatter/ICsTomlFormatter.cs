
using CsToml.Utility;
using System.Buffers;

namespace CsToml.Formatter;

internal interface ICsTomlFormatter<T>
{
    static abstract void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, T value)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract T Deserialize(ref Utf8Reader reader, int length);
}
