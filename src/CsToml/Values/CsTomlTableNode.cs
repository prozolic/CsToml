using CsToml.Debugger;
using CsToml.Error;
using CsToml.Values.Internal;
using CsToml.Utility;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Values;

[DebuggerTypeProxy(typeof(CsTomlTableNodeDebugView))]
[DebuggerDisplay("{Value}")]
internal class CsTomlTableNode
{
    internal static readonly CsTomlTableNode Empty = new() { Value = CsTomlValue.Empty};

    private readonly CsTomlTableNodeDictionary nodes = new();
    private readonly List<CsTomlString> comments = [];
    private CsTomlTableNodeType nodeType = CsTomlTableNodeType.None;

    public CsTomlValue? Value { get; set; }

    internal int NodeCount => nodes?.Count ?? 0;

    internal IReadOnlyList<CsTomlString> Comments => comments; // Debug

    internal int CommentCount => comments.Count;

    internal ReadOnlySpan<CsTomlString> CommentSpan => CollectionsMarshal.AsSpan(comments);

    internal CsTomlTableNodeDictionary.KeyValuePairEnumerator KeyValuePairs => new(nodes);

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

    internal CsTomlTableNode() {}

    internal void AddComment(IReadOnlyCollection<CsTomlString> comments)
        => this.comments.AddRange(comments);

    internal void AddKeyValue(CsTomlDotKey key, CsTomlValue value, IReadOnlyCollection<CsTomlString> comments)
    {
        if (!IsGroupingProperty || nodes.ContainsKey(key))
        {
            ExceptionHelper.ThrowKeyIsDefined(key);
        }

        var newNode = new CsTomlTableNode() { Value = value };
        if (comments.Count > 0)
        {
            newNode.AddComment(comments);
        }
        nodes.TryAdd(key, newNode);
    }

    internal bool TryAddGroupingPropertyNode(CsTomlDotKey key, out CsTomlTableNode childNode)
    {
        if (nodes.TryGetValue(key, out var addedChildNode))
        {
            childNode = addedChildNode!;
            return false;
        }

        childNode = CreateGroupingPropertyNode();
        nodes.TryAdd(key, childNode);
        return true;
    }

    internal bool TryGetChildNode(ReadOnlySpan<byte> key, out CsTomlTableNode? value)
    {
        var nodes = this.nodes;
        if (Value is CsTomlInlineTable t)
        {
            nodes = t.RootNode.nodes;
        }

        if (nodes.TryGetValue(key, out value))
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
                    value = default;
                    return false;
                }
                utf8BufferWriter.Flush();
                continue;
            }

            utf8BufferWriter.Write(ch);
            reader.Advance(1);
        }

        // search Quoted keys
        return nodes.TryGetValue(bufferWriter.WrittenSpan, out value);
    }

    private bool TryGetChildNode(ReadOnlySpan<char> keySpan, out CsTomlTableNode? value)
    {
        unsafe
        {
            ref var keySpanRef = ref MemoryMarshal.GetReference(keySpan);
            fixed (char* p = &keySpanRef)
            {
                var key = string.Create(keySpan.Length, ((IntPtr)p, keySpan.Length), static (chars, args) =>
                {
                    var keySpanRef = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<char>((char*)args.Item1), args.Length);
                    for (var i = 0; i < chars.Length; i++)
                    {
                        chars[i] = keySpanRef[i];
                    }
                });

                return TryGetChildNode(key, out value);
            }
        };
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

