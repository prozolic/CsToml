using System.Buffers;
using System.Collections;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CsToml.Playground.Utility;

/// <summary>
/// Converts a parsed <see cref="TomlDocument"/> into an indented JSON string for the output pane.
/// </summary>
public static class TomlJsonConverter
{
    private static readonly JsonWriterOptions WriterOptions = new()
    {
        Indented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string ToJson(TomlDocument document)
    {
        var dictionary = document.ToDictionary<object, object>();

        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer, WriterOptions))
        {
            WriteValue(writer, dictionary);
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    private static void WriteValue(Utf8JsonWriter writer, object? value)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                break;
            case string s:
                writer.WriteStringValue(s);
                break;
            case bool b:
                writer.WriteBooleanValue(b);
                break;
            case long l:
                writer.WriteNumberValue(l);
                break;
            case double d when double.IsFinite(d):
                writer.WriteNumberValue(d);
                break;
            case double d:
                // JSON has no representation for inf/nan; keep the TOML notation as a string.
                writer.WriteStringValue(double.IsNaN(d) ? "nan" : d > 0 ? "inf" : "-inf");
                break;
            case DateTime dt:
                writer.WriteStringValue(dt.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF"));
                break;
            case DateTimeOffset dto:
                writer.WriteStringValue(dto.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz"));
                break;
            case DateOnly date:
                writer.WriteStringValue(date.ToString("yyyy-MM-dd"));
                break;
            case TimeOnly time:
                writer.WriteStringValue(time.ToString("HH:mm:ss.FFFFFFF"));
                break;
            case IDictionary dictionary:
                writer.WriteStartObject();
                foreach (DictionaryEntry entry in dictionary)
                {
                    writer.WritePropertyName(entry.Key.ToString() ?? "");
                    WriteValue(writer, entry.Value);
                }
                writer.WriteEndObject();
                break;
            case IEnumerable sequence:
                writer.WriteStartArray();
                foreach (var item in sequence)
                {
                    WriteValue(writer, item);
                }
                writer.WriteEndArray();
                break;
            default:
                writer.WriteStringValue(value.ToString());
                break;
        }
    }
}
