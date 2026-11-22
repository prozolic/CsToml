using CsToml.Error;
using CsToml.Utility;
using System.Buffers;
using System.Globalization;
using System.Numerics;

namespace CsToml.Tests;

public class SerializeUtf8WriterTest
{
    private static TomlDocument CreateDocument()
        => CsTomlSerializer.Deserialize<TomlDocument>("[table]\nkey = \"value\"\nnumber = 123\n"u8);

    [Test]
    public void SerializeTwiceIntoSameWriter()
    {
        var document = CreateDocument();
        using var serialized = CsTomlSerializer.Serialize(document);
        var expected = serialized.ByteSpan.ToArray();

        Span<byte> destination = stackalloc byte[256];
        var writer = new Utf8SpanWriter(destination);
        CsTomlSerializer.Serialize<TomlDocument, Utf8SpanWriter>(ref writer, document);
        CsTomlSerializer.Serialize<TomlDocument, Utf8SpanWriter>(ref writer, document);

        writer.WrittenSpan.ToArray().ShouldBe(expected.Concat(expected).ToArray());
    }

    [Test]
    public void SerializeValueType()
    {
        var dict = new Dictionary<string, object> { ["x"] = 1 };

        Span<byte> destination = stackalloc byte[64];
        var writer = new Utf8SpanWriter(destination);
        CsTomlSerializer.SerializeValueType<Dictionary<string, object>, Utf8SpanWriter>(ref writer, dict);
        writer.WrittenSpan.ToArray().ShouldBe("{x = 1}"u8.ToArray());

        var bufferWriter = new ArrayBufferWriter<byte>();
        CsTomlSerializer.SerializeValueType<ArrayBufferWriter<byte>, Dictionary<string, object>>(ref bufferWriter, dict);
        bufferWriter.WrittenSpan.ToArray().ShouldBe("{x = 1}"u8.ToArray());
    }

    [Test]
    public void ExactFitSpanWriter()
    {
        var doubleWriter = new Utf8SpanWriter(stackalloc byte[3]);
        CsTomlSerializer.SerializeValueType<double, Utf8SpanWriter>(ref doubleWriter, 1.5);
        doubleWriter.WrittenSpan.ToArray().ShouldBe("1.5"u8.ToArray());

        var decimalWriter = new Utf8SpanWriter(stackalloc byte[3]);
        CsTomlSerializer.SerializeValueType<decimal, Utf8SpanWriter>(ref decimalWriter, 1.5m);
        decimalWriter.WrittenSpan.ToArray().ShouldBe("1.5"u8.ToArray());

        var dateTimeWriter = new Utf8SpanWriter(stackalloc byte[19]);
        CsTomlSerializer.SerializeValueType<DateTime, Utf8SpanWriter>(ref dateTimeWriter, new DateTime(2024, 10, 20, 15, 16, 0));
        dateTimeWriter.WrittenSpan.ToArray().ShouldBe("2024-10-20T15:16:00"u8.ToArray());

        var dateTimeOffsetWriter = new Utf8SpanWriter(stackalloc byte[20]);
        CsTomlSerializer.SerializeValueType<DateTimeOffset, Utf8SpanWriter>(ref dateTimeOffsetWriter, new DateTimeOffset(2024, 10, 20, 15, 16, 0, TimeSpan.Zero));
        dateTimeOffsetWriter.WrittenSpan.ToArray().ShouldBe("2024-10-20T15:16:00Z"u8.ToArray());
    }

#if NET11_0_OR_GREATER
    [Test]
    public void Decimal128WithoutFraction()
    {
        var value = Decimal128.Parse("1234567890123456789", CultureInfo.InvariantCulture);

        var writer = new Utf8SpanWriter(stackalloc byte[19]);
        CsTomlSerializer.SerializeValueType<Decimal128, Utf8SpanWriter>(ref writer, value);
        writer.WrittenSpan.ToArray().ShouldBe("1234567890123456789"u8.ToArray());

        var bufferWriter = new ConsumingWriter();
        CsTomlSerializer.SerializeValueType<ConsumingWriter, Decimal128>(ref bufferWriter, value);
        bufferWriter.Output.WrittenSpan.ToArray().ShouldBe("1234567890123456789"u8.ToArray());
    }
#endif

    [Test]
    public void SpanWriterTooSmall()
    {
        var document = CreateDocument();

        Span<byte> destination = stackalloc byte[10];
        var writer = new Utf8SpanWriter(destination);
        CsTomlSerializeException? exception = null;
        try
        {
            CsTomlSerializer.Serialize<TomlDocument, Utf8SpanWriter>(ref writer, document);
        }
        catch (CsTomlSerializeException e)
        {
            exception = e;
        }

        exception.ShouldNotBeNull();
        writer.WrittenLength.ShouldBeGreaterThan(0);
    }

    [Test]
    public void ValueTypeBufferWriter()
    {
        var document = CreateDocument();
        using var expected = CsTomlSerializer.Serialize(document);

        var bufferWriter = new ValueBufferWriter(new byte[256]);
        CsTomlSerializer.Serialize<ValueBufferWriter, TomlDocument>(ref bufferWriter, document);

        bufferWriter.WrittenSpan.ToArray().ShouldBe(expected.ByteSpan.ToArray());
    }

    [Test]
    public void ConsumingBufferWriter()
    {
        var document = CreateDocument();
        using var expected = CsTomlSerializer.Serialize(document);

        var bufferWriter = new ConsumingWriter();
        CsTomlSerializer.Serialize<ConsumingWriter, TomlDocument>(ref bufferWriter, document);

        bufferWriter.Output.WrittenSpan.ToArray().ShouldBe(expected.ByteSpan.ToArray());
    }

    private sealed class ConsumingWriter : IBufferWriter<byte>
    {
        private readonly byte[] buffer = new byte[256];

        public ArrayBufferWriter<byte> Output { get; } = new();

        public void Advance(int count)
        {
            Output.Write(buffer.AsSpan(0, count));
            buffer.AsSpan().Clear();
        }

        public Memory<byte> GetMemory(int sizeHint = 0) => buffer;

        public Span<byte> GetSpan(int sizeHint = 0) => buffer;
    }
}
