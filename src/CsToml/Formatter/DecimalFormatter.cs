using CsToml.Error;
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

        if (rootNode.HasValueOnly)
        {
            // If toml value is an integer, get it as Int64 to avoid precision loss when converting from double to decimal
            var rawTomlValue = rootNode.Value;
            if (rawTomlValue.Type == TomlValueType.Integer)
            {
                return rawTomlValue.GetInt64();
            }

            if (rawTomlValue.TryGetDouble(out var doubleValue))
            {
                return ConvertToDecimal(doubleValue);
            }
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(decimal));
        return default;
    }

    internal static decimal ConvertToDecimal(double doubleValue)
    {
        // The (decimal)double cast rounding changed in .NET 11 (.NET 11 preview 7).
        // [dotnet/runtime PR]: https://github.com/dotnet/runtime/pull/130566
        // Parse the shortest round-trippable representation instead of casting, so the result keeps
        // the decimal digits written in the TOML text (e.g. 3.14 -> 3.14m) on all runtime versions.

        if (!double.IsFinite(doubleValue))
        {
            return (decimal)doubleValue;
        }

        Span<byte> utf8 = stackalloc byte[32];
        doubleValue.TryFormat(utf8, out var bytesWritten, default, CultureInfo.InvariantCulture);
        if (Utf8Parser.TryParse(utf8.Slice(0, bytesWritten), out decimal value, out var bytesConsumed, 'G') && bytesConsumed == bytesWritten)
        {
            return value;
        }

        return (decimal)doubleValue;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, decimal target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteDecimal(target);
    }
}

internal sealed class NullableDecimalFormatter : ITomlValueFormatter<decimal?>
{
    public static readonly NullableDecimalFormatter Instance = new NullableDecimalFormatter();

    public decimal? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue) return default;

        if (rootNode.HasValueOnly)
        {
            // If toml value is an integer, get it as Int64 to avoid precision loss when converting from double to decimal
            var rawTomlValue = rootNode.Value;
            if (rawTomlValue.Type == TomlValueType.Integer)
            {
                return rawTomlValue.GetInt64();
            }

            if (rawTomlValue.TryGetDouble(out var doubleValue))
            {
                return DecimalFormatter.ConvertToDecimal(doubleValue);
            }
        }

        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, decimal? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            writer.WriteDecimal(target.GetValueOrDefault());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(decimal?));
        }
    }
}


