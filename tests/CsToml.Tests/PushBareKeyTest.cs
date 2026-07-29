using CsToml.Error;
using CsToml.Formatter;
using CsToml.Formatter.Resolver;
using System.Buffers;
using System.Collections.Immutable;
using System.Text;

namespace CsToml.Tests;

public class PushBareKeyTest
{
    public sealed class KeyHolder
    {
        public ImmutableArray<byte> Key { get; init; }

        public bool SkipValidation { get; init; }

        public bool PushAsSpan { get; init; }
    }

    private sealed class KeyHolderFormatter : ITomlValueFormatter<KeyHolder>
    {
        public KeyHolder Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
            => throw new NotSupportedException();

        public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, KeyHolder target, CsTomlSerializerOptions options)
            where TBufferWriter : IBufferWriter<byte>
        {
            if (target.PushAsSpan)
            {
                writer.PushBareKey(target.Key.AsSpan(), target.SkipValidation);
            }
            else
            {
                writer.PushBareKey(target.Key, target.SkipValidation);
            }
            writer.WriteBareKey("value"u8);
            writer.WriteEqual();
            writer.WriteInt64(1);
            writer.EndKeyValue(true);
            writer.PopKey();
        }
    }

    static PushBareKeyTest()
    {
        TomlValueFormatterResolver.Register(new KeyHolderFormatter());
    }

    private static byte[] SerializeToUtf8(ImmutableArray<byte> key, bool skipValidation = false, bool pushAsSpan = false)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        CsTomlSerializer.Serialize(ref bufferWriter, new KeyHolder() { Key = key, SkipValidation = skipValidation, PushAsSpan = pushAsSpan });
        return bufferWriter.WrittenSpan.ToArray();
    }

    private static string Serialize(ImmutableArray<byte> key)
        => Encoding.UTF8.GetString(SerializeToUtf8(key));

    [Fact]
    public void PushedKeyIsUsedAsDottedKeyPrefix()
    {
        Serialize(ImmutableArray.Create("table"u8)).ShouldStartWith("table.value = 1");
    }

    [Fact]
    public void ThrowIfImmutableArrayIsUninitialized()
    {
        Should.Throw<CsTomlSerializeException>(() => Serialize(default));
    }

    [Fact]
    public void ThrowIfKeyIsInvalidUtf8SequenceWhenValidationIsNotSkipped()
    {
        // 0xC3 is the lead byte of a 2-byte sequence, but the continuation byte is missing.
        var invalidKey = ImmutableArray.Create<byte>(0xC3);

        Should.Throw<CsTomlSerializeException>(() => SerializeToUtf8(invalidKey));
        Should.Throw<CsTomlSerializeException>(() => SerializeToUtf8(invalidKey, pushAsSpan: true));
    }

    [Fact]
    public void WriteKeyAsIsIfValidationIsSkipped()
    {
        var invalidKey = ImmutableArray.Create<byte>(0xC3);
        var expected = new byte[] { 0xC3, (byte)'.', (byte)'v', (byte)'a', (byte)'l', (byte)'u', (byte)'e', (byte)' ', (byte)'=', (byte)' ', (byte)'1' };

        SerializeToUtf8(invalidKey, skipValidation: true).AsSpan(0, expected.Length).ToArray().ShouldBe(expected);
        SerializeToUtf8(invalidKey, skipValidation: true, pushAsSpan: true).AsSpan(0, expected.Length).ToArray().ShouldBe(expected);
    }
}
