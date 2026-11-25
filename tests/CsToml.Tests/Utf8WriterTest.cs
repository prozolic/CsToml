using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace CsToml.Tests;

public class Utf8WriterTest
{
    private static readonly byte[] Expected = "key = \"value\"\n"u8.ToArray();

    private static void WriteExpected(ref Utf8TomlDocumentWriter writer)
    {
        "key"u8.CopyTo(writer.GetSpan(3));
        writer.Advance(3);
        " = "u8.CopyTo(writer.GetWrittenSpan(3));
        writer.WriteBytes("\"value\""u8);
        writer.Write((byte)'\n');
    }

    [Test]
    public void FixedSpan()
    {
        var destination = new byte[Expected.Length];
        long writtenSize = 0;
        var throwsWhenFull = false;
        DocumentWriterHelper.WithFixedSpan(destination, false, (ref Utf8TomlDocumentWriter writer) =>
        {
            WriteExpected(ref writer);
            writtenSize = writer.WrittenSize;
            writer.WrittenSpan.ToArray().ShouldBe(Expected);
            try
            {
                writer.GetSpan(0);
            }
            catch (CsTomlException)
            {
                throwsWhenFull = true;
            }
        });

        writtenSize.ShouldBe(Expected.Length);
        destination.ShouldBe(Expected);
        throwsWhenFull.ShouldBeTrue();
    }

    [Test]
    public void FixedSpanTooSmall()
    {
        var operations = new DocumentWriterHelper.WriterAction[]
        {
            (ref Utf8TomlDocumentWriter writer) => writer.GetWrittenSpan(3),
            (ref Utf8TomlDocumentWriter writer) => writer.GetSpan(3),
            (ref Utf8TomlDocumentWriter writer) => writer.Advance(3),
            (ref Utf8TomlDocumentWriter writer) => writer.WriteBytes("abc"u8),
            (ref Utf8TomlDocumentWriter writer) =>
            {
                writer.WriteBytes("ab"u8);
                writer.Write((byte)'c');
            },
        };

        foreach (var operation in operations)
        {
            Should.Throw<CsTomlException>(() => DocumentWriterHelper.WithFixedSpan(new byte[2], false, operation));
        }
    }

    [Test]
    public void ArrayPoolGrow()
    {
        var expected = new byte[5000];
        Random.Shared.NextBytes(expected);

        long writtenSize = 0;
        long writtenSizeAfterDispose = -1;
        DocumentWriterHelper.WithArrayPool(16, false, (ref Utf8TomlDocumentWriter writer) =>
        {
            try
            {
                var first = writer.GetSpan(0).Length;
                expected.AsSpan(0, first).CopyTo(writer.GetWrittenSpan(first));
                writer.GetSpan(0).Length.ShouldBeGreaterThan(0);
                expected.AsSpan(first, 990).CopyTo(writer.GetSpan(990));
                writer.Advance(990);
                writer.Write(expected[first + 990]);
                writer.WriteBytes(expected.AsSpan(first + 991, 1000));
                expected.AsSpan(first + 1991).CopyTo(writer.GetWrittenSpan(expected.Length - first - 1991));

                writtenSize = writer.WrittenSize;
                writer.WrittenSpan.ToArray().ShouldBe(expected);
            }
            finally
            {
                writer.Dispose();
            }

            writtenSizeAfterDispose = writer.WrittenSize;
            writer.Dispose();
        });

        writtenSize.ShouldBe(expected.Length);
        writtenSizeAfterDispose.ShouldBe(0);
    }

    [Test]
    public void ArrayPoolInvalidLength()
    {
        Should.Throw<CsTomlException>(() => DocumentWriterHelper.WithArrayPool(16, false, (ref Utf8TomlDocumentWriter writer) =>
        {
            try
            {
                writer.GetWrittenSpan(-1);
            }
            finally
            {
                writer.Dispose();
            }
        }));
        Should.Throw<CsTomlException>(() => DocumentWriterHelper.WithArrayPool(16, false, (ref Utf8TomlDocumentWriter writer) =>
        {
            try
            {
                writer.Advance(writer.GetSpan(0).Length + 1);
            }
            finally
            {
                writer.Dispose();
            }
        }));
    }

    [Test]
    public void BufferWriter()
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        var writtenSize = DocumentWriterHelper.WithBufferWriter(bufferWriter, WriteExpected);

        writtenSize.ShouldBe(Expected.Length);
        bufferWriter.WrittenSpan.ToArray().ShouldBe(Expected);
    }

    [Test]
    public void BufferWriterValueType()
    {
        IBufferWriter<byte> boxed = new ValueBufferWriter(new byte[64]);
        var writtenSize = DocumentWriterHelper.WithBufferWriter(boxed, WriteExpected);

        writtenSize.ShouldBe(Expected.Length);
        ((ValueBufferWriter)boxed).WrittenSpan.ToArray().ShouldBe(Expected);
    }

    [Test]
    public void BufferWriterStrict()
    {
        var expected = new byte[300];
        Random.Shared.NextBytes(expected);

        var bufferWriter = new StrictBufferWriter(3);
        var writtenSize = DocumentWriterHelper.WithBufferWriter(bufferWriter, (ref Utf8TomlDocumentWriter writer) =>
        {
            writer.WrittenSize.ShouldBe(0);
            for (var i = 0; i < 100; i++)
            {
                writer.Write(expected[i]);
            }
            writer.WrittenSize.ShouldBe(100);
            writer.WriteBytes(expected.AsSpan(100, 50));
            expected.AsSpan(150, 50).CopyTo(writer.GetWrittenSpan(50));
            var span = writer.GetSpan(70);
            span.Length.ShouldBeGreaterThanOrEqualTo(70);
            expected.AsSpan(200, 70).CopyTo(span);
            writer.Advance(70);
            writer.WriteBytes(expected.AsSpan(270));
            writer.WrittenSize.ShouldBe(expected.Length);
        });

        writtenSize.ShouldBe(expected.Length);
        bufferWriter.Verify();
        bufferWriter.WrittenSpan.ToArray().ShouldBe(expected);
    }

    [Test]
    public void BufferWriterFlushesWithoutWriting()
    {
        var bufferWriter = new StrictBufferWriter(3);
        var writtenSize = DocumentWriterHelper.WithBufferWriter(bufferWriter, (ref Utf8TomlDocumentWriter writer) => { });

        writtenSize.ShouldBe(0);
        bufferWriter.Verify();
        bufferWriter.GetSpanCount.ShouldBe(0);
        bufferWriter.WrittenSpan.Length.ShouldBe(0);
    }
}

internal static class DocumentWriterHelper
{
    public delegate void WriterAction(ref Utf8TomlDocumentWriter writer);

    public static void WithFixedSpan(Span<byte> destination, bool valueOnly, WriterAction action)
    {
        InlineArray4<TomlDottedKey> initialKeys = default;
        ref InlineArray4<TomlDottedKey> initialKeysRef = ref Unsafe.AsRef(in initialKeys);
        TempList<TomlDottedKey> keyList = new(initialKeysRef);

        InlineArray4<(TomlValueState state, int dottedKeyIndex)> initialStates = default;
        ref InlineArray4<(TomlValueState state, int dottedKeyIndex)> initialStatesRef = ref Unsafe.AsRef(in initialStates);
        TempList<(TomlValueState state, int dottedKeyIndex)> stateList = new(initialStatesRef);

        var writer = new Utf8TomlDocumentWriter(destination, in keyList, in stateList, valueOnly, CsTomlSerializer.DefaultOptions);
        action(ref writer);
    }

    public static void WithArrayPool(int initialCapacity, bool valueOnly, WriterAction action)
    {
        InlineArray4<TomlDottedKey> initialKeys = default;
        ref InlineArray4<TomlDottedKey> initialKeysRef = ref Unsafe.AsRef(in initialKeys);
        TempList<TomlDottedKey> keyList = new(initialKeysRef);

        InlineArray4<(TomlValueState state, int dottedKeyIndex)> initialStates = default;
        ref InlineArray4<(TomlValueState state, int dottedKeyIndex)> initialStatesRef = ref Unsafe.AsRef(in initialStates);
        TempList<(TomlValueState state, int dottedKeyIndex)> stateList = new(initialStatesRef);

        var writer = new Utf8TomlDocumentWriter(new ArrayPoolBufferSource(initialCapacity), in keyList, in stateList, valueOnly, CsTomlSerializer.DefaultOptions);
        action(ref writer);
    }

    public static long WithBufferWriter(IBufferWriter<byte> bufferWriter, WriterAction action)
    {
        InlineArray4<TomlDottedKey> initialKeys = default;
        ref InlineArray4<TomlDottedKey> initialKeysRef = ref Unsafe.AsRef(in initialKeys);
        TempList<TomlDottedKey> keyList = new(initialKeysRef);

        InlineArray4<(TomlValueState state, int dottedKeyIndex)> initialStates = default;
        ref InlineArray4<(TomlValueState state, int dottedKeyIndex)> initialStatesRef = ref Unsafe.AsRef(in initialStates);
        TempList<(TomlValueState state, int dottedKeyIndex)> stateList = new(initialStatesRef);

        var writer = new Utf8TomlDocumentWriter(new WriterBufferSource(bufferWriter), in keyList, in stateList, false, CsTomlSerializer.DefaultOptions);
        try
        {
            action(ref writer);
        }
        finally
        {
            writer.Flush();
        }
        return writer.WrittenSize;
    }

    public static long SerializeToFixedSpan<T>(Span<byte> destination, T value, bool valueOnly)
    {
        long writtenSize = 0;
        WithFixedSpan(destination, valueOnly, (ref Utf8TomlDocumentWriter writer) =>
        {
            try
            {
                if (value is TomlDocument document)
                {
                    document.ToTomlString(ref writer);
                }
                else
                {
                    CsTomlSerializer.DefaultOptions.Resolver.GetFormatter<T>()!.Serialize(ref writer, value, CsTomlSerializer.DefaultOptions);
                }
            }
            finally
            {
                writtenSize = writer.WrittenSize;
            }
        });
        return writtenSize;
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

internal sealed class StrictBufferWriter(int minimumSpanLength) : IBufferWriter<byte>
{
    private const byte Poison = 0xCC;

    private readonly ArrayBufferWriter<byte> output = new();
    private readonly List<byte[]> released = [];
    private byte[]? current;
    private bool lastCallWasGetSpan;

    public ReadOnlySpan<byte> WrittenSpan => output.WrittenSpan;

    public int GetSpanCount { get; private set; }

    public void Advance(int count)
    {
        VerifyReleased();
        if (current is null)
        {
            throw new InvalidOperationException("Advance was called without a preceding GetSpan.");
        }
        if ((uint)count > (uint)current.Length)
        {
            throw new InvalidOperationException("Advance was called past the end of the span.");
        }

        output.Write(current.AsSpan(0, count));
        current.AsSpan().Fill(Poison);
        released.Add(current);
        current = null;
        lastCallWasGetSpan = false;
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
        => throw new NotSupportedException();

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        VerifyReleased();
        if (current is not null)
        {
            current.AsSpan().Fill(Poison);
            released.Add(current);
        }

        GetSpanCount++;
        current = new byte[Math.Max(sizeHint, minimumSpanLength)];
        lastCallWasGetSpan = true;
        return current;
    }

    public void Verify()
    {
        VerifyReleased();
        if (lastCallWasGetSpan)
        {
            throw new InvalidOperationException("GetSpan was called after the last Advance.");
        }
    }

    private void VerifyReleased()
    {
        foreach (var span in released)
        {
            if (span.AsSpan().ContainsAnyExcept(Poison))
            {
                throw new InvalidOperationException("A span was written after Advance or a later GetSpan.");
            }
        }
    }
}
