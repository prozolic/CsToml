using CsToml.Utility;
using System.Buffers;

namespace CsToml.Formatter;

internal interface ITomlValueSpanFormatter<T>
    where T : struct
{
    void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, ReadOnlySpan<T> value)
        where TBufferWriter : IBufferWriter<byte>;
}

