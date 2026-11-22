using CsToml.Utility;
using CsToml.Error;

namespace CsToml.Formatter;

internal sealed class UInt64Formatter : ITomlValueFormatter<ulong>
{
    public static readonly UInt64Formatter Instance = new UInt64Formatter();

    public ulong Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return IntegerRangeHelper.ConvertFromInt64<ulong>(value);
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(ulong), rootNode);
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ulong target, CsTomlSerializerOptions options)
        where TBufferWriter : IUtf8Writer, allows ref struct
    {
        writer.WriteInt64(IntegerRangeHelper.ConvertToInt64(target));
    }
}

internal sealed class NullableUInt64Formatter : ITomlValueFormatter<ulong?>
{
    public static readonly NullableUInt64Formatter Instance = new NullableUInt64Formatter();

    public ulong? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return IntegerRangeHelper.ConvertFromInt64<ulong>(value);
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(ulong), rootNode);
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ulong? target, CsTomlSerializerOptions options)
        where TBufferWriter : IUtf8Writer, allows ref struct
    {
        if (target.HasValue)
        {
            writer.WriteInt64(IntegerRangeHelper.ConvertToInt64(target.GetValueOrDefault()));
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(ulong?));
        }
    }
}
