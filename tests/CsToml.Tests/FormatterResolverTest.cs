using CsToml.Formatter;
using CsToml.Formatter.Resolver;
using System.Buffers;

namespace CsToml.Tests;

public class FormatterResolverTest
{
    private sealed class ObjectFormatter : ITomlValueFormatter<object>
    {
        public object Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
            => throw new NotSupportedException();

        public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, object target, CsTomlSerializerOptions options)
            where TBufferWriter : IBufferWriter<byte>
            => throw new NotSupportedException();
    }

    private sealed class IntFormatter : ITomlValueFormatter<int>
    {
        public int Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
            => throw new NotSupportedException();

        public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, int target, CsTomlSerializerOptions options)
            where TBufferWriter : IBufferWriter<byte>
            => throw new NotSupportedException();
    }

    [Fact]
    public void ObjectIsRegisteredAsBuiltin()
    {
        TomlValueFormatterResolver.IsRegistered<object>().ShouldBeTrue();
        TomlValueFormatterResolver.Instance.GetFormatter<object>().ShouldBeSameAs(PrimitiveObjectFormatter.Instance);
    }

    [Fact]
    public void RegisterDoesNotReplaceBuiltinFormatter()
    {
        TomlValueFormatterResolver.Register(new ObjectFormatter());
        TomlValueFormatterResolver.Instance.GetFormatter<object>().ShouldBeSameAs(PrimitiveObjectFormatter.Instance);

        TomlValueFormatterResolver.Register(new IntFormatter());
        TomlValueFormatterResolver.Instance.GetFormatter<int>().ShouldBeSameAs(Int32Formatter.Instance);
    }
}
