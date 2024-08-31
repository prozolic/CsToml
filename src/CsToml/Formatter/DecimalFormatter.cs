using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Buffers.Text;
using System.Globalization;

namespace CsToml.Formatter;

internal sealed class DecimalFormatter : ITomlValueFormatter<decimal>
{
    public static readonly DecimalFormatter Instance = new DecimalFormatter();

    public decimal Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        var tomlValue = rootNode.Value;
        if (tomlValue is TomlInteger)
        {
            return tomlValue.GetInt64();
        }
        else
        {
            var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();

            try
            {
                var length = 128;
                var bytesWritten = 0;
                while (!tomlValue.TryFormat(bufferWriter.GetSpan(length), out bytesWritten))
                {
                    length *= 2;
                }
                bufferWriter.Advance(bytesWritten);

                if (Utf8Parser.TryParse(bufferWriter.WrittenSpan, out decimal value, out int bytesConsumed, '\0'))
                {
                    return value;
                }
            }
            finally
            {
                RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
            }
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(decimal));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, decimal target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            var length = 64;
            var bytesWritten = 0;
            if (target.TryFormat(bufferWriter.GetSpan(length), out bytesWritten))
            {
                bufferWriter.Advance(bytesWritten);
                writer.WriteBytes(bufferWriter.WrittenSpan);
            }
            else
            {
                Utf8Helper.FromUtf16(bufferWriter, target.ToString(CultureInfo.InvariantCulture));
                writer.WriteBytes(bufferWriter.WrittenSpan);
            }
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }
}

internal sealed class NullableDecimalFormatter : ITomlValueFormatter<decimal?>
{
    public static readonly NullableDecimalFormatter Instance = new NullableDecimalFormatter();

    public decimal? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue) return default;

        var tomlValue = rootNode.Value;
        if (tomlValue is TomlInteger)
        {
            return tomlValue.GetInt64();
        }
        else
        {
            var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();

            try
            {
                var length = 128;
                var bytesWritten = 0;
                while (!tomlValue.TryFormat(bufferWriter.GetSpan(length), out bytesWritten))
                {
                    length *= 2;
                }
                bufferWriter.Advance(bytesWritten);

                if (Utf8Parser.TryParse(bufferWriter.WrittenSpan, out decimal value, out int bytesConsumed, '\0'))
                {
                    return value;
                }
            }
            finally
            {
                RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
            }
        }
        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, decimal? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
            try
            {
                var length = 64;
                var bytesWritten = 0;
                if (target.Value.TryFormat(bufferWriter.GetSpan(length), out bytesWritten))
                {
                    bufferWriter.Advance(bytesWritten);
                    writer.WriteBytes(bufferWriter.WrittenSpan);
                }
                else
                {
                    Utf8Helper.FromUtf16(bufferWriter, target.Value.ToString(CultureInfo.InvariantCulture));
                    writer.WriteBytes(bufferWriter.WrittenSpan);
                }
            }
            finally
            {
                RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
            }
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(decimal?));
        }
    }
}


