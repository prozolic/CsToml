#if NET11_0_OR_GREATER

using CsToml.Error;
using System.Numerics;

namespace CsToml.Formatter;

internal sealed class BFloat16Formatter : ITomlValueFormatter<BFloat16>
{
    public static readonly BFloat16Formatter Instance = new BFloat16Formatter();

    public BFloat16 Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetDouble(out var value))
        {
            var result = (BFloat16)value;
            if (BFloat16.IsInfinity(result) && double.IsFinite(value))
            {
                ExceptionHelper.ThrowOverflow(typeof(BFloat16));
            }
            return result;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(BFloat16), rootNode);
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, BFloat16 target, CsTomlSerializerOptions options)
        where TBufferWriter : IUtf8Writer, allows ref struct
    {
        writer.WriteDouble((double)target);
    }
}

internal sealed class NullableBFloat16Formatter : ITomlValueFormatter<BFloat16?>
{
    public static readonly NullableBFloat16Formatter Instance = new NullableBFloat16Formatter();

    public BFloat16? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.TryGetDouble(out var value))
        {
            var result = (BFloat16)value;
            if (BFloat16.IsInfinity(result) && double.IsFinite(value))
            {
                ExceptionHelper.ThrowOverflow(typeof(BFloat16));
            }
            return result;
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(BFloat16), rootNode);
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, BFloat16? target, CsTomlSerializerOptions options)
        where TBufferWriter : IUtf8Writer, allows ref struct
    {
        if (target.HasValue)
        {
            writer.WriteDouble((double)target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(BFloat16?));
        }
    }
}

#endif
