
using CsToml.Utility;
using System.Buffers;

namespace CsToml.Formatter;

internal interface ITomlValueFormatter<T>
{
    void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, T value)
        where TBufferWriter : IBufferWriter<byte>;

    void Deserialize(ReadOnlySpan<byte> bytes, ref T value);
}

