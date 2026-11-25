using CsToml.Utility;
using CsToml.Error;
using CsToml.Values;
using System.Globalization;
using System.Numerics;

namespace CsToml.Formatter;

internal sealed class Int128Formatter : ITomlValueFormatter<Int128>
{
    public static readonly Int128Formatter Instance = new Int128Formatter();

    public Int128 Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return (Int128)value;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(Int128), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, Int128 target, CsTomlSerializerOptions options)
    {
        writer.WriteInt64(IntegerRangeHelper.ConvertToInt64(target));
    }
}

internal sealed class NullableInt128Formatter : ITomlValueFormatter<Int128?>
{
    public static readonly NullableInt128Formatter Instance = new NullableInt128Formatter();

    public Int128? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return (Int128)value;
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(Int128), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, Int128? target, CsTomlSerializerOptions options)
    {
        if (target.HasValue)
        {
            writer.WriteInt64(IntegerRangeHelper.ConvertToInt64(target.GetValueOrDefault()));
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(Int128?));
        }
    }
}