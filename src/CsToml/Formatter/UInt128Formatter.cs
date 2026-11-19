using CsToml.Utility;
using CsToml.Error;
using CsToml.Values;
using System.Buffers;
using System.Globalization;

namespace CsToml.Formatter;

internal sealed class UInt128Formatter : ITomlValueFormatter<UInt128>
{
    public static readonly UInt128Formatter Instance = new UInt128Formatter();

    public UInt128 Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return IntegerRangeHelper.ConvertFromInt64<UInt128>(value);
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(UInt128), rootNode);
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, UInt128 target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteInt64(IntegerRangeHelper.ConvertToInt64(target));
    }
}

internal sealed class NullableUInt128Formatter : ITomlValueFormatter<UInt128?>
{
    public static readonly NullableUInt128Formatter Instance = new NullableUInt128Formatter();

    public UInt128? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return IntegerRangeHelper.ConvertFromInt64<UInt128>(value);
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(UInt128), rootNode);
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, UInt128? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteInt64(IntegerRangeHelper.ConvertToInt64(target.GetValueOrDefault()));
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(UInt128?));
        }
    }
}
