using CsToml.Error;
using CsToml.Formatter.Resolver;
using CsToml.Utility;
using System.Buffers;

namespace CsToml;

public sealed class CsTomlSerializer
{
    private CsTomlSerializer() { }

    public static T Deserialize<T>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options = null)
        where T : ITomlSerializedObject<T>
    {
        options ??= CsTomlSerializerOptions.Default;

        var document = new TomlDocument();
        var reader = new Utf8SequenceReader(tomlText);
        document.Deserialize(ref reader, options);

        var rootNode = document.RootNode;
        return TomlValueFormatterResolver.GetFormatterForInternal<T>().Deserialize(ref rootNode, options);
    }

    public static T Deserialize<T>(ReadOnlySequence<byte> tomlSequence, CsTomlSerializerOptions? options = null)
        where T : ITomlSerializedObject<T>
    {
        options ??= CsTomlSerializerOptions.Default;

        var document = new TomlDocument();
        var reader = new Utf8SequenceReader(tomlSequence);
        document.Deserialize(ref reader, options);

        var rootNode = document.RootNode;
        return TomlValueFormatterResolver.GetFormatterForInternal<T>().Deserialize(ref rootNode, options);
    }


    public static ByteMemoryResult Serialize<T>(T target, CsTomlSerializerOptions? options = null)
        where T : ITomlSerializedObject<T>
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            Serialize(ref bufferWriter, target, options);
            return ByteMemoryResult.Create(bufferWriter);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }

    public static void Serialize<TBufferWriter, T>(ref TBufferWriter bufferWriter, T target, CsTomlSerializerOptions? options = null)
        where TBufferWriter : IBufferWriter<byte>
        where T : ITomlSerializedObject<T>
    {
        options ??= CsTomlSerializerOptions.Default;
        try
        {
            var documentWriter = new Utf8TomlDocumentWriter<TBufferWriter>(ref bufferWriter);

            TomlValueFormatterResolver.GetFormatterForInternal<T>().Serialize(ref documentWriter, target, options);
        }
        catch (CsTomlException cte)
        {
            throw new CsTomlSerializeException([cte]);
        }
    }
}

