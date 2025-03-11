using CsToml.Values;
using System.Buffers;
using System.Text;
using System.Text.Json.Nodes;

namespace CsToml.Tests;

internal static class TomlDocumentExtensions
{
    public static JsonNode ToJsonObject(this TomlDocument document)
    {
        var jsonObj = new JsonObject();
        var rootTomlNode = document.RootNode.Node;

        foreach (var pair in rootTomlNode.KeyValuePairs)
        {
            AddChildNode(jsonObj, pair.Value, pair.Key.Utf16String);
        }

        return jsonObj;
    }

    private static void AddChildNode<TJsonNode>(TJsonNode jsonObj, TomlTableNode tomlNode, string? key)
        where TJsonNode : JsonNode
    {
        static void Add(JsonNode node, string? key, JsonNode value)
        {
            if (node is JsonObject jsonObj)
            {
                jsonObj.Add(key!, value);
            }
            else if (node is JsonArray arrayJsonObj)
            {
                if (key == null)
                {
                    arrayJsonObj.Add(value);
                }
                else
                {
                    arrayJsonObj.Add(new JsonObject() { [key] = value });
                }
            }
        }

        var tomlValue = tomlNode.Value;
        if (tomlValue?.HasValue ?? false)
        {
            AddChildTomlValue(jsonObj, tomlValue, key);
        }
        else
        {
            var childJsonObj = new JsonObject();
            Add(jsonObj, key!, childJsonObj);
            foreach (var pair in tomlNode.KeyValuePairs)
            {
                AddChildNode(childJsonObj, pair.Value, pair.Key.Utf16String);
            }
            return;
        }

        foreach (var pair in tomlNode.KeyValuePairs)
        {
            AddChildNode(jsonObj, pair.Value, pair.Key.Utf16String);
        }
    }

    private static void AddChildTomlValue<TJsonNode>(TJsonNode jsonObj, TomlValue tomlValue, string? key)
        where TJsonNode : JsonNode
    {
        static void Add(JsonNode node, string? key, JsonNode value)
        {
            if (node is JsonObject jsonObj)
                jsonObj.Add(key!, value);
            else if (node is JsonArray arrayJsonObj)
            {
                if (key == null)
                {
                    arrayJsonObj.Add(value);
                }
                else
                {
                    arrayJsonObj.Add(new JsonObject() { [key] = value });
                }
            }
        }

        switch (tomlValue)
        {
            case TomlInteger integerValue:
                Add(jsonObj, key!, new JsonObject { ["type"] = "integer", ["value"] = integerValue.ToTomlUtf16String() });
                break;
            case TomlFloat floatValue:
                Add(jsonObj, key, new JsonObject { ["type"] = "float", ["value"] = floatValue.ToTomlUtf16String() });
                break;
            case TomlString stringValue:
                Add(jsonObj, key, new JsonObject { ["type"] = "string", ["value"] = stringValue.GetString() });
                break;
            case TomlBoolean booleanValue:
                Add(jsonObj, key, new JsonObject { ["type"] = "bool", ["value"] = booleanValue.ToTomlUtf16String() });
                break;
            case TomlOffsetDateTime dateTimeOffsetValue:
                Add(jsonObj, key, new JsonObject { ["type"] = "datetime", ["value"] = dateTimeOffsetValue.ToTomlUtf16String() });
                break;
            case TomlLocalDateTime localDateTimeValue:
                Add(jsonObj, key, new JsonObject { ["type"] = "datetime-local", ["value"] = localDateTimeValue.ToTomlUtf16String() });
                break;
            case TomlLocalDate localDateValue:
                Add(jsonObj, key, new JsonObject { ["type"] = "date-local", ["value"] = localDateValue.ToTomlUtf16String() });
                break;
            case TomlLocalTime localTimeValue:
                Add(jsonObj, key, new JsonObject { ["type"] = "time-local", ["value"] = localTimeValue.ToTomlUtf16String() });
                break;
            case TomlArray arrayValue:
                var arrayJsonObj = new JsonArray();
                Add(jsonObj, key, arrayJsonObj);
                foreach (var v in arrayValue)
                {
                    AddChildTomlValue(arrayJsonObj, v, null);
                }
                break;
            case TomlTable tableValue: // Array of Tables
                var childTableJsonObj = new JsonObject();
                Add(jsonObj, key, childTableJsonObj);
                foreach (var pair in tableValue.RootNode.KeyValuePairs)
                {
                    AddChildNode(childTableJsonObj, pair.Value, pair.Key.Utf16String);
                }
                break;
            case TomlInlineTable tomlInlineTable:
                var childinlineTableJsonObj = new JsonObject();
                Add(jsonObj, key, childinlineTableJsonObj);
                foreach (var pair in tomlInlineTable.RootNode.KeyValuePairs)
                {
                    AddChildNode(childinlineTableJsonObj, pair.Value, pair.Key.Utf16String);
                }
                break;
        }
    }

    private static string ToTomlUtf16String(this TomlValue tomlValue)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        var writer = new Utf8TomlDocumentWriter<ArrayBufferWriter<byte>>(ref bufferWriter);
        tomlValue.ToTomlString(ref writer);
        return Encoding.UTF8.GetString(bufferWriter.WrittenSpan);
    }

}

