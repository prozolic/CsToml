using CsToml.Error;
using CsToml.Utility;
using System.Buffers;

namespace CsToml.Tests;

public class Utf8WriterTest
{
    private static readonly byte[] Expected = "key = \"value\"\n"u8.ToArray();

    private static void WriteExpected<TWriter>(ref TWriter writer)
        where TWriter : IUtf8Writer, allows ref struct
    {
        "key"u8.CopyTo(writer.GetSpan(3));
        writer.Advance(3);
        " = "u8.CopyTo(writer.GetWrittenSpan(3));
        "\"value\""u8.CopyTo(writer.GetWrittenSpan(7));
        writer.GetWrittenSpan()[0] = (byte)'\n';
    }

    [Test]
    public void SpanWriter()
    {
        Span<byte> destination = stackalloc byte[Expected.Length];
        var writer = new Utf8SpanWriter(destination);
        WriteExpected(ref writer);

        writer.WrittenLength.ShouldBe(Expected.Length);
        writer.WrittenSpan.ToArray().ShouldBe(Expected);
        writer.GetSpan().Length.ShouldBe(0);
    }

    [Test]
    public void SpanWriterTooSmall()
    {
        Should.Throw<CsTomlException>(() =>
        {
            var writer = new Utf8SpanWriter(new byte[2]);
            writer.GetWrittenSpan(3);
        });
        Should.Throw<CsTomlException>(() =>
        {
            var writer = new Utf8SpanWriter(new byte[2]);
            writer.GetSpan(3);
        });
        Should.Throw<CsTomlException>(() =>
        {
            var writer = new Utf8SpanWriter(new byte[2]);
            writer.Advance(3);
        });
    }

    [Test]
    public void ArrayPoolWriterGrow()
    {
        var expected = new byte[5000];
        Random.Shared.NextBytes(expected);

        var writer = new Utf8ArrayPoolWriter(16);
        try
        {
            var first = writer.GetSpan().Length;
            expected.AsSpan(0, first).CopyTo(writer.GetWrittenSpan(first));
            writer.GetSpan().Length.ShouldBeGreaterThan(0);
            expected.AsSpan(first, 990).CopyTo(writer.GetSpan(990));
            writer.Advance(990);
            expected.AsSpan(first + 990).CopyTo(writer.GetWrittenSpan(expected.Length - first - 990));

            writer.WrittenLength.ShouldBe(expected.Length);
            writer.WrittenSpan.ToArray().ShouldBe(expected);
        }
        finally
        {
            writer.Dispose();
        }

        writer.WrittenLength.ShouldBe(0);
        writer.Dispose();
    }

    [Test]
    public void ArrayPoolWriterInvalidLength()
    {
        Should.Throw<CsTomlException>(() =>
        {
            using var writer = new Utf8ArrayPoolWriter(16);
            writer.GetWrittenSpan(-1);
        });
        Should.Throw<CsTomlException>(() =>
        {
            using var writer = new Utf8ArrayPoolWriter(16);
            writer.Advance(writer.GetSpan().Length + 1);
        });
    }

    [Test]
    public void BufferWriter()
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        var writer = new Utf8BufferWriter(bufferWriter);
        WriteExpected(ref writer);
        writer.Flush();

        writer.WrittenLength.ShouldBe(Expected.Length);
        bufferWriter.WrittenSpan.ToArray().ShouldBe(Expected);
    }

    [Test]
    public void BufferWriterValueType()
    {
        var bufferWriter = new ValueBufferWriter(new byte[64]);
        var writer = new Utf8BufferWriter<ValueBufferWriter>(ref bufferWriter);
        WriteExpected(ref writer);
        writer.Flush();

        writer.WrittenLength.ShouldBe(Expected.Length);
        bufferWriter.WrittenSpan.ToArray().ShouldBe(Expected);
    }
}

internal struct ValueBufferWriter(byte[] buffer) : IBufferWriter<byte>
{
    private readonly byte[] scratch = new byte[256];
    private int written;

    public readonly ReadOnlySpan<byte> WrittenSpan => buffer.AsSpan(0, written);

    public void Advance(int count)
    {
        scratch.AsSpan(0, count).CopyTo(buffer.AsSpan(written));
        scratch.AsSpan().Clear();
        written += count;
    }

    public readonly Memory<byte> GetMemory(int sizeHint = 0) => scratch;

    public readonly Span<byte> GetSpan(int sizeHint = 0) => scratch;
}
