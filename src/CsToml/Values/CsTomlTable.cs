using CsToml.Error;
using CsToml.Extension;
using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CsToml.Values;

[DebuggerDisplay("Count = {RootNode.NodeCount}")]
internal partial class CsTomlTable : CsTomlValue
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly CsTomlTableNode node = CsTomlTableNode.CreateGroupingPropertyNode();

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    internal CsTomlTableNode RootNode => node;

    internal CsTomlTableNode this[ReadOnlySpan<byte> key]
    {
        get
        {
            if (RootNode.TryGetChildNode(key, out var value))
            {
                return value!;
            }
            return CsTomlTableNode.Empty;
        }
    }

    internal CsTomlTable() : base() { }

    internal void AddKeyValue(ArrayPoolList<CsTomlDotKey> csTomlKey, CsTomlValue value, CsTomlTableNode? searchRootNode, IReadOnlyCollection<CsTomlString> comments)
    {
        var currentNode = searchRootNode ?? RootNode;
        var dotKeys = csTomlKey.WrittenSpan;
        var lastKey = dotKeys[^1];

        for (var i = 0; i < dotKeys.Length - 1; i++)
        {
            var sectionKey = dotKeys[i];
            if (currentNode!.TryAddGroupingPropertyNode(sectionKey, out var childNode))
            {
                currentNode = childNode;
                continue;
            }
            if (childNode.IsTableHeaderDefinitionPosition)
            {
                ExceptionHelper.ThrowTheKeyIsDefinedAsTable();
            }
            if (childNode.IsArrayOfTablesHeaderDefinitionPosition)
            {
                ExceptionHelper.ThrowTheKeyIsDefinedAsArrayOfTables();
            }
            if (childNode.IsGroupingProperty)
            {
                currentNode = childNode;
                continue;
            }

            ExceptionHelper.ThrowNotTurnIntoTable(csTomlKey.WrittenSpan.GetJoinName());
        }

        currentNode.AddKeyValue(lastKey, value, comments);
    }

    internal void AddTableHeader(ArrayPoolList<CsTomlDotKey> csTomlKey, IReadOnlyCollection<CsTomlString> comments, out CsTomlTableNode? newNode)
    {
        var node = RootNode;
        var dotKeys = csTomlKey.WrittenSpan;

        var addedNewNode = false;
        for (var i = 0; i < dotKeys.Length; i++)
        {
            var sectionKey = dotKeys[i];
            if (node!.TryAddGroupingPropertyNode(sectionKey, out var childNode))
            {
                addedNewNode = true;
                node = childNode;
                node.IsTableHeader = true;
                continue;
            }
            
            if (childNode!.IsArrayOfTablesHeaderDefinitionPosition)
            {
                var tableHeaderArrayValue = (childNode!.Value as CsTomlArray)?.LastValue;
                node = (tableHeaderArrayValue as CsTomlTable)?.RootNode;
                continue;
            }

            if (childNode!.IsGroupingProperty)
            {
                node = childNode;
                continue;
            }

            newNode = null;
            ExceptionHelper.ThrowIncorrectTomlFormat();
        }

        if (!addedNewNode)
        {
            if (node!.IsTableHeaderDefinitionPosition)
            {
                ExceptionHelper.ThrowTableHeaderIsDefined(csTomlKey.WrittenSpan.GetJoinName());
            }
            if (node!.IsArrayOfTablesHeaderDefinitionPosition)
            {
                ExceptionHelper.ThrowTableHeaderIsDefinedAsArrayOfTables(csTomlKey.WrittenSpan.GetJoinName());
            }
            if (!node!.IsTableHeader)
            {
                ExceptionHelper.ThrowTableHeaderIsDefined(csTomlKey.WrittenSpan.GetJoinName());
            }
        }

        if (comments.Count > 0)
        {
            node!.AddComment(comments);
        }
        node!.IsTableHeaderDefinitionPosition = true;
        newNode = node;
    }

    internal void AddArrayOfTablesHeader(ArrayPoolList<CsTomlDotKey> csTomlKey, IReadOnlyCollection<CsTomlString> comments, out CsTomlTableNode? newNode)
    {
        var currentNode = RootNode;
        var dotKeys = csTomlKey.WrittenSpan;

        var addedNewNode = false;
        for (var i = 0; i < dotKeys.Length; i++)
        {
            var sectionKey = dotKeys[i];
            if (currentNode!.TryAddGroupingPropertyNode(sectionKey, out var childNode))
            {
                addedNewNode = true;
                currentNode = childNode;
                currentNode.IsArrayOfTablesHeader = true;
                currentNode.IsTableHeader = i < dotKeys.Length - 1;
                continue;
            }

            if (childNode!.IsArrayOfTablesHeaderDefinitionPosition)
            {
                if (i == dotKeys.Length - 1)
                {
                    currentNode = childNode;
                    continue;
                }
                else
                {
                    var tableHeaderArrayValue = (childNode!.Value as CsTomlArray)?.LastValue;
                    currentNode = (tableHeaderArrayValue as CsTomlTable)?.RootNode;
                    continue;
                }
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
            ExceptionHelper.ThrowTheArrayOfTablesIsDefinedAsTable(csTomlKey.WrittenSpan.GetJoinName());
        }

        if (addedNewNode)
        {
            currentNode.Value = new CsTomlArray();
            currentNode.IsArrayOfTablesHeader = true;
            currentNode.IsArrayOfTablesHeaderDefinitionPosition = true;
        }
        else
        {
            if (!currentNode!.IsArrayOfTablesHeaderDefinitionPosition)
            {
                ExceptionHelper.ThrowTheArrayOfTablesIsDefinedAsTable(csTomlKey.WrittenSpan.GetJoinName());
            }
        }
        var table = new CsTomlTable();
        (currentNode.Value as CsTomlArray)?.Add(table);
        if (comments.Count > 0)
        {
            currentNode.AddComment(comments);
        }
        newNode = table.RootNode;
    }

    internal CsTomlValue FindAsDottedKey(ReadOnlySpan<byte> dottedKeySpan)
        => FindNodeAsDottedKey(RootNode, dottedKeySpan).Value!;

    internal CsTomlValue Find(ReadOnlySpan<ByteArray> dottedKeys)
        => FindNode(RootNode, dottedKeys).Value!;

    internal CsTomlValue FindAsKey(ReadOnlySpan<byte> keySpan)
    {
        var node = FindNode(RootNode, keySpan);
        if (!node!.IsGroupingProperty)
        {
            return node.Value!;
        }
        return Empty;
    }

    internal CsTomlValue FindArrayOfTableOrValueAsDotted(ReadOnlySpan<byte> keys)
    {
        var node = FindNodeAsDottedKey(RootNode, keys);
        if (!node!.IsGroupingProperty || node!.IsArrayOfTablesHeader)
        {
            return node.Value!;
        }
        return Empty;
    }

    internal CsTomlValue FindArrayOfTableOrValue(ReadOnlySpan<byte> keySpan)
    {
        var node = FindNode(RootNode, keySpan);
        if (!node!.IsGroupingProperty || node!.IsArrayOfTablesHeader)
        {
            return node.Value!;
        }
        return Empty;
    }

    internal CsTomlValue FindArrayOfTablesOrValue(ReadOnlySpan<ByteArray> keys)
    {
        var node = FindNode(RootNode, keys);
        if (!node!.IsGroupingProperty || node!.IsArrayOfTablesHeader)
        {
            return node.Value!;
        }
        return Empty;
    }

    internal CsTomlTableNode FindNodeAsDottedKey(CsTomlTableNode root, ReadOnlySpan<byte> dottedKeySpan)
    {
        var hit = false;
        var currentNode = root;
        foreach (var dottedKey in dottedKeySpan.SplitSpan("."u8))
        {
            if (currentNode!.TryGetChildNode(dottedKey.Value, out var childNode))
            {
                hit = true;
                currentNode = childNode;
            }
        }
        
        return hit ? currentNode! : CsTomlTableNode.Empty;
    }

    internal CsTomlTableNode FindNode(CsTomlTableNode root, ReadOnlySpan<byte> dottedKeySpan)
        => root.TryGetChildNode(dottedKeySpan, out var childNode) ? childNode! : CsTomlTableNode.Empty;

    internal CsTomlTableNode FindNode(CsTomlTableNode root, ReadOnlySpan<ByteArray> dottedKeys)
    {
        var hit = false;
        var currentNode = root;

        for (int i = 0; i < dottedKeys.Length; i++)
        {
            if (currentNode!.TryGetChildNode(dottedKeys[i].value, out var childNode))
            {
                hit = true;
                currentNode = childNode;
            }
        }

        return hit ? currentNode! : CsTomlTableNode.Empty;
    }

    internal override bool ToTomlString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer)
    {
        var csTomlWriter = new CsTomlWriter<TBufferWriter>(ref writer);
        var keys = new List<CsTomlDotKey>();
        var tableNodes = new List<CsTomlDotKey>();
        ToTomlStringCore(ref csTomlWriter, RootNode, keys, tableNodes);

        return true;
    }

    private void ToTomlStringCore<TBufferWriter>(ref CsTomlWriter<TBufferWriter> writer, CsTomlTableNode parentNode, List<CsTomlDotKey> keys, List<CsTomlDotKey> tableHeaderKeys)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (parentNode.IsArrayOfTablesHeader)
        {
            if (parentNode.Value is CsTomlArray arrayValue)
            {
                var skipNewLine = false;
                if (parentNode.CommentCount > 0)
                {
                    if (writer.WrittingCount > 0)
                    {
                        writer.WriteNewLine();
                    }
                    writer.WriteComments(parentNode.CommentSpan);
                    skipNewLine = true;
                }
                var keysSpan = CollectionsMarshal.AsSpan(tableHeaderKeys);
                foreach (var v in arrayValue)
                {
                    if (writer.WrittingCount > 0 && !skipNewLine)
                    {
                        writer.WriteNewLine();
                    }
                    writer.WriteArrayOfTablesHeader(keysSpan);
                    if (v is CsTomlTable table)
                    {
                        table.ToTomlStringCore(ref writer, table.RootNode, [], tableHeaderKeys);
                    }
                    skipNewLine = false;
                }
            }
        }

        foreach (var (key, childNode) in parentNode.KeyValuePairs)
        {
            tableHeaderKeys.Add(key);
            if (childNode.IsGroupingProperty)
            {
                if (!childNode.IsTableHeader && parentNode.IsTableHeader && keys.Count > 0)
                {
                    var skipNewLine = false;
                    if (parentNode.CommentCount > 0)
                    {
                        writer.WriteComments(parentNode.CommentSpan);
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
                    if (parentNode.CommentCount > 0)
                    {
                        if (writer.WrittingCount > 0)
                        {
                            writer.WriteNewLine();
                        }
                        writer.WriteComments(parentNode.CommentSpan);
                        skipNewLine = true;
                    }
                    if (writer.WrittingCount > 0 && !skipNewLine)
                    {
                        writer.WriteNewLine();
                    }
                    var keysSpan = CollectionsMarshal.AsSpan(tableHeaderKeys);
                    writer.WriteTableHeader(keysSpan);
                    keys.Clear();
                    writer.WriteKeyValueAndNewLine(key, childNode.Value!);
                }
                else
                {
                    if (childNode.CommentCount > 0)
                    {
                        writer.WriteComments(childNode.CommentSpan);
                    }
                    var keysSpan = CollectionsMarshal.AsSpan(keys);
                    if (keysSpan.Length > 0)
                    {
                        for (var i = 0; i < keysSpan.Length; i++)
                        {
                            writer.WriterKey(keysSpan[i], true);
                        }
                    }
                    writer.WriteKeyValueAndNewLine(key, childNode.Value!);
                }
            }
            tableHeaderKeys.Remove(key);
        }

        keys.Clear(); // clear subkey
    }

}
