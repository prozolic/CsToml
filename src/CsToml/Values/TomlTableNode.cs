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
    internal Span<TomlString> SetCommentCount(int commentCount)
    {
        if (this.comments == null)
        {
            this.comments = new List<TomlString>(commentCount);
            CollectionsMarshal.SetCount(this.comments, commentCount);
            return CollectionsMarshal.AsSpan(this.comments).Slice(0, commentCount);
        }
        else
        {
            var currentCount = this.comments.Count;
            CollectionsMarshal.SetCount(this.comments, this.comments.Count + commentCount);
            return CollectionsMarshal.AsSpan(this.comments).Slice(currentCount, commentCount);
        }
    }


    internal TomlTableNode AddKeyValue(ReadOnlySpan<TomlDottedKey> dotKeys, TomlValue value)
    {
        var currentNode = this;
        var lastKey = dotKeys[^1];

        for (var i = 0; i < dotKeys.Length - 1; i++)
        {
            var sectionKey = dotKeys[i];
            if (currentNode.TryGetOrAddChildNode(sectionKey, out var childNode) == NodeStatus.NewAdd)
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

            ExceptionHelper.ThrowNotTurnIntoTable(dotKeys.GetJoinName());
        }

        var newNode = new TomlTableNode(value);
        if (!currentNode.IsGroupingProperty || !(currentNode.nodes?.TryAdd(lastKey, newNode) ?? false))
        {
            ExceptionHelper.ThrowKeyIsDefined(lastKey);
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
        if (nodes.TryGetValueOrAdd(key, out var existingNode, out var newNode))
        {
            getOrAddChildNode = existingNode!;
            return NodeStatus.Existed;
        }

        getOrAddChildNode = newNode!;
        return NodeStatus.NewAdd;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryGetChildNode(ReadOnlySpan<byte> key, out TomlTableNode? childNode)
    {
        var nodes = this.nodes;
        if (Value is TomlInlineTable t)
        {
            nodes = t.RootNode.nodes;
        }

        if (nodes == null)
        {
            childNode = null;
            return false;
        }

        if (nodes.TryGetValue(key, out childNode))
        {
            return true;
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
                    if (TryGetChildNode(key.Value, out var value))
                    {
                        dictionary.Add(key.Utf16String, value!.GetDictionary());
                    }
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

