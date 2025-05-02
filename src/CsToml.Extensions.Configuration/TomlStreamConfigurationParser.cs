using CsToml.Values;
using Microsoft.Extensions.Configuration;

namespace CsToml.Extensions.Configuration;

internal sealed class TomlStreamConfigurationParser
{
    private IDictionary<string, string?> data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    private readonly Stack<string> paths = new();
    private bool isEmpty;

    public IDictionary<string, string?> Parse(Stream stream)
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(stream);
        VisitObjectElement(document.RootNode);
        return data;
    }

    private void VisitObjectElement(TomlDocumentNode node)
    {
        isEmpty = true;
        foreach (var element in node)
        {
            isEmpty = false;
            EnterContext(element.Key.GetString()!);
            VisitElement(element.Value);
            ExitContext();
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
            EnterContext(index.ToString());
            VisitElement(new TomlDocumentNode(arrayElement));
            ExitContext();
            index++;
        }

        if (paths.Count > 0 && index == 0)
        {
            data[paths.Peek()] = null;
        }
    }

    private void EnterContext(string context)
        => paths.Push(paths.Count > 0 ? paths.Peek() + ConfigurationPath.KeyDelimiter + context : context);

    private void ExitContext()
        => paths.Pop();
}


