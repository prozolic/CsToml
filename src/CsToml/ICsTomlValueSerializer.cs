
using System.Buffers;

namespace CsToml;

public interface ICsTomlValueSerializer
{
    static abstract void Serialize<TBufferWriter>(ref TBufferWriter writer, long value)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract void Serialize<TBufferWriter>(ref TBufferWriter writer, bool value)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract void Serialize<TBufferWriter>(ref TBufferWriter writer, double value)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract void Serialize<TBufferWriter>(ref TBufferWriter writer, DateTime value)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract void Serialize<TBufferWriter>(ref TBufferWriter writer, DateTimeOffset value)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract void Serialize<TBufferWriter>(ref TBufferWriter writer, DateOnly value)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract void Serialize<TBufferWriter>(ref TBufferWriter writer, TimeOnly value)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract void Serialize<TBufferWriter>(ref TBufferWriter writer, ReadOnlySpan<char> value)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract void SerializeDynamic<TBufferWriter>(ref TBufferWriter writer, dynamic value)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract void Serialize<TBufferWriter, TArrayItem>(ref TBufferWriter writer, IEnumerable<TArrayItem> value)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract void SerializeKey<TBufferWriter>(ref TBufferWriter writer, ReadOnlySpan<char> value)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract void SerializeTableHeader<TBufferWriter>(ref TBufferWriter writer, ReadOnlySpan<char> value)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract void SerializeArrayOfTablesHeader<TBufferWriter>(ref TBufferWriter writer, ReadOnlySpan<char> value)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract void SerializeNewLine<TBufferWriter>(ref TBufferWriter writer)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract void SerializeEqual<TBufferWriter>(ref TBufferWriter writer)
        where TBufferWriter : IBufferWriter<byte>;

}
