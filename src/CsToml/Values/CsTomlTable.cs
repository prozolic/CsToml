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
    public CsTomlTableNode RootNode => node;

    public CsTomlTableNode this[ReadOnlySpan<byte> key]
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

    public CsTomlTable() : base() { }

    public void AddKeyValue(ArrayPoolList<CsTomlDotKey> csTomlKey, CsTomlValue value, CsTomlTableNode? searchRootNode, IReadOnlyCollection<CsTomlString> comments)
    {
        var currentNode = searchRootNode ?? RootNode;
        var dotKeys = csTomlKey.DotKeysSpan;
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

            ExceptionHelper.ThrowNotTurnIntoTable(csTomlKey.DotKeysSpan.GetJoinName());
        }

        currentNode.AddKeyValue(lastKey, value, comments);
    }

    public void AddTableHeader(ArrayPoolList<CsTomlDotKey> csTomlKey, IReadOnlyCollection<CsTomlString> comments, out CsTomlTableNode? newNode)
    {
        var node = RootNode;
        var dotKeys = csTomlKey.DotKeysSpan;

        var addedNewNode = false;
        for (var i = 0; i < dotKeys.Length; i++)
        {
            var sectionKey = (CsTomlDotKey)dotKeys[i];
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
                ExceptionHelper.ThrowTableHeaderIsDefined(csTomlKey.DotKeysSpan.GetJoinName());
            }
            if (node!.IsArrayOfTablesHeaderDefinitionPosition)
            {
                ExceptionHelper.ThrowTableHeaderIsDefinedAsArrayOfTables(csTomlKey.DotKeysSpan.GetJoinName());
            }
            if (!node!.IsTableHeader)
            {
                ExceptionHelper.ThrowTableHeaderIsDefined(csTomlKey.DotKeysSpan.GetJoinName());
            }
        }

        node!.AddComment(comments);
        node.IsTableHeaderDefinitionPosition = true;
        newNode = node;
    }

    public void AddArrayOfTablesHeader(ArrayPoolList<CsTomlDotKey> csTomlKey, IReadOnlyCollection<CsTomlString> comments, out CsTomlTableNode? newNode)
    {
        var currentNode = RootNode;
        var dotKeys = csTomlKey.DotKeysSpan;

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
            ExceptionHelper.ThrowTheArrayOfTablesIsDefinedAsTable(csTomlKey.DotKeysSpan.GetJoinName());
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
                ExceptionHelper.ThrowTheArrayOfTablesIsDefinedAsTable(csTomlKey.DotKeysSpan.GetJoinName());
            }
        }
        var table = new CsTomlTable();
        (currentNode.Value as CsTomlArray)?.Add(table);
        currentNode.AddComment(comments);
        newNode = table.RootNode;
    }

    public bool TryGetTable(ReadOnlySpan<byte> tableHeader, ReadOnlySpan<byte> key, out CsTomlValue? value)
    {
        var currentNode = RootNode;
        if (currentNode!.TryGetChildNode(tableHeader, out var childNode) && childNode!.IsGroupingProperty)
        {
            if (childNode!.TryGetChildNode(key, out var valueNode))
            {
                value = valueNode!.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    public bool TryGetSubTable(ReadOnlySpan<byte> tableHeader, ReadOnlySpan<byte> key, out CsTomlValue? value)
    {
        var hit = false;
        CsTomlTableNode? currentNode = RootNode;
        foreach (var dottedKey in tableHeader.SplitSpan("."u8))
        {
            if (currentNode!.TryGetChildNode(dottedKey.Value, out var childNode) && childNode!.IsGroupingProperty)
            {
                hit = true;
                currentNode = childNode!;
            }
            else
            {
                value = default;
                return false;
            }
        }

        if (hit && currentNode!.IsGroupingProperty && currentNode!.TryGetChildNode(key, out var valueNode))
        {
            value = valueNode!.Value!;
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetSubTable(ReadOnlySpan<ByteArray> tableHeaderSpan, ReadOnlySpan<byte> key, out CsTomlValue? value)
    {
        var hit = false;
        CsTomlTableNode? currentNode = RootNode;
        for (int i = 0; i < tableHeaderSpan.Length; i++)
        {
            if (currentNode!.TryGetChildNode(tableHeaderSpan[i].value, out var childNode) && childNode!.IsGroupingProperty)
            {
                hit = true;
                currentNode = childNode!;
            }
            else
            {
                value = default;
                return false;
            }
        }

        if (hit && currentNode!.IsGroupingProperty && currentNode!.TryGetChildNode(key, out var valueNode))
        {
            value = valueNode!.Value;
            return true;
        }

        value = default;
        return false;
    }

    public CsTomlValue FindAsDottedKey(ReadOnlySpan<byte> dottedKeySpan)
    {
        var hit = false;
        var currentNode = RootNode;
        foreach (var dottedKey in dottedKeySpan.SplitSpan("."u8))
        {
            if (currentNode!.TryGetChildNode(dottedKey.Value, out var childNode))
            {
                hit = true;
                currentNode = childNode;
            }
        }

        if (hit && !currentNode!.IsGroupingProperty)
        {
            return currentNode.Value!;
        }

        return Empty;
    }

    public CsTomlValue FindAsDottedKey(ReadOnlySpan<ByteArray> dottedKeys)
    {
        var hit = false;
        var currentNode = RootNode;

        for (int i = 0; i < dottedKeys.Length; i++)
        {
            if (currentNode!.TryGetChildNode(dottedKeys[i].value, out var childNode))
            {
                hit = true;
                currentNode = childNode;
            }
        }

        if (hit && !currentNode!.IsGroupingProperty)
        {
            return currentNode.Value!;
        }

        return Empty;
    }

    public CsTomlValue FindAsKey(ReadOnlySpan<byte> keySpan)
    {
        var currentNode = RootNode;
        if (currentNode!.TryGetChildNode(keySpan, out var childNode) && !childNode!.IsGroupingProperty)
        {
            return childNode!.Value!;
        }
        return Empty;
    }

    public CsTomlValue FindArrayOfTableOrValueAsDotted(ReadOnlySpan<byte> keys)
    {
        var hit = false;
        var currentNode = RootNode;
        foreach (var key in keys.SplitSpan("."u8))
        {
            if (currentNode!.TryGetChildNode(key.Value, out var childNode))
            {
                hit = true;
                currentNode = childNode;
            }
        }

        if (hit && (!currentNode!.IsGroupingProperty || currentNode!.IsArrayOfTablesHeader))
        {
            return currentNode.Value!;
        }

        return Empty;
    }

    public CsTomlValue FindArrayOfTableOrValue(ReadOnlySpan<byte> keySpan)
    {
        var currentNode = RootNode;
        if (currentNode!.TryGetChildNode(keySpan, out var childNode) &&
            (!childNode!.IsGroupingProperty || childNode!.IsArrayOfTablesHeader))
        {
            return childNode!.Value!;
        }
        return Empty;
    }

    public CsTomlValue FindArrayOfTablesOrValue(ReadOnlySpan<ByteArray> keys)
    {
        var hit = false;
        var currentNode = RootNode;
        foreach (var key in keys)
        {
            if (currentNode!.TryGetChildNode(key.value, out var childNode))
            {
                hit = true;
                currentNode = childNode;
            }
        }

        if (hit && (!currentNode!.IsGroupingProperty || currentNode!.IsArrayOfTablesHeader))
        {
            return currentNode.Value!;
        }

        return Empty;
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
