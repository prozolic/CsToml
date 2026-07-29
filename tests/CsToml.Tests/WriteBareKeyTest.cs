using CsToml.Error;
using CsToml.Formatter;
using CsToml.Formatter.Resolver;
using System.Buffers;
using System.Collections.Immutable;

namespace CsToml.Tests;

public class WriteBareKeyTest
{
    public enum WriteMode
    {
        BareKey,
        BareKeyValidated,
        BareKeySkipValidation,
        BareTableHeader,
        BareTableHeaderValidated,
        BareTableHeaderSkipValidation,
    }

    public sealed class BareKeyHolder
    {
        public ImmutableArray<byte> Key { get; init; }

        public WriteMode Mode { get; init; }
    }

    private sealed class BareKeyHolderFormatter : ITomlValueFormatter<BareKeyHolder>
    {
        public BareKeyHolder Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
            => throw new NotSupportedException();

        public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, BareKeyHolder target, CsTomlSerializerOptions options)
            where TBufferWriter : IBufferWriter<byte>
        {
            var key = target.Key.AsSpan();
            switch (target.Mode)
            {
                case WriteMode.BareKey:
                    writer.WriteBareKey(key);
                    break;
                case WriteMode.BareKeyValidated:
                    writer.WriteBareKey(key, false);
                    break;
                case WriteMode.BareKeySkipValidation:
                    writer.WriteBareKey(key, true);
                    break;
                case WriteMode.BareTableHeader:
                    writer.WriteBareTableHeader(key);
                    return;
                case WriteMode.BareTableHeaderValidated:
                    writer.WriteBareTableHeader(key, false);
                    return;
                case WriteMode.BareTableHeaderSkipValidation:
                    writer.WriteBareTableHeader(key, true);
                    return;
            }

            writer.WriteEqual();
            writer.WriteInt64(1);
            writer.EndKeyValue(true);
        }
    }

    static WriteBareKeyTest()
    {
        TomlValueFormatterResolver.Register(new BareKeyHolderFormatter());
    }

    // 0xC3 is the lead byte of a 2-byte sequence, but the continuation byte is missing.
    private static readonly ImmutableArray<byte> InvalidUtf8Key = ImmutableArray.Create<byte>(0xC3);

    private static byte[] SerializeToUtf8(ImmutableArray<byte> key, WriteMode mode)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        CsTomlSerializer.Serialize(ref bufferWriter, new BareKeyHolder() { Key = key, Mode = mode });
        return bufferWriter.WrittenSpan.ToArray();
    }

    [Theory]
    [InlineData(WriteMode.BareKey)]
    [InlineData(WriteMode.BareKeyValidated)]
    [InlineData(WriteMode.BareTableHeader)]
    [InlineData(WriteMode.BareTableHeaderValidated)]
    public void ThrowIfKeyIsInvalidUtf8SequenceWhenValidationIsNotSkipped(WriteMode mode)
    {
        Should.Throw<CsTomlSerializeException>(() => SerializeToUtf8(InvalidUtf8Key, mode));
    }

    [Fact]
    public void WriteKeyAsIsIfValidationIsSkipped()
    {
        var written = SerializeToUtf8(InvalidUtf8Key, WriteMode.BareKeySkipValidation);

        written.AsSpan(0, 5).ToArray().ShouldBe([0xC3, (byte)' ', (byte)'=', (byte)' ', (byte)'1']);
    }

    [Fact]
    public void WriteTableHeaderAsIsIfValidationIsSkipped()
    {
        var written = SerializeToUtf8(InvalidUtf8Key, WriteMode.BareTableHeaderSkipValidation);

        written.ShouldBe([(byte)'[', 0xC3, (byte)']']);
    }

    [Theory]
    [InlineData(WriteMode.BareTableHeader)]
    [InlineData(WriteMode.BareTableHeaderValidated)]
    [InlineData(WriteMode.BareTableHeaderSkipValidation)]
    public void WriteTableHeaderWhenKeyIsValid(WriteMode mode)
    {
        var written = SerializeToUtf8(ImmutableArray.Create("table"u8), mode);

        written.ShouldBe("[table]"u8.ToArray());
    }

    [Theory]
    [InlineData(WriteMode.BareKey)]
    [InlineData(WriteMode.BareKeyValidated)]
    [InlineData(WriteMode.BareKeySkipValidation)]
    public void WriteKeyWhenKeyIsValid(WriteMode mode)
    {
        var written = SerializeToUtf8(ImmutableArray.Create("key"u8), mode);

        written.AsSpan(0, 7).ToArray().ShouldBe("key = 1"u8.ToArray());
    }
}
