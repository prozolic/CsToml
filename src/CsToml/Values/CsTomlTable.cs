using CsToml.Error;
using CsToml.Extension;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlTable : {RootNode}")]
internal class CsTomlTable : CsTomlValue
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly CsTomlTableNode node = CsTomlTableNode.CreateGroupingPropertyNode();

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public CsTomlTableNode RootNode => node;

    public CsTomlTable() : base(CsTomlType.Table) { }

    public bool TryAddValue(CsTomlKey csTomlKey, CsTomlValue value, CsTomlTableNode? searchRootNode = null, IEnumerable<CsTomlString> comments = null)
    {
        var currentNode = searchRootNode ?? RootNode;
        var dotKeyStrings = csTomlKey.DotKeyStrings;

        for (var i = 0; i < dotKeyStrings.Count - 1; i++)
        {
            var sectionKey = dotKeyStrings[i];
            if (currentNode!.TryGetOrAddGroupingPropertyNode(sectionKey, out var childNode) || childNode.IsGroupingProperty)
            {
                currentNode = childNode;
                continue;
            }

            ExceptionHelper.ThrowIncorrectTomlFormat();
            return false;
        }

        var key = dotKeyStrings[dotKeyStrings.Count - 1];

        return currentNode.TryAddValue(key, value, comments);
    }

    public bool TryAddTableHeader(CsTomlKey csTomlKey, out CsTomlTableNode? newNode, IEnumerable<CsTomlString>? comments)
    {
        var currentNode = RootNode;
        var dotKeyStrings = csTomlKey.DotKeyStrings;

        for (var i = 0; i < dotKeyStrings.Count; i++)
        {
            var sectionKey = dotKeyStrings[i];
            if (currentNode!.TryGetOrAddGroupingPropertyNode(sectionKey, out var childNode))
            {
                currentNode = childNode;
                currentNode.IsTableHeader = true;
                continue;
            }
            if ((childNode!.IsTableHeader || childNode!.IsTableArrayHeader) && i == dotKeyStrings.Count - 1)
            {
                ExceptionHelper.ThrowTableHeaderIsRedefined(string.Join(".",dotKeyStrings.Select(s => s.Value)));
            }
            if (childNode!.IsGroupingProperty)
            {
                currentNode = childNode;
                continue;
            }
            ExceptionHelper.ThrowIncorrectTomlFormat();
            newNode = null;
            return false;
        }

        newNode = currentNode;
        newNode.AddComment(comments);
        newNode.IsTableHeader = true;
        return true;
    }

    public bool TryAddTableArrayHeader(CsTomlKey csTomlKey, out CsTomlTableNode? newNode, IEnumerable<CsTomlString>? comments)
    {
        var currentNode = RootNode;
        var dotKeyStrings = csTomlKey.DotKeyStrings;

        var isNewNode = false;
        for (var i = 0; i < dotKeyStrings.Count; i++)
        {
            var sectionKey = dotKeyStrings[i];
            if (currentNode!.TryGetOrAddGroupingPropertyNode(sectionKey, out var childNode))
            {
                isNewNode = true;
                currentNode.IsTableArrayHeader = true;
                currentNode = childNode;
                continue;
            }
            if (childNode!.IsTableArrayHeader && i == dotKeyStrings.Count - 1)
            {
                // sub-tables are registered or not
                foreach (var subNode in childNode.Nodes.Values)
                {
                    if (subNode.IsTableHeader || subNode.IsTableArrayHeader)
                    {
                        ExceptionHelper.ThrowOrderOfSubtableDefinitions(string.Join(".", dotKeyStrings.Select(s => s.Value)));
                    }
                }
                currentNode = childNode;
                break;
            }
            if (childNode!.IsTableHeader && i == dotKeyStrings.Count - 1)
            {
                ExceptionHelper.ThrowIncorrectTomlFormat();
            }
            if (childNode!.IsGroupingProperty)
            {
                currentNode = childNode;
                continue;
            }

            ExceptionHelper.ThrowIncorrectTomlFormat();
            newNode = null;
            return false;
        }

        if (isNewNode)
        {
            currentNode.Value = new CsTomlArray();
            currentNode.IsTableArrayHeader = true;
        }
        var table = new CsTomlTable();
        (currentNode.Value as CsTomlArray)?.Add(table);
        currentNode.AddComment(comments);
        newNode = table.RootNode;

        return true;
    }

    public bool TryGetValue(ReadOnlySpan<byte> key, out CsTomlValue? value, CsTomlTableNode? searchRootNode = null)
    {
        var hit = false;
        var separated = false;
        var currentNode = searchRootNode ?? RootNode;
        foreach (var separateKeyRange in key.SplitSpan("."u8))
        {
            separated = true;
            var separateKey = separateKeyRange.Value;
            if (currentNode!.TryGetChildNode(separateKey, out var childNode))
            {
                hit = true;
                currentNode = childNode;
            }
        }

        if (!separated)
        {
            if (currentNode!.TryGetChildNode(key, out var node))
            {
                value = node!.Value;
                return true;
            }
        }
        if (hit && (!currentNode!.IsGroupingProperty || currentNode!.IsTableArrayHeader))
        {
            value = currentNode.Value!;
            return true;
        }

        value = null;
        return false;
    }

}
