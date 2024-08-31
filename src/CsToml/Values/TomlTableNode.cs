using CsToml.Debugger;
using CsToml.Error;
using CsToml.Values.Internal;
using CsToml.Utility;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Reflection.Metadata;
using System.Collections.Generic;

namespace CsToml.Values;

internal enum NodeStatus : byte
{
    Empty,
    NewAdd,
    Existed,
}

[DebuggerTypeProxy(typeof(TomlTableNodeDebugView))]
[DebuggerDisplay("{Value}")]
internal class TomlTableNode
{
    internal static readonly TomlTableNode Empty = new() { Value = TomlValue.Empty};

    private readonly TomlTableNodeDictionary? nodes;
    private List<TomlString>? comments;
    private TomlTableNodeType nodeType = TomlTableNodeType.None;

    public TomlValue? Value { get; set; }

    internal int NodeCount => nodes?.Count ?? 0;

    internal IReadOnlyList<TomlString>? Comments => comments; // Debug

    internal int CommentCount => comments?.Count ?? 0;

    internal ReadOnlySpan<TomlString> CommentSpan => comments != null ? CollectionsMarshal.AsSpan(comments) : ReadOnlySpan<TomlString>.Empty;

    internal TomlTableNodeDictionary.KeyValuePairEnumerator KeyValuePairs => new(nodes ?? Empty.nodes!);

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TomlTableNode CreateGroupingPropertyNode()
    {
        var node = new TomlTableNode() { IsGroupingProperty = true};
#if DEBUG
        node.Value = new NodeCountDebuggerValue(node);
#else
        node.Value = TomlValue.Empty;
#endif
        return node;
    }

    private TomlTableNode()
    {
        nodes = new();
    }

    private TomlTableNode(TomlValue value)
    {
        Value = value;
    }

    internal void AddComment(IReadOnlyCollection<TomlString> comments)
    {
        if (this.comments == null)
        {
            this.comments = new List<TomlString>(comments.Count);
        }
        this.comments.AddRange(comments);
    }

    internal TomlTableNode AddKeyValue(TomlDotKey key, TomlValue value, IReadOnlyCollection<TomlString>? comments)
    {
        var newNode = new TomlTableNode(value);
        if (comments?.Count > 0)
        {
            newNode.AddComment(comments);
        }
        
        if (!IsGroupingProperty || !(nodes?.TryAdd(key, newNode) ?? false))
        {
            ExceptionHelper.ThrowKeyIsDefined(key);
        }

        return newNode;
    }

    internal NodeStatus TryGetOrAddChildNode(TomlDotKey key, out TomlTableNode getOrAddChildNode)
    {
        if (nodes == null)
        {
            getOrAddChildNode = Empty;
            return NodeStatus.Empty;
        }
        if (nodes.TryGetValueOrAdd(key, CreateGroupingPropertyNode, out var existingNode, out var newNode))
        {
            getOrAddChildNode = existingNode!;
            return NodeStatus.Existed;
        }

        getOrAddChildNode = newNode!;
        return NodeStatus.NewAdd;
    }

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

        var reader = new Utf8SequenceReader(key);
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            while (reader.TryPeek(out var ch))
            {
                if (TomlCodes.IsBackSlash(ch))
                {
                    reader.Advance(1);
                    if (TomlCodes.TryParseEscapeSequence(ref reader, bufferWriter, false, false) == EscapeSequenceResult.Failure)
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

    public IDictionary<string, object?> GetDictionary()
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
            return new Dictionary<string, object?>();
        }
        else
        {
            var dictionary = new Dictionary<string, object?>(this.NodeCount);

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

    public bool TryGetDictionary(out IDictionary<string, object?> value)
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

    [DebuggerDisplay("NodeCount = {currentNode.NodeCount}")]
    private sealed class NodeCountDebuggerValue : TomlValue
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly TomlTableNode currentNode;

        internal NodeCountDebuggerValue(TomlTableNode node) : base()
        {
            currentNode = node;
        }
    }
}

