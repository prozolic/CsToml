using CsToml.Debugger;
using CsToml.Error;
using CsToml.Values.Internal;
using CsToml.Utility;
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

[DebuggerTypeProxy(typeof(CsTomlTableNodeDebugView))]
[DebuggerDisplay("{Value}")]
internal class CsTomlTableNode
{
    internal static readonly CsTomlTableNode Empty = new() { Value = CsTomlValue.Empty};

    private readonly CsTomlTableNodeDictionary? nodes;
    private List<CsTomlString>? comments;
    private CsTomlTableNodeType nodeType = CsTomlTableNodeType.None;

    public CsTomlValue? Value { get; set; }

    internal int NodeCount => nodes?.Count ?? 0;

    internal IReadOnlyList<CsTomlString>? Comments => comments; // Debug

    internal int CommentCount => comments?.Count ?? 0;

    internal ReadOnlySpan<CsTomlString> CommentSpan => comments != null ? CollectionsMarshal.AsSpan(comments) : ReadOnlySpan<CsTomlString>.Empty;

    internal CsTomlTableNodeDictionary.KeyValuePairEnumerator KeyValuePairs => new(nodes ?? Empty.nodes!);

    internal bool IsGroupingProperty 
    {
        get => (nodeType & CsTomlTableNodeType.GroupingProperty) == CsTomlTableNodeType.GroupingProperty;
        set
        {
            if (value)
            {
                nodeType |= CsTomlTableNodeType.GroupingProperty;
            }
            else
            {
                nodeType &= ~CsTomlTableNodeType.GroupingProperty;
            }
        }
    }

    internal bool IsTableHeader 
    {
        get => (nodeType & CsTomlTableNodeType.TableHeaderProperty) == CsTomlTableNodeType.TableHeaderProperty;
        set
        {
            if (value)
            {
                nodeType |= CsTomlTableNodeType.TableHeaderProperty;
            }
            else
            {
                nodeType &= ~CsTomlTableNodeType.TableHeaderProperty;
            }
        }
    }

    internal bool IsTableHeaderDefinitionPosition
    {
        get => (nodeType & CsTomlTableNodeType.TableHeaderDefinitionPosition) == CsTomlTableNodeType.TableHeaderDefinitionPosition;
        set
        {
            if (value)
            {
                nodeType |= CsTomlTableNodeType.TableHeaderDefinitionPosition;
            }
            else
            {
                nodeType &= ~CsTomlTableNodeType.TableHeaderDefinitionPosition;
            }
        }
    }

    internal bool IsArrayOfTablesHeader 
    {
        get => (nodeType & CsTomlTableNodeType.ArrayOfTablesHeaderProperty) == CsTomlTableNodeType.ArrayOfTablesHeaderProperty;
        set
        {
            if (value)
            {
                nodeType |= CsTomlTableNodeType.ArrayOfTablesHeaderProperty;
            }
            else
            {
                nodeType &= ~CsTomlTableNodeType.ArrayOfTablesHeaderProperty;
            }
        }
    }

    internal bool IsArrayOfTablesHeaderDefinitionPosition
    {
        get => (nodeType & CsTomlTableNodeType.ArrayOfTablesHeaderDefinitionPosition) == CsTomlTableNodeType.ArrayOfTablesHeaderDefinitionPosition;
        set
        {
            if (value)
            {
                nodeType |= CsTomlTableNodeType.ArrayOfTablesHeaderDefinitionPosition;
            }
            else
            {
                nodeType &= ~CsTomlTableNodeType.ArrayOfTablesHeaderDefinitionPosition;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CsTomlTableNode CreateGroupingPropertyNode()
    {
        var node = new CsTomlTableNode() { IsGroupingProperty = true};
#if DEBUG
        node.Value = new NodeCountDebuggerValue(node);
#else
        node.Value = CsTomlValue.Empty;
#endif
        return node;
    }

    private CsTomlTableNode()
    {
        nodes = new();
    }

    private CsTomlTableNode(CsTomlValue value)
    {
        Value = value;
    }

    internal void AddComment(IReadOnlyCollection<CsTomlString> comments)
    {
        if (this.comments == null)
        {
            this.comments = new List<CsTomlString>(comments.Count);
        }
        this.comments.AddRange(comments);
    }

    internal CsTomlTableNode AddKeyValue(CsTomlDotKey key, CsTomlValue value, IReadOnlyCollection<CsTomlString> comments)
    {
        var newNode = new CsTomlTableNode(value);
        if (comments.Count > 0)
        {
            newNode.AddComment(comments);
        }
        
        if (!IsGroupingProperty || !(nodes?.TryAdd(key, newNode) ?? false))
        {
            ExceptionHelper.ThrowKeyIsDefined(key);
        }

        return newNode;
    }

    internal NodeStatus TryGetOrAddChildNode(CsTomlDotKey key, out CsTomlTableNode getOrAddChildNode)
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

    internal bool TryGetChildNode(ReadOnlySpan<byte> key, out CsTomlTableNode? childNode)
    {
        var nodes = this.nodes;
        if (Value is CsTomlInlineTable t)
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

        var bufferSize = key.Length;
        var reader = new Utf8Reader(key);
        var bufferWriter = new ArrayPoolBufferWriter<byte>(bufferSize);
        using var _ = bufferWriter;
        var utf8BufferWriter = new Utf8BufferWriter<ArrayPoolBufferWriter<byte>>(ref bufferWriter);

        while (reader.TryPeek(out var ch))
        {
            if (CsTomlSyntax.IsBackSlash(ch))
            {
                reader.Advance(1);
                if (CsTomlString.TryFormatEscapeSequence(ref reader, ref bufferWriter, false, false) == CsTomlString.EscapeSequenceResult.Failure)
                {
                    childNode = default;
                    return false;
                }
                utf8BufferWriter.Flush();
                continue;
            }

            utf8BufferWriter.Write(ch);
            reader.Advance(1);
        }

        // search Quoted keys
        return nodes.TryGetValue(bufferWriter.WrittenSpan, out childNode);
    }

    [DebuggerDisplay("NodeCount = {currentNode.NodeCount}")]
    private sealed class NodeCountDebuggerValue : CsTomlValue
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly CsTomlTableNode currentNode;

        internal NodeCountDebuggerValue(CsTomlTableNode node) : base()
        {
            currentNode = node;
        }
    }
}

