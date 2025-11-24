
using CsToml.Error;
using CsToml.Values;
using System.Buffers;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CsToml.Formatter;

public sealed class ImmutableArrayFormatter<T> : ITomlValueFormatter<ImmutableArray<T>>, ITomlArrayHeaderFormatter<ImmutableArray<T>>
{
    public ImmutableArray<T> Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.CanGetValue(TomlValueFeature.Array) && rootNode.Value is TomlArray tomlArray)
        {
            var formatter = options.Resolver.GetFormatter<T>()!;
            var array = new T[tomlArray.Count];
            var arraySpan = array.AsSpan();
            for (int i = 0; i < arraySpan.Length; i++)
            {
                var arrayValueNode = rootNode[i];
                arraySpan[i] = formatter.Deserialize(ref arrayValueNode, options);
            }
            return ImmutableCollectionsMarshal.AsImmutableArray(array);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(ImmutableArray<T>));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ImmutableArray<T> target, CsTomlSerializerOptions options) where TBufferWriter : IBufferWriter<byte>
    {
        if (target == null)
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(ImmutableArray<T>));
            return;
        }

        var targetSpan = target.AsSpan();
        writer.BeginArray();
        if (targetSpan.Length == 0)
        {
            writer.EndArray();
            return;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        formatter.Serialize(ref writer, targetSpan[0], options);
        if (targetSpan.Length == 1)
        {
            writer.WriteSpace();
            writer.EndArray();
            return;
        }

        for (int i = 1; i < targetSpan.Length; i++)
        {
            writer.Write(TomlCodes.Symbol.COMMA);
            writer.WriteSpace();
            formatter.Serialize(ref writer, targetSpan[i], options);
        }
        writer.WriteSpace();
        writer.EndArray();
    }

    public bool TrySerialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, ImmutableArray<T> target, CsTomlSerializerOptions options) where TBufferWriter : IBufferWriter<byte>
    {
        if (target == null)
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(ImmutableArray<T>));
            return false; // not reached.
        }

        var targetSpan = target.AsSpan();
        if (targetSpan.Length == 0)
        {
            return false;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        for (int i = 0; i < targetSpan.Length; i++)
        {
            writer.BeginArrayOfTablesHeader();
            writer.WriteKey(header);
            writer.EndArrayOfTablesHeader();
            writer.WriteNewLine();
            writer.BeginCurrentState(TomlValueState.ArrayOfTableForMulitiLine);
            formatter.Serialize(ref writer, targetSpan[i], options);
            writer.EndCurrentState();
            writer.EndKeyValue(false);
        }

        return true;
    }
}
