﻿using CsToml.Error;
using CsToml.Formatter;
using CsToml.Formatter.Resolver;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace CsToml;

public static class CsTomlSerializer
{
    private static readonly CsTomlSerializerOptions DefaultOptions = CsTomlSerializerOptions.Default with { SerializeOptions = new ()};

    [ThreadStatic]
    private static TomlDocumentFormatter? DocumentFormatter;

    private sealed class TomlDocumentFormatter : ITomlValueFormatter<TomlDocument>, IDisposable
    {
        private TomlDocument? tomlDocument;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    {
        options ??= DefaultOptions;

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
    {
        options ??= DefaultOptions;

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

    public static T Deserialize<T>(Stream stream, CsTomlSerializerOptions? options = null)
    {
        if (stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var arraySegment))
        {
            memoryStream.Seek(arraySegment.Count, SeekOrigin.Current);
            return CsTomlSerializer.Deserialize<T>(arraySegment.Array, options);
        }

        using ByteBufferSegmentWriter bufferWriter = new ByteBufferSegmentWriter();
        while (true)
        {
            int length = stream.Read(bufferWriter.GetSpan(65536));
            if (length == 0)
            {
                break;
            }
            bufferWriter.Advance(length);
        }
        return Deserialize<T>(bufferWriter.CreateReadOnlySequence(), options);
    }

    public static async ValueTask<T> DeserializeAsync<T>(Stream stream, CsTomlSerializerOptions? options = null, bool configureAwait = false, CancellationToken cancellationToken = default)
    {
        if (stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var arraySegment))
        {
            memoryStream.Seek(arraySegment.Count, SeekOrigin.Current);
            return CsTomlSerializer.Deserialize<T>(arraySegment.Array, options);
        }

        using ByteBufferSegmentWriter bufferWriter = new ByteBufferSegmentWriter();
        while (true)
        {
            int length = await stream.ReadAsync(bufferWriter.GetMemory(65536), cancellationToken: cancellationToken).ConfigureAwait(configureAwait);
            if (length == 0)
            {
                break;
            }
            bufferWriter.Advance(length);
        }
        return Deserialize<T>(bufferWriter.CreateReadOnlySequence(), options);
    }

    public static T DeserializeValueType<T>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options = null)
    {
        options ??= DefaultOptions;
        var utf8SequenceReader = new Utf8SequenceReader(tomlText);
        var reader = new CsTomlReader(ref utf8SequenceReader);
        TomlValue tomlValue = reader.ReadValue();

        var tomlDocumentNode = new TomlDocumentNode(tomlValue);
        return options.Resolver.GetFormatter<T>()!.Deserialize(ref tomlDocumentNode, options);
    }

    public static T DeserializeValueType<T>(ReadOnlySequence<byte> tomlSequence, CsTomlSerializerOptions? options = null)
    {
        options ??= DefaultOptions;
        var utf8SequenceReader = new Utf8SequenceReader(tomlSequence);
        var reader = new CsTomlReader(ref utf8SequenceReader);
        TomlValue tomlValue = reader.ReadValue();

        var tomlDocumentNode = new TomlDocumentNode(tomlValue);
        return options.Resolver.GetFormatter<T>()!.Deserialize(ref tomlDocumentNode, options);
    }

    public static ByteMemoryResult Serialize<T>(T target, CsTomlSerializerOptions? options = null)
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

    public static void Serialize<TBufferWriter, T>(ref TBufferWriter bufferWriter, T? target, CsTomlSerializerOptions? options = null)
        where TBufferWriter : IBufferWriter<byte>
    {
        options ??= DefaultOptions;
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

    public static void Serialize<T>(Stream stream, T? value, CsTomlSerializerOptions? options = null)
    {
        using var bufferWriter = new ByteBufferSegmentWriter();
        var tempWriter = bufferWriter;
        Serialize<ByteBufferSegmentWriter, T>(ref tempWriter, value, options);

        var streamByteWriter = new StreamByteWriter(stream);
        bufferWriter.WriteTo(streamByteWriter.ByteWriter);
    }

    public static async ValueTask SerializeAsync<T>(Stream stream, T? value, CsTomlSerializerOptions? options = null, bool configureAwait = false, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var bufferWriter = new ByteBufferSegmentWriter();
        var tempWriter = bufferWriter;
        Serialize<ByteBufferSegmentWriter, T>(ref tempWriter, value, options);

        var streamByteWriter = new StreamByteWriter(stream);
        await bufferWriter.WriteToAsync(streamByteWriter.ByteWriter, configureAwait, cancellationToken);
    }

    public static ByteMemoryResult SerializeValueType<T>(T target, CsTomlSerializerOptions? options = null)
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            SerializeValueType(ref bufferWriter, target, options);
            return ByteMemoryResult.Create(bufferWriter);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }

    public static void SerializeValueType<TBufferWriter, T>(ref TBufferWriter bufferWriter, T target, CsTomlSerializerOptions? options = null)
        where TBufferWriter : IBufferWriter<byte>
    {
        options ??= DefaultOptions;
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

