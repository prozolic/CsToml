using CsToml.Error;
using CsToml.Formatter;
using CsToml.Formatter.Resolver;
using CsToml.Utility;
using System.Buffers;

namespace CsToml;

public static class CsTomlSerializer
{
    [ThreadStatic]
    private static TomlDocumentFormatter? DocumentFormatter;

    private sealed class TomlDocumentFormatter : ITomlValueFormatter<TomlDocument>, IDisposable
    {
        private TomlDocument? tomlDocument;

        public void SetTomlDocument(TomlDocument? tomlDocument)
        {
            this.tomlDocument = tomlDocument;
        }

        public TomlDocument Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
        {
            return this.tomlDocument!;
        }

        public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TomlDocument target, CsTomlSerializerOptions options) where TBufferWriter : IBufferWriter<byte>
        {
            try
            {
                target!.ToTomlString(ref writer);
            }
            catch (CsTomlException cte)
            {
                throw new CsTomlSerializeException("An error occurred when serializing the TOML file. Check InnerException for exception information.", cte);
            }
        }

        public void Dispose()
        {
            tomlDocument = null;
        }
    }

    private static ITomlValueFormatter<T> GetFormatter<T>(TomlDocument? tomlDocument)
    {
        if (typeof(T) == typeof(TomlDocument))
        {
            var formatter = DocumentFormatter ??= new TomlDocumentFormatter();
            formatter.SetTomlDocument(tomlDocument);
            return (formatter as ITomlValueFormatter<T>)!;
        }
        return TomlValueFormatterResolver.Instance.GetFormatter<T>()!;
    }

    public static T Deserialize<T>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options = null)
        where T : ITomlSerializedObject<T>
    {
        options ??= CsTomlSerializerOptions.Default;

        var document = new TomlDocument();
        var reader = new Utf8SequenceReader(tomlText);
        document.Deserialize(ref reader, options);

        try
        {
            var rootNode = document.RootNode;
            var formatter = GetFormatter<T>(document);
            using (formatter as IDisposable)
            {
                return formatter.Deserialize(ref rootNode, options);
            }
        }
        catch (CsTomlException cte)
        {
            throw new CsTomlSerializeException("An error occurred when serializing the TOML file. Check InnerException for exception information.", cte);
        }
    }

    public static T Deserialize<T>(ReadOnlySequence<byte> tomlSequence, CsTomlSerializerOptions? options = null)
        where T : ITomlSerializedObject<T>
    {
        options ??= CsTomlSerializerOptions.Default;

        var document = new TomlDocument();
        var reader = new Utf8SequenceReader(tomlSequence);
        document.Deserialize(ref reader, options);

        try
        {
            var rootNode = document.RootNode;
            var formatter = GetFormatter<T>(document);
            using (formatter as IDisposable)
            {
                return formatter.Deserialize(ref rootNode, options);
            }
        }
        catch (CsTomlException cte)
        {
            throw new CsTomlSerializeException("An error occurred when serializing the TOML file. Check InnerException for exception information.", cte);
        }
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
            var formatter = GetFormatter<T>(null);
            using (formatter as IDisposable)
            {
                formatter.Serialize(ref documentWriter, target, options);
            }
        }
        catch (CsTomlException cte)
        {
            throw new CsTomlSerializeException("An error occurred when serializing the TOML file. Check InnerException for exception information.", cte);
        }
    }
}

