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
    }

    private sealed class KeyHolderFormatter : ITomlValueFormatter<KeyHolder>
    {
        public KeyHolder Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
            => throw new NotSupportedException();

        public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, KeyHolder target, CsTomlSerializerOptions options)
            where TBufferWriter : IBufferWriter<byte>
        {
            writer.PushBareKey(target.Key);
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

    private static string Serialize(ImmutableArray<byte> key)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        CsTomlSerializer.Serialize(ref bufferWriter, new KeyHolder() { Key = key });
        return Encoding.UTF8.GetString(bufferWriter.WrittenSpan);
    }

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
}
