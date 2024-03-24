using CsToml.Utility;
using System.Buffers;

namespace CsToml.Formatter;

internal interface ICsTomlSpanFormatter<T>
    where T : struct
{
    static abstract void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, ReadOnlySpan<T> value)
        where TBufferWriter : IBufferWriter<byte>;
}

