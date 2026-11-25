using CsToml.Error;
using System.Buffers;
using System.Globalization;
using System.Numerics;

namespace CsToml.Tests;

public class SerializeUtf8WriterTest
{
    private static TomlDocument CreateDocument()
        => CsTomlSerializer.Deserialize<TomlDocument>("root = 1\n[table]\nkey = \"value\"\nnumber = 123\n"u8);

    private static Dictionary<string, object> CreateLargeValue()
    {
        var value = new Dictionary<string, object>();
        for (var i = 0; i < 300; i++)
        {
            value[$"key{i}"] = (i % 3) switch
            {
                0 => (object)$"value-{i}-{new string('x', i % 17)}",
                1 => (object)(i * 1.25),
                _ => (object)new Dictionary<string, object> { ["nested"] = (long)i, ["list"] = new object[] { i, $"s{i}" } },
            };
        }
        return value;
    }

    [Test]
    public void SerializeTwiceIntoSameBufferWriter()
    {
        var document = CreateDocument();
        using var serialized = CsTomlSerializer.Serialize(document);
        var expected = serialized.ByteSpan.ToArray();

        var bufferWriter = new ArrayBufferWriter<byte>();
        CsTomlSerializer.Serialize(bufferWriter, document);
        CsTomlSerializer.Serialize(bufferWriter, document);
        bufferWriter.WrittenSpan.ToArray().ShouldBe(expected.Concat(expected).ToArray());

        IBufferWriter<byte> boxed = new ValueBufferWriter(new byte[512]);
        CsTomlSerializer.Serialize(boxed, document);
        CsTomlSerializer.Serialize(boxed, document);
        ((ValueBufferWriter)boxed).WrittenSpan.ToArray().ShouldBe(expected.Concat(expected).ToArray());
    }

    [Test]
    public void SerializeValueType()
    {
        var dict = new Dictionary<string, object> { ["x"] = 1 };

        var destination = new byte[64];
        var writtenSize = DocumentWriterHelper.SerializeToFixedSpan(destination, dict, true);
        destination.AsSpan(0, (int)writtenSize).ToArray().ShouldBe("{x = 1}"u8.ToArray());

        var bufferWriter = new ArrayBufferWriter<byte>();
        CsTomlSerializer.SerializeValueType<Dictionary<string, object>>(bufferWriter, dict);
        bufferWriter.WrittenSpan.ToArray().ShouldBe("{x = 1}"u8.ToArray());
    }

    [Test]
    public void ExactFitFixedSpan()
    {
        var doubleDestination = new byte[3];
        DocumentWriterHelper.SerializeToFixedSpan(doubleDestination, 1.5, true).ShouldBe(3);
        doubleDestination.ShouldBe("1.5"u8.ToArray());

        var decimalDestination = new byte[3];
        DocumentWriterHelper.SerializeToFixedSpan(decimalDestination, 1.5m, true).ShouldBe(3);
        decimalDestination.ShouldBe("1.5"u8.ToArray());

        var dateTimeDestination = new byte[19];
        DocumentWriterHelper.SerializeToFixedSpan(dateTimeDestination, new DateTime(2024, 10, 20, 15, 16, 0), true).ShouldBe(19);
        dateTimeDestination.ShouldBe("2024-10-20T15:16:00"u8.ToArray());

        var dateTimeOffsetDestination = new byte[20];
        DocumentWriterHelper.SerializeToFixedSpan(dateTimeOffsetDestination, new DateTimeOffset(2024, 10, 20, 15, 16, 0, TimeSpan.Zero), true).ShouldBe(20);
        dateTimeOffsetDestination.ShouldBe("2024-10-20T15:16:00Z"u8.ToArray());
    }

#if NET11_0_OR_GREATER
    [Test]
    public void Decimal128WithoutFraction()
    {
        var value = Decimal128.Parse("1234567890123456789", CultureInfo.InvariantCulture);

        var destination = new byte[19];
        DocumentWriterHelper.SerializeToFixedSpan(destination, value, true).ShouldBe(19);
        destination.ShouldBe("1234567890123456789"u8.ToArray());

        var bufferWriter = new ConsumingWriter();
        CsTomlSerializer.SerializeValueType<Decimal128>(bufferWriter, value);
        bufferWriter.Output.WrittenSpan.ToArray().ShouldBe("1234567890123456789"u8.ToArray());
    }
#endif

    [Test]
    public void FixedSpanTooSmall()
    {
        var document = CreateDocument();

        var destination = new byte[10];
        Should.Throw<CsTomlException>(() => DocumentWriterHelper.SerializeToFixedSpan(destination, document, false));

        destination.ShouldContain(b => b != 0);
    }

    [Test]
    public void ValueTypeBufferWriterIsBoxed()
    {
        var document = CreateDocument();
        using var expected = CsTomlSerializer.Serialize(document);

        var bufferWriter = new ValueBufferWriter(new byte[256]);
        CsTomlSerializer.Serialize(bufferWriter, document);
        bufferWriter.WrittenSpan.Length.ShouldBe(0);

        IBufferWriter<byte> boxed = bufferWriter;
        CsTomlSerializer.Serialize(boxed, document);
        ((ValueBufferWriter)boxed).WrittenSpan.ToArray().ShouldBe(expected.ByteSpan.ToArray());
        bufferWriter.WrittenSpan.Length.ShouldBe(0);

        IBufferWriter<byte> boxedValueType = new ValueBufferWriter(new byte[256]);
        CsTomlSerializer.SerializeValueType(boxedValueType, 1.5);
        ((ValueBufferWriter)boxedValueType).WrittenSpan.ToArray().ShouldBe("1.5"u8.ToArray());
    }

    [Test]
    public void NullBufferWriter()
    {
        Should.Throw<ArgumentNullException>(() => CsTomlSerializer.Serialize((IBufferWriter<byte>)null!, CreateDocument()));
        Should.Throw<ArgumentNullException>(() => CsTomlSerializer.Serialize((IBufferWriter<byte>)null!, new Dictionary<string, object> { ["a"] = 1 }));
        Should.Throw<ArgumentNullException>(() => CsTomlSerializer.SerializeValueType((IBufferWriter<byte>)null!, 1.5));
    }

    [Test]
    public void ConsumingBufferWriter()
    {
        var document = CreateDocument();
        using var expected = CsTomlSerializer.Serialize(document);

        var bufferWriter = new ConsumingWriter();
        CsTomlSerializer.Serialize<TomlDocument>(bufferWriter, document);

        bufferWriter.Output.WrittenSpan.ToArray().ShouldBe(expected.ByteSpan.ToArray());
    }

    [Test]
    public void LargeValueAcrossBufferBoundaries()
    {
        var value = CreateLargeValue();
        using var serialized = CsTomlSerializer.Serialize(value);
        var expected = serialized.ByteSpan.ToArray();
        expected.Length.ShouldBeGreaterThan(4096);

        var arrayBufferWriter = new ArrayBufferWriter<byte>(1);
        CsTomlSerializer.Serialize(arrayBufferWriter, value);
        arrayBufferWriter.WrittenSpan.ToArray().ShouldBe(expected);

        var strictWriter = new StrictBufferWriter(7);
        CsTomlSerializer.Serialize(strictWriter, value);
        strictWriter.Verify();
        strictWriter.GetSpanCount.ShouldBeGreaterThan(1);
        strictWriter.WrittenSpan.ToArray().ShouldBe(expected);

        IBufferWriter<byte> valueBufferWriter = new ValueBufferWriter(new byte[expected.Length]);
        CsTomlSerializer.Serialize(valueBufferWriter, value);
        ((ValueBufferWriter)valueBufferWriter).WrittenSpan.ToArray().ShouldBe(expected);

        var consumingWriter = new ConsumingWriter();
        CsTomlSerializer.Serialize(consumingWriter, value);
        consumingWriter.Output.WrittenSpan.ToArray().ShouldBe(expected);

        using var stream = new MemoryStream();
        CsTomlSerializer.Serialize(stream, value);
        stream.ToArray().ShouldBe(expected);

        CsTomlSerializer.Deserialize<Dictionary<string, object>>(expected).Count.ShouldBe(value.Count);

        var document = CsTomlSerializer.Deserialize<TomlDocument>(expected);
        using var documentSerialized = CsTomlSerializer.Serialize(document);
        var documentStrictWriter = new StrictBufferWriter(7);
        CsTomlSerializer.Serialize(documentStrictWriter, document);
        documentStrictWriter.Verify();
        documentStrictWriter.WrittenSpan.ToArray().ShouldBe(documentSerialized.ByteSpan.ToArray());
    }

    [Test]
    public void LargeValueTypeAcrossBufferBoundaries()
    {
        var value = Enumerable.Range(0, 1000).Select(i => (long)i * 1_000_003).ToList();
        using var serialized = CsTomlSerializer.SerializeValueType(value);
        var expected = serialized.ByteSpan.ToArray();
        expected.Length.ShouldBeGreaterThan(1024);

        var strictWriter = new StrictBufferWriter(5);
        CsTomlSerializer.SerializeValueType(strictWriter, value);
        strictWriter.Verify();
        strictWriter.WrittenSpan.ToArray().ShouldBe(expected);

        CsTomlSerializer.DeserializeValueType<List<long>>(expected).ShouldBe(value);
    }

    [Test]
    public void BufferWriterFlushedOnException()
    {
        var value = new Dictionary<string, object> { ["a"] = 1, ["b"] = ulong.MaxValue };
        using var prefix = CsTomlSerializer.Serialize(new Dictionary<string, object> { ["a"] = 1 });
        var expectedPrefix = prefix.ByteSpan.ToArray();

        var strictWriter = new StrictBufferWriter(3);
        Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Serialize(strictWriter, value));
        strictWriter.Verify();
        strictWriter.WrittenSpan.Length.ShouldBeGreaterThan(expectedPrefix.Length);
        strictWriter.WrittenSpan.Slice(0, expectedPrefix.Length).ToArray().ShouldBe(expectedPrefix);

        IBufferWriter<byte> valueBufferWriter = new ValueBufferWriter(new byte[256]);
        Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Serialize(valueBufferWriter, value));
        var written = ((ValueBufferWriter)valueBufferWriter).WrittenSpan;
        written.Length.ShouldBeGreaterThan(expectedPrefix.Length);
        written.Slice(0, expectedPrefix.Length).ToArray().ShouldBe(expectedPrefix);
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
