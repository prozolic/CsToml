using CsToml.Utility;
using CsToml.Error;

namespace CsToml.Formatter;

internal sealed class UInt16Formatter : ITomlValueFormatter<ushort>
{
    public static readonly UInt16Formatter Instance = new UInt16Formatter();

    public ushort Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return IntegerRangeHelper.ConvertFromInt64<ushort>(value);
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(ushort), rootNode);
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ushort target, CsTomlSerializerOptions options)
        where TBufferWriter : IUtf8Writer, allows ref struct
    {
        writer.WriteInt64(target);
    }
}

internal sealed class NullableUInt16Formatter : ITomlValueFormatter<ushort?>
{
    public static readonly NullableUInt16Formatter Instance = new NullableUInt16Formatter();

    public ushort? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return IntegerRangeHelper.ConvertFromInt64<ushort>(value);
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(ushort), rootNode);
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ushort? target, CsTomlSerializerOptions options)
        where TBufferWriter : IUtf8Writer, allows ref struct
    {
        if (target.HasValue)
        {
            writer.WriteInt64(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(ushort?));
        }
    }
}
