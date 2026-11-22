using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CsToml;

public static class CsTomlSerializer
{
    internal static readonly CsTomlSerializerOptions DefaultOptions = CsTomlSerializerOptions.Default with { SerializeOptions = new() };

    public static T Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options = null)
    {
        options ??= DefaultOptions;

        var document = new TomlDocument();
        var reader = new Utf8SequenceReader(tomlText, true);
        document.Parse(ref reader, options);
        if (typeof(T) == typeof(TomlDocument))
        {
            return Unsafe.As<TomlDocument, T>(ref document);
        }

        try
        {
            var rootNode = document.RootNode;
            var formatter = options.Resolver.GetFormatter<T>()!;
            using (formatter as IDisposable)
            {
                return formatter.Deserialize(ref rootNode, options);
            }
        }
        catch (CsTomlException cte)
        {
            throw new CsTomlSerializeException("An exception was thrown while deserializing TOML. See the 'ParseExceptions' property for details about any errors.", cte);
        }
    }

    public static T Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ReadOnlySequence<byte> tomlSequence, CsTomlSerializerOptions? options = null)
    {
        options ??= DefaultOptions;

        var document = new TomlDocument();
        var reader = new Utf8SequenceReader(tomlSequence, true);
        document.Parse(ref reader, options);
        if (typeof(T) == typeof(TomlDocument))
        {
            return Unsafe.As<TomlDocument, T>(ref document);
        }

        try
        {
            var rootNode = document.RootNode;
            var formatter = options.Resolver.GetFormatter<T>()!;
            using (formatter as IDisposable)
            {
                return formatter.Deserialize(ref rootNode, options);
            }
        }
        catch (CsTomlException cte)
        {
            throw new CsTomlSerializeException("An exception was thrown while deserializing TOML. See the 'ParseExceptions' property for details about any errors.", cte);
        }
    }

    public static T Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(Stream stream, CsTomlSerializerOptions? options = null)
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

    public static async ValueTask<T> DeserializeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(Stream stream, CsTomlSerializerOptions? options = null, bool configureAwait = false, CancellationToken cancellationToken = default)
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

    public static T DeserializeValueType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options = null)
    {
        options ??= DefaultOptions;
        var utf8SequenceReader = new Utf8SequenceReader(tomlText, true);
        var reader = new CsTomlReader(ref utf8SequenceReader, options);
        TomlValue tomlValue = reader.ReadValue(depth: 0);

        var tomlDocumentNode = new TomlDocumentNode(tomlValue);
        return options.Resolver.GetFormatter<T>()!.Deserialize(ref tomlDocumentNode, options);
    }

    public static T DeserializeValueType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ReadOnlySequence<byte> tomlSequence, CsTomlSerializerOptions? options = null)
    {
        options ??= DefaultOptions;
        var utf8SequenceReader = new Utf8SequenceReader(tomlSequence, true);
        var reader = new CsTomlReader(ref utf8SequenceReader, options);
        TomlValue tomlValue = reader.ReadValue(depth: 0);

        var tomlDocumentNode = new TomlDocumentNode(tomlValue);
        return options.Resolver.GetFormatter<T>()!.Deserialize(ref tomlDocumentNode, options);
    }

    public static ByteMemoryResult Serialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(T target, CsTomlSerializerOptions? options = null)
        => SerializeToResult(target, options, false);

    public static void Serialize<TBufferWriter, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ref TBufferWriter bufferWriter, T target, CsTomlSerializerOptions? options = null)
        where TBufferWriter : IBufferWriter<byte>
        => SerializeToBufferWriter(ref bufferWriter, target, options, false);

    [OverloadResolutionPriority(1)]
    public static void Serialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T, TWriter>(ref TWriter writer, T target, CsTomlSerializerOptions? options = null)
        where TWriter : IUtf8Writer, allows ref struct
        => SerializeToWriter(ref writer, target, options, false);

    public static void Serialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(Stream stream, T value, CsTomlSerializerOptions? options = null)
    {
        using var bufferWriter = new ByteBufferSegmentWriter();
        var tempWriter = bufferWriter;
        Serialize<ByteBufferSegmentWriter, T>(ref tempWriter, value, options);

        var streamByteWriter = new StreamByteWriter(stream);
        bufferWriter.WriteTo(streamByteWriter.ByteWriter);
    }

    public static async ValueTask SerializeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(Stream stream, T value, CsTomlSerializerOptions? options = null, bool configureAwait = false, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var bufferWriter = new ByteBufferSegmentWriter();
        var tempWriter = bufferWriter;
        Serialize<ByteBufferSegmentWriter, T>(ref tempWriter, value, options);

        var streamByteWriter = new StreamByteWriter(stream);
        await bufferWriter.WriteToAsync(streamByteWriter.ByteWriter, configureAwait, cancellationToken);
    }

    public static ByteMemoryResult SerializeValueType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(T target, CsTomlSerializerOptions? options = null)
        => SerializeToResult(target, options, true);

    public static void SerializeValueType<TBufferWriter, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ref TBufferWriter bufferWriter, T target, CsTomlSerializerOptions? options = null)
        where TBufferWriter : IBufferWriter<byte>
        => SerializeToBufferWriter(ref bufferWriter, target, options, true);

    [OverloadResolutionPriority(1)]
    public static void SerializeValueType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T, TWriter>(ref TWriter writer, T target, CsTomlSerializerOptions? options = null)
        where TWriter : IUtf8Writer, allows ref struct
        => SerializeToWriter(ref writer, target, options, true);

    private static ByteMemoryResult SerializeToResult<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(T target, CsTomlSerializerOptions? options, bool valueOnly)
    {
        options ??= DefaultOptions;

        InlineArray4<TomlDottedKey> initialKeys = default;
        ref InlineArray4<TomlDottedKey> initialKeysRef = ref Unsafe.AsRef(in initialKeys);
        TempList<TomlDottedKey> keyList = new(initialKeysRef);

        InlineArray4<(TomlValueState state, int dottedKeyIndex)> initialStates = default;
        ref InlineArray4<(TomlValueState state, int dottedKeyIndex)> initialStatesRef = ref Unsafe.AsRef(in initialStates);
        TempList<(TomlValueState state, int dottedKeyIndex)> stateList = new(initialStatesRef);

        var documentWriter = new Utf8TomlDocumentWriter<Utf8ArrayPoolWriter>(new Utf8ArrayPoolWriter(1024), in keyList, in stateList, valueOnly, options);
        try
        {
            SerializeCore(ref documentWriter, target, options);
            return ByteMemoryResult.Create(documentWriter.BufferWriter.WrittenSpan);
        }
        finally
        {
            documentWriter.BufferWriter.Dispose();
        }
    }

    private static void SerializeToBufferWriter<TBufferWriter, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ref TBufferWriter bufferWriter, T target, CsTomlSerializerOptions? options, bool valueOnly)
        where TBufferWriter : IBufferWriter<byte>
    {
        options ??= DefaultOptions;

        InlineArray4<TomlDottedKey> initialKeys = default;
        ref InlineArray4<TomlDottedKey> initialKeysRef = ref Unsafe.AsRef(in initialKeys);
        TempList<TomlDottedKey> keyList = new(initialKeysRef);

        InlineArray4<(TomlValueState state, int dottedKeyIndex)> initialStates = default;
        ref InlineArray4<(TomlValueState state, int dottedKeyIndex)> initialStatesRef = ref Unsafe.AsRef(in initialStates);
        TempList<(TomlValueState state, int dottedKeyIndex)> stateList = new(initialStatesRef);

        if (typeof(TBufferWriter).IsValueType)
        {
            var documentWriter = new Utf8TomlDocumentWriter<Utf8BufferWriter<TBufferWriter>>(new Utf8BufferWriter<TBufferWriter>(ref bufferWriter), in keyList, in stateList, valueOnly, options);
            try
            {
                SerializeCore(ref documentWriter, target, options);
            }
            finally
            {
                documentWriter.BufferWriter.Flush();
            }
        }
        else
        {
            var documentWriter = new Utf8TomlDocumentWriter<Utf8BufferWriter>(new Utf8BufferWriter(bufferWriter), in keyList, in stateList, valueOnly, options);
            try
            {
                SerializeCore(ref documentWriter, target, options);
            }
            finally
            {
                documentWriter.BufferWriter.Flush();
            }
        }
    }

    private static void SerializeToWriter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T, TWriter>(ref TWriter writer, T target, CsTomlSerializerOptions? options, bool valueOnly)
        where TWriter : IUtf8Writer, allows ref struct
    {
        options ??= DefaultOptions;

        InlineArray4<TomlDottedKey> initialKeys = default;
        ref InlineArray4<TomlDottedKey> initialKeysRef = ref Unsafe.AsRef(in initialKeys);
        TempList<TomlDottedKey> keyList = new(initialKeysRef);

        InlineArray4<(TomlValueState state, int dottedKeyIndex)> initialStates = default;
        ref InlineArray4<(TomlValueState state, int dottedKeyIndex)> initialStatesRef = ref Unsafe.AsRef(in initialStates);
        TempList<(TomlValueState state, int dottedKeyIndex)> stateList = new(initialStatesRef);

        var documentWriter = new Utf8TomlDocumentWriter<TWriter>(writer, in keyList, in stateList, valueOnly, options);
        try
        {
            SerializeCore(ref documentWriter, target, options);
        }
        finally
        {
            writer = documentWriter.BufferWriter;
        }
    }

    private static void SerializeCore<TBufferWriter, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ref Utf8TomlDocumentWriter<TBufferWriter> documentWriter, T target, CsTomlSerializerOptions options)
        where TBufferWriter : IUtf8Writer, allows ref struct
    {
        try
        {
            if (typeof(T) == typeof(TomlDocument))
            {
                Unsafe.As<T, TomlDocument>(ref target).ToTomlString(ref documentWriter);
                return;
            }

            var formatter = options.Resolver.GetFormatter<T>()!;
            using (formatter as IDisposable)
            {
                formatter.Serialize(ref documentWriter, target, options);
            }
        }
        catch (CsTomlException cte)
        {
            throw new CsTomlSerializeException("An exception was thrown while serializing TOML. See the 'ParseExceptions' property for details about any errors.", cte);
        }
    }
}

