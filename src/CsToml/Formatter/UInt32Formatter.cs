using CsToml.Utility;
using CsToml.Error;

namespace CsToml.Formatter;

internal sealed class UInt32Formatter : ITomlValueFormatter<uint>
{
    public static readonly UInt32Formatter Instance = new UInt32Formatter();

    public uint Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return IntegerRangeHelper.ConvertFromInt64<uint>(value);
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(uint), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, uint target, CsTomlSerializerOptions options)
    {
        writer.WriteInt64(target);
    }
}

internal sealed class NullableUInt32Formatter : ITomlValueFormatter<uint?>
{
    public static readonly NullableUInt32Formatter Instance = new NullableUInt32Formatter();

    public uint? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return IntegerRangeHelper.ConvertFromInt64<uint>(value);
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(uint), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, uint? target, CsTomlSerializerOptions options)
    {
        if (target.HasValue)
        {
            writer.WriteInt64(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(uint?));
        }
    }
}
