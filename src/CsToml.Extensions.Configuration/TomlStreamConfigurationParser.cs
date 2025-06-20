using CsToml.Values;
using Microsoft.Extensions.Configuration;

namespace CsToml.Extensions.Configuration;

internal struct TomlStreamConfigurationParser
{
    private readonly Dictionary<string, string?> data = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stack<string> paths = new();
    private bool isEmpty;

    public TomlStreamConfigurationParser() { }

    public IDictionary<string, string?> Parse(Stream stream, CsTomlSerializerOptions? serializerOptions)
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(stream, serializerOptions);
        VisitObjectElement(document.RootNode);
        return data;
    }

    private void VisitObjectElement(TomlDocumentNode node)
    {
        isEmpty = true;
        foreach (var element in node)
        {
            isEmpty = false;
            paths.Push(paths.Count > 0 ? $"{paths.Peek()}{ConfigurationPath.KeyDelimiter}{element.Key.GetString()}" : element.Key.GetString());
            VisitElement(element.Value);
            paths.Pop();
        }

        if (paths.Count > 0 && isEmpty)
        {
            data[paths.Peek()] = null;
        }
    }

    private void VisitElement(TomlDocumentNode node)
    {
        if (node.HasNodeOnly)
        {
            VisitObjectElement(node);
            return;
        }
        else if (node.CanGetValue(TomlValueFeature.Array))
        {
            VisitArrayElement(node);
            return;
        }
        else if (node.CanGetValue(TomlValueFeature.Table | TomlValueFeature.InlineTable))
        {
            VisitObjectElement(node);
            return;
        }
        else if (node.HasValue)
        {
            var key = paths.Peek();
            if (data.ContainsKey(key))
            {
                throw new FormatException();
            }
            data[key] = node.GetString();
        }
    }

    private void VisitArrayElement(TomlDocumentNode node)
    {
        int index = 0;

        foreach (var arrayElement in node.GetArray())
        {
            paths.Push(paths.Count > 0 ? $"{paths.Peek()}{ConfigurationPath.KeyDelimiter}{index}" : $"{index}");
            VisitElement(new TomlDocumentNode(arrayElement));
            paths.Pop();
            index++;
        }

        if (paths.Count > 0 && index == 0)
        {
            data[paths.Peek()] = null;
        }
    }


}


