using CsToml.Debugger;
using CsToml.Error;
using CsToml.Utility;
using CsToml.Values.Internal;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Values;

internal enum NodeStatus : byte
{
    Empty,
    NewAdd,
    Existed,
}

[DebuggerTypeProxy(typeof(TomlTableNodeDebugView))]
[DebuggerDisplay("{DebuggerValue}")]
internal sealed class TomlTableNode
{
    internal static readonly TomlTableNode Empty = new() { Value = TomlValue.Empty };

    private readonly TomlTableNodeDictionary? nodes;
    private List<TomlString>? comments;
    private TomlTableNodeType nodeType = TomlTableNodeType.None;

    public TomlValue? Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    }

    internal int NodeCount => nodes?.Count ?? 0;

    internal int CommentCount => comments?.Count ?? 0;

    internal ReadOnlySpan<TomlString> CommentSpan => comments != null ? CollectionsMarshal.AsSpan(comments) : ReadOnlySpan<TomlString>.Empty;

    internal TomlTableNodeDictionary.KeyValuePairEnumerator KeyValuePairs => new(nodes ?? Empty.nodes!);

    internal object DebuggerValue => (Value?.HasValue ?? false ? Value : nodes)!;

    internal bool IsGroupingProperty
    {
        get => (nodeType & TomlTableNodeType.GroupingProperty) == TomlTableNodeType.GroupingProperty;
        set
        {
            if (value)
            {
                nodeType |= TomlTableNodeType.GroupingProperty;
            }
            else
            {
                nodeType &= ~TomlTableNodeType.GroupingProperty;
            }
        }
    }

    internal bool IsTableHeader
    {
        get => (nodeType & TomlTableNodeType.TableHeaderProperty) == TomlTableNodeType.TableHeaderProperty;
        set
        {
            if (value)
            {
                nodeType |= TomlTableNodeType.TableHeaderProperty;
            }
            else
            {
                nodeType &= ~TomlTableNodeType.TableHeaderProperty;
            }
        }
    }

    internal bool IsTableHeaderDefinitionPosition
    {
        get => (nodeType & TomlTableNodeType.TableHeaderDefinitionPosition) == TomlTableNodeType.TableHeaderDefinitionPosition;
        set
        {
            if (value)
            {
                nodeType |= TomlTableNodeType.TableHeaderDefinitionPosition;
            }
            else
            {
                nodeType &= ~TomlTableNodeType.TableHeaderDefinitionPosition;
            }
        }
    }

    internal bool IsArrayOfTablesHeader
    {
        get => (nodeType & TomlTableNodeType.ArrayOfTablesHeaderProperty) == TomlTableNodeType.ArrayOfTablesHeaderProperty;
        set
        {
            if (value)
            {
                nodeType |= TomlTableNodeType.ArrayOfTablesHeaderProperty;
            }
            else
            {
                nodeType &= ~TomlTableNodeType.ArrayOfTablesHeaderProperty;
            }
        }
    }

    internal bool IsArrayOfTablesHeaderDefinitionPosition
    {
        get => (nodeType & TomlTableNodeType.ArrayOfTablesHeaderDefinitionPosition) == TomlTableNodeType.ArrayOfTablesHeaderDefinitionPosition;
        set
        {
            if (value)
            {
                nodeType |= TomlTableNodeType.ArrayOfTablesHeaderDefinitionPosition;
            }
            else
            {
                nodeType &= ~TomlTableNodeType.ArrayOfTablesHeaderDefinitionPosition;
            }
        }
    }

    internal TomlTableNode()
    {
        nodes = new();
    }

    internal TomlTableNode(TomlValue value)
    {
        Value = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ReserveNodeCapacity(int estimatedCount)
        => nodes?.EnsureCapacity(estimatedCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Span<TomlString> SetCommentCount(int commentCount)
    {
        var count = (this.comments ??= new List<TomlString>(commentCount)).Count;
        CollectionsMarshal.SetCount(this.comments, count + commentCount);
        return CollectionsMarshal.AsSpan(this.comments).Slice(count, commentCount);
    }

    internal TomlTableNode GetOrAddArrayOfTableHeaderKeyNode(TomlDottedKey key, bool lastNode, out bool newNode)
    {
        newNode = false;

        if (this!.TryGetOrAddChildNode(key, out var childNode) == NodeStatus.NewAdd)
        {
            newNode = true;
            childNode.IsArrayOfTablesHeader = true;
            childNode.IsTableHeader = !lastNode;
            return childNode;
        }

        if (childNode!.IsArrayOfTablesHeaderDefinitionPosition)
        {
            if (lastNode)
            {
                return childNode;
            }
            else
            {
                var tableHeaderArrayValue = (childNode!.Value as TomlArray)?.LastValue;
                return (tableHeaderArrayValue as TomlTable)?.RootNode!;
            }
        }
        if (childNode!.IsGroupingProperty)
        {
            return childNode;
        }

        ExceptionHelper.ThrowIncorrectTomlFormat();
        return default;
    }

    internal TomlTableNode AddArrayOfTableHeaderKeyLastNode(TomlDottedKey key, out TomlTableNode commentNode)
    {
        var node = GetOrAddArrayOfTableHeaderKeyNode(key, true, out var newNode);

        if (node!.IsTableHeader)
        {
            ExceptionHelper.ThrowTheArrayOfTablesIsDefinedAsTable(key.ToString());
        }

        if (newNode)
        {
            node.Value = new TomlArray();
            node.IsArrayOfTablesHeader = true;
            node.IsArrayOfTablesHeaderDefinitionPosition = true;
        }
        else
        {
            if (!node!.IsArrayOfTablesHeaderDefinitionPosition)
            {
                ExceptionHelper.ThrowTheArrayOfTablesIsDefinedAsTable(key.ToString());
            }
        }
        var table = new TomlTable();
        (node.Value as TomlArray)?.Add(table);
        commentNode = node;
        return table.RootNode;
    }


    internal TomlTableNode GetOrAddTableHeaderKeyNode(TomlDottedKey key, out bool newNode)
    {
        if (this!.TryGetOrAddChildNode(key, out var childNode) == NodeStatus.NewAdd)
        {
            newNode = true;
            childNode.IsTableHeader = true;
            return childNode;
        }

        newNode = false;
        if (childNode!.IsArrayOfTablesHeaderDefinitionPosition)
        {
            TomlValue tableHeaderArrayValue = (childNode!.Value as TomlArray)?.LastValue!;
            return (tableHeaderArrayValue as TomlTable)!.RootNode;
        }

        if (childNode!.IsGroupingProperty)
        {
            return childNode;
        }

        // key is already defined.
        ExceptionHelper.ThrowKeyIsDefined(key);
        return default;

    }

    internal TomlTableNode AddTableHeaderKeyLastNode(TomlDottedKey key)
    {
        var node = GetOrAddTableHeaderKeyNode(key, out var newNode);

        if (!newNode)
        {
            if (node!.IsTableHeaderDefinitionPosition)
            {
                ExceptionHelper.ThrowTableHeaderIsDefined(key.ToString());
            }
            if (node!.IsArrayOfTablesHeaderDefinitionPosition)
            {
                ExceptionHelper.ThrowTableHeaderIsDefinedAsArrayOfTables(key.ToString());
            }
            if (!node!.IsTableHeader)
            {
                ExceptionHelper.ThrowTableHeaderIsDefined(key.ToString());
            }
        }

        node!.IsTableHeaderDefinitionPosition = true;
        return node;
    }

    internal TomlTableNode GetOrAddKeyNode(TomlDottedKey key)
    {
        if (this.TryGetOrAddChildNode(key, out var childNode) == NodeStatus.NewAdd)
        {
            return childNode;
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
            return childNode;
        }

        ExceptionHelper.ThrowKeyIsDefined(key);
        return default;
    }

    internal TomlTableNode AddKeyValueNode(TomlDottedKey key, TomlValue value)
    {
        var newNode = new TomlTableNode(value);
        if (!this.IsGroupingProperty || !(this.nodes?.TryAdd(key, newNode) ?? false))
        {
            ExceptionHelper.ThrowKeyIsDefined(key);
        }

        return newNode;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal NodeStatus TryGetOrAddChildNode(TomlDottedKey key, out TomlTableNode getOrAddChildNode)
    {
        if (nodes == null)
        {
            getOrAddChildNode = Empty;
            return NodeStatus.Empty;
        }

        var result = nodes.GetOrAddIfNotFound(key);
        if (result.IsExistingValueFound)
        {
            getOrAddChildNode = result.ExistingValue!;
            return NodeStatus.Existed;
        }


        getOrAddChildNode = result.AddedValue!;
        return NodeStatus.NewAdd;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryGetChildNode(ReadOnlySpan<byte> key, out TomlTableNode? childNode)
    {
        TomlTableNodeDictionary? nodes =
            Value is TomlInlineTable inlineTable ?
            inlineTable.RootNode.nodes :
            this.nodes;

        if (nodes == null)
        {
            childNode = null;
            return false;
        }

        if (nodes.TryGetValue(key, out childNode))
        {
            return true;
        }

        if (!key.Contains(TomlCodes.Symbol.BACKSLASH))
        {
            return false;
        }

        return TryGetChildNodeSlow(nodes, key, out childNode);
    }

    private bool TryGetChildNodeSlow(TomlTableNodeDictionary nodes, ReadOnlySpan<byte> key, out TomlTableNode? childNode)
    {
        var reader = new Utf8SequenceReader(key);
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            while (reader.TryPeek(out var ch))
            {
                if (TomlCodes.IsBackSlash(ch))
                {
                    reader.Advance(1);

                    if (TomlCodes.TryParseEscapeSequence(
                        ref reader, 
                        bufferWriter, 
                        multiLine: false, 
                        supportsEscapeSequenceE: true, 
                        supportsEscapeSequenceX: true,
                        throwError: false) == EscapeSequenceResult.Failure)
                    {
                        childNode = default;
                        return false;
                    }
                    continue;
                }

                bufferWriter.Write(ch);
                reader.Advance(1);
            }

            // search Quoted keys
            return nodes.TryGetValue(bufferWriter.WrittenSpan, out childNode);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }

    public IDictionary<object, object> GetDictionary()
    {
        if (this.NodeCount == 0)
        {
            if (Value is TomlTable table)
            {
                return table.GetDictionary();
            }
            else if (Value is TomlInlineTable inlineTable)
            {
                return inlineTable.GetDictionary();
            }
            return new Dictionary<object, object>();
        }
        else
        {
            var dictionary = new Dictionary<object, object>(this.NodeCount);

            foreach ((var key, var node) in KeyValuePairs)
            {
                if (node.Value!.HasValue)
                {
                    dictionary.Add(key.Utf16String, node.Value!.GetObject());
                }
                else
                {
                    dictionary.Add(key.Utf16String, node.GetDictionary());
                }
            }

            return dictionary;
        }
    }

    public bool TryGetDictionary(out IDictionary<object, object> value)
    {
        try
        {
            value = GetDictionary();
            return true;
        }
        catch (CsTomlException)
        {
            value = default!;
            return false;
        }
    }
}

