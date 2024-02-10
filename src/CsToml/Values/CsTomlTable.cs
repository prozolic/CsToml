using CsToml.Error;
using CsToml.Utility;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlTable : {RootNode}")]
internal partial class CsTomlTable : CsTomlValue
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly CsTomlTableNode node = CsTomlTableNode.CreateGroupingPropertyNode();

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public CsTomlTableNode RootNode => node;

    public CsTomlTable() : base(CsTomlType.Table) { }

    public void AddKeyValue(CsTomlKey csTomlKey, CsTomlValue value, CsTomlTableNode? searchRootNode, IReadOnlyCollection<CsTomlString> comments)
    {
        var currentNode = searchRootNode ?? RootNode;
        var dotKeys = csTomlKey.DotKeys;

        for (var i = 0; i < dotKeys.Count - 1; i++)
        {
            var sectionKey = dotKeys[i];
            if (currentNode!.TryAddGroupingPropertyNode(sectionKey, out var childNode) || childNode.IsGroupingProperty)
            {
                currentNode = childNode;
                continue;
            }

            ExceptionHelper.ThrowNotTurnIntoTable(csTomlKey.GetJoinName());
        }

        var key = dotKeys[dotKeys.Count - 1];
        currentNode.AddKeyValue(key, value, comments);
    }

    public void AddTableHeader(CsTomlKey csTomlKey, out CsTomlTableNode? newNode, IReadOnlyCollection<CsTomlString> comments)
    {
        var currentNode = RootNode;
        var dotKeys = csTomlKey.DotKeys;

        var addedNewNode = false;
        for (var i = 0; i < dotKeys.Count; i++)
        {
            var sectionKey = dotKeys[i];
            if (currentNode!.TryAddGroupingPropertyNode(sectionKey, out var childNode))
            {
                addedNewNode = true;
                currentNode = childNode;
                currentNode.IsTableHeader = true;
                continue;
            }

            if (childNode!.IsGroupingProperty)
            {
                currentNode = childNode;
                continue;
            }

            newNode = null;
            ExceptionHelper.ThrowIncorrectTomlFormat();
        }

        if (!addedNewNode)
        {
            if (currentNode.IsTableHeaderDefinitionPosition)
            {
                ExceptionHelper.ThrowTableHeaderIsDefined(csTomlKey.GetJoinName());
            }
            if (currentNode.IsTableArrayHeaderDefinitionPosition)
            {
                ExceptionHelper.ThrowTableHeaderIsDefinedAsTableArray(csTomlKey.GetJoinName());
            }
        }

        newNode = currentNode;
        newNode.AddComment(comments);
        newNode.IsTableHeaderDefinitionPosition = true;
    }

    public void AddTableArrayHeader(CsTomlKey csTomlKey, out CsTomlTableNode? newNode, IReadOnlyCollection<CsTomlString> comments)
    {
        var currentNode = RootNode;
        var dotKeys = csTomlKey.DotKeys;

        var addedNewNode = false;
        for (var i = 0; i < dotKeys.Count; i++)
        {
            var sectionKey = dotKeys[i];
            if (currentNode!.TryAddGroupingPropertyNode(sectionKey, out var childNode))
            {
                addedNewNode = true;
                currentNode = childNode;
                currentNode.IsTableArrayHeader = true;
                continue;
            }
            if (childNode!.IsGroupingProperty)
            {
                currentNode = childNode;
                continue;
            }

            newNode = null;
            ExceptionHelper.ThrowIncorrectTomlFormat();
        }

        if (currentNode!.IsTableHeader)
        {
            ExceptionHelper.ThrowTableArrayIsDefinedAsTable(csTomlKey.GetJoinName());
        }

        if (addedNewNode)
        {
            currentNode.Value = new CsTomlArray();
            currentNode.IsTableArrayHeader = true;
            currentNode.IsTableArrayHeaderDefinitionPosition = true;
        }
        var table = new CsTomlTable();
        (currentNode.Value as CsTomlArray)?.Add(table);
        currentNode.AddComment(comments);
        newNode = table.RootNode;
    }

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        var csTomlWriter = new CsTomlWriter(ref writer);
        var keys = new List<CsTomlString>();
        var tableNodes = new List<CsTomlString>();
        ToTomlStringCore(ref csTomlWriter, RootNode, keys, tableNodes);

        return true;
    }

    private void ToTomlStringCore(ref CsTomlWriter writer, CsTomlTableNode parentNode, List<CsTomlString> keys, List<CsTomlString> tableHeaderKeys)
    {
        if (parentNode.IsTableArrayHeader)
        {
            if (parentNode.Value is CsTomlArray arrayValue)
            {
                var skipNewLine = false;
                if (parentNode.Comments.Count > 0)
                {
                    if (writer.WrittingCount > 0)
                    {
                        writer.WriteNewLine();
                    }
                    writer.WriteComments(parentNode.CommentsSpan);
                    skipNewLine = true;
                }
                var keysSpan = CollectionsMarshal.AsSpan(tableHeaderKeys);
                foreach (var v in arrayValue)
                {
                    if (writer.WrittingCount > 0 && !skipNewLine)
                    {
                        writer.WriteNewLine();
                    }
                    writer.WriteTableArrayHeader(keysSpan);
                    if (v is CsTomlTable table)
                    {
                        table.ToTomlStringCore(ref writer, table.RootNode, [], tableHeaderKeys);
                    }
                    skipNewLine = false;
                }
            }
        }

        foreach (var (key, childNode) in parentNode.Nodes)
        {
            tableHeaderKeys.Add(key);
            if (childNode.IsGroupingProperty)
            {
                if (!childNode.IsTableHeader && parentNode.IsTableHeader && keys.Count > 0)
                {
                    var skipNewLine = false;
                    if (parentNode.Comments.Count > 0)
                    {
                        writer.WriteComments(parentNode.CommentsSpan);
                        skipNewLine = true;
                    }
                    if (writer.WrittingCount > 0 && !skipNewLine)
                    {
                        writer.WriteNewLine();
                    }
                    var keysSpan = CollectionsMarshal.AsSpan(keys);
                    writer.WriteTableHeader(keysSpan);
                    keys.Clear();
                }
                keys.Add(key);
                ToTomlStringCore(ref writer, childNode, keys, tableHeaderKeys);
            }
            else
            {
                tableHeaderKeys.RemoveAt(tableHeaderKeys.Count - 1);
                if (parentNode.IsTableHeader && keys.Count > 0)
                {
                    var skipNewLine = false;
                    if (parentNode.Comments.Count > 0)
                    {
                        if (writer.WrittingCount > 0)
                        {
                            writer.WriteNewLine();
                        }
                        writer.WriteComments(parentNode.CommentsSpan);
                        skipNewLine = true;
                    }
                    if (writer.WrittingCount > 0 && !skipNewLine)
                    {
                        writer.WriteNewLine();
                    }
                    var keysSpan = CollectionsMarshal.AsSpan(tableHeaderKeys);
                    writer.WriteTableHeader(keysSpan);
                    keys.Clear();
                    writer.WriteKeyValueAndNewLine(in key, childNode.Value!);
                }
                else
                {
                    if (childNode.Comments.Count > 0)
                    {
                        writer.WriteComments(childNode.CommentsSpan);
                    }
                    var keysSpan = CollectionsMarshal.AsSpan(keys);
                    if (keysSpan.Length > 0)
                    {
                        for (var i = 0; i < keysSpan.Length; i++)
                        {
                            writer.WriterKey(in keysSpan[i], true);
                        }
                    }
                    writer.WriteKeyValueAndNewLine(in key, childNode.Value!);
                }
            }
            tableHeaderKeys.Remove(key);
        }

        keys.Clear(); // clear subkey
    }

}
