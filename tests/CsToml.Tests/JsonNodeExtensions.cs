
using System.Text.Json.Nodes;

namespace CsToml.Tests;

internal static class JsonNodeExtensions
{
    public static bool DeepEqualsForTomlFormat(JsonNode expectedNode, JsonNode actualNode)
    {
        //var result = JsonNode.DeepEquals(expectedNode, actualNode);
        //if (result) return true;

        // Values are compared as strings and may vary from output to output.
        // Therefore, the values are converted to TOML values and reverified.
        return DeepEqualsCore(expectedNode, actualNode);
    }

    private static bool DeepEqualsCore(JsonNode? expectedNode, JsonNode? actualNode)
    {
        if (expectedNode is JsonArray expectedArrayNode)
        {
            if (actualNode is not JsonArray actualArrayNode)
            {
                return false;
            }

            var expectedNodeList = (IList<JsonNode?>)expectedArrayNode!;
            var actualNodeList = (IList<JsonNode?>)actualArrayNode!;

            if (expectedNodeList.Count != actualNodeList.Count)
            {
                return false;
            }

            for (int i = 0; i < expectedNodeList.Count; i++)
            {
                if (!DeepEqualsCore(expectedNodeList[i], actualNodeList[i]))
                {
                    return false;
                }
            }

            return true;
        }
        else if (expectedNode is JsonObject valueNode)
        {
            var expectedNodeDict = (IDictionary<string, JsonNode?>)expectedNode!;
            var actualNodeDict = (IDictionary<string, JsonNode?>)actualNode!;

            if (expectedNodeDict.Count != actualNodeDict.Count)
                return false;

            if (expectedNodeDict.Count == 2 && actualNodeDict.Count == 2)
            {
                if (expectedNodeDict.TryGetValue("type", out var n) && n!.GetValueKind() == System.Text.Json.JsonValueKind.String &&
                    expectedNodeDict.ContainsKey("value") &&
                    actualNodeDict.TryGetValue("type", out var n2) && n2!.GetValueKind() == System.Text.Json.JsonValueKind.String &&
                    actualNodeDict.ContainsKey("value"))
                {
                    return EqualTomlValue(expectedNodeDict, actualNodeDict);
                }
            }

            foreach (var item in expectedNodeDict)
            {
                JsonNode? jsonNode = actualNodeDict[item.Key];

                if (!DeepEqualsCore(item.Value, jsonNode))
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    private static bool EqualTomlValue(IDictionary<string, JsonNode?> expected, IDictionary<string, JsonNode?> actual)
    {
        var expectedValueType = expected["type"]!.GetValue<string>().AsSpan();
        var actualValueType = actual["type"]!.GetValue<string>().AsSpan();

        if (!expectedValueType.SequenceEqual(actualValueType))
            return false;

        var expectedValue = expected["value"]!.GetValue<string>().AsSpan();
        var actualValue = actual["value"]!.GetValue<string>().AsSpan();

        switch (expectedValueType)
        {
            case "string":
                return expectedValue.SequenceEqual(actualValue);
            case "integer":
                if (long.TryParse(expectedValue, out var intValue) && long.TryParse(actualValue, out var intValue2))
                {
                    return intValue == intValue2;
                }
                return false;
            case "float":
                // Special float values
                switch (expectedValue)
                {
                    case "inf":
                    case "+inf":
                    case "-inf":
                    case "nan":
                    case "+nan":
                    case "-nan":
                        return expectedValue.SequenceEqual(actualValue);
                }

                if (double.TryParse(expectedValue, out var floatValue) && double.TryParse(actualValue, out var floatValue2))
                {
                    return floatValue == floatValue2;
                }

                return false;
            case "bool":
                if (bool.TryParse(expectedValue, out var boolValue) && bool.TryParse(actualValue, out var boolValue2))
                {
                    return boolValue == boolValue2;
                }
                return false;
            case "datetime":
                if (DateTimeOffset.TryParse(expectedValue, out var dateTimeOffsetValue) && DateTimeOffset.TryParse(actualValue, out var dateTimeOffsetValue2))
                {
                    return dateTimeOffsetValue == dateTimeOffsetValue2;
                }
                return false;
            case "datetime-local":
                if (DateTime.TryParse(expectedValue, out var dateTimeValue) && DateTime.TryParse(actualValue, out var dateTimeValue2))
                {
                    return dateTimeValue == dateTimeValue2;
                }
                return false;
            case "date-local":
                if (DateOnly.TryParse(expectedValue, out var dateOnlyValue) && DateOnly.TryParse(actualValue, out var dateOnlyValue2))
                {
                    return dateOnlyValue == dateOnlyValue2;
                }
                return false;
            case "time-local":
                if (TimeOnly.TryParse(expectedValue, out var timeOnlyValue) && TimeOnly.TryParse(actualValue, out var timeOnlyValue2))
                {
                    return timeOnlyValue == timeOnlyValue2;
                }
                return false;
        }
        return false;

    }
}
