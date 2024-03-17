
using System.Buffers;

namespace CsToml;

public interface ICsTomlValueSerializer
{
    static abstract void Serialize<TWriter>(ref TWriter writer, long value)
        where TWriter : IBufferWriter<byte>;

    static abstract void Serialize<TWriter>(ref TWriter writer, bool value)
        where TWriter : IBufferWriter<byte>;

    static abstract void Serialize<TWriter>(ref TWriter writer, double value)
        where TWriter : IBufferWriter<byte>;

    static abstract void Serialize<TWriter>(ref TWriter writer, DateTime value)
        where TWriter : IBufferWriter<byte>;

    static abstract void Serialize<TWriter>(ref TWriter writer, DateTimeOffset value)
        where TWriter : IBufferWriter<byte>;

    static abstract void Serialize<TWriter>(ref TWriter writer, DateOnly value)
        where TWriter : IBufferWriter<byte>;

    static abstract void Serialize<TWriter>(ref TWriter writer, TimeOnly value)
        where TWriter : IBufferWriter<byte>;

    static abstract void Serialize<TWriter>(ref TWriter writer, ReadOnlySpan<char> value)
        where TWriter : IBufferWriter<byte>;

    static abstract void SerializeDynamic<TWriter>(ref TWriter writer, dynamic value)
        where TWriter : IBufferWriter<byte>;

    static abstract void SerializeKey<TWriter>(ref TWriter writer, ReadOnlySpan<char> value)
        where TWriter : IBufferWriter<byte>;

    static abstract void Serialize<TWriter, TArrayItem>(ref TWriter writer, IEnumerable<TArrayItem> value)
        where TWriter : IBufferWriter<byte>;

}
