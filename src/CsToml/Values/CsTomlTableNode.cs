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
    private readonly CsTomlTableNodeDictionary nodes = new();
    private readonly List<CsTomlString> comments = [];
    private CsTomlTableNodeType nodeType = CsTomlTableNodeType.None;

    internal IReadOnlyList<CsTomlString> Comments => comments; // Debug

    public int CommentCount => comments.Count;

    public ReadOnlySpan<CsTomlString> CommentSpan => CollectionsMarshal.AsSpan(comments);

    public CsTomlTableNodeDictionary.KeyValuePairEnumerator KeyValuePairs => new(nodes);

    public int NodeCount => nodes.Count;

    public CsTomlValue? Value { get; set; }

    public bool IsGroupingProperty 
    {
        get => nodeType.Has(CsTomlTableNodeType.GroupingProperty);
        set
        {
            if (value)
            {
                CsTomlTableNodeTypeExtensions.Add(ref nodeType, CsTomlTableNodeType.GroupingProperty);
            }
            else
            {
                CsTomlTableNodeTypeExtensions.Remove(ref nodeType, CsTomlTableNodeType.GroupingProperty);
            }
        }
    }

    public bool IsTableHeader 
    {
        get => nodeType.Has(CsTomlTableNodeType.TableHeaderProperty);
        set
        {
            if (value)
            {
                CsTomlTableNodeTypeExtensions.Add(ref nodeType, CsTomlTableNodeType.TableHeaderProperty);
            }
            else
            {
                CsTomlTableNodeTypeExtensions.Remove(ref nodeType, CsTomlTableNodeType.TableHeaderProperty);
            }
        }
    }

    public bool IsTableHeaderDefinitionPosition
    {
        get => nodeType.Has(CsTomlTableNodeType.TableHeaderDefinitionPosition);
        set
        {
            if (value)
            {
                CsTomlTableNodeTypeExtensions.Add(ref nodeType, CsTomlTableNodeType.TableHeaderDefinitionPosition);
            }
            else
            {
                CsTomlTableNodeTypeExtensions.Remove(ref nodeType, CsTomlTableNodeType.TableHeaderDefinitionPosition);
            }
        }
    }

    public bool IsArrayOfTablesHeader 
    {
        get => nodeType.Has(CsTomlTableNodeType.ArrayOfTablesHeaderProperty);
        set
        {
            if (value)
            {
                CsTomlTableNodeTypeExtensions.Add(ref nodeType, CsTomlTableNodeType.ArrayOfTablesHeaderProperty);
            }
            else
            {
                CsTomlTableNodeTypeExtensions.Remove(ref nodeType, CsTomlTableNodeType.ArrayOfTablesHeaderProperty);
            }
        }
    }

    public bool IsArrayOfTablesHeaderDefinitionPosition
    {
        get => nodeType.Has(CsTomlTableNodeType.ArrayOfTablesHeaderDefinitionPosition);
        set
        {
            if (value)
            {
                CsTomlTableNodeTypeExtensions.Add(ref nodeType, CsTomlTableNodeType.ArrayOfTablesHeaderDefinitionPosition);
            }
            else
            {
                CsTomlTableNodeTypeExtensions.Remove(ref nodeType, CsTomlTableNodeType.ArrayOfTablesHeaderDefinitionPosition);
            }
        }
    }

    public static CsTomlTableNode CreateGroupingPropertyNode()
    {
        var node = new CsTomlTableNode() { IsGroupingProperty = true};
#if DEBUG
        node.Value = new NodeCountDebuggerValue(node);
#else
        node.Value = CsTomlValue.Empty;
#endif
        return node;
    }

    internal void AddComment(IReadOnlyCollection<CsTomlString> comments)
    {
        if (comments.Count == 0) return;
        this.comments.AddRange(comments);
    }

    public void AddKeyValue(CsTomlString key, CsTomlValue value, IReadOnlyCollection<CsTomlString> comments)
    {
        if (!IsGroupingProperty || nodes.ContainsKey(key))
        {
            ExceptionHelper.ThrowKeyIsDefined(key);
        }

        var newNode = new CsTomlTableNode() { Value = value };
        newNode.AddComment(comments);
        nodes.TryAdd(key, newNode);
    }

    public bool TryAddGroupingPropertyNode(CsTomlString key, out CsTomlTableNode childNode)
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

    public bool TryGetChildNode(ReadOnlySpan<byte> key, out CsTomlTableNode? value)
    {
        var isBareKey = true;
        var firstEscapeSequenceIndex = 0;
        for (var i = 0; i < key.Length; i++)
        {
            isBareKey = CsTomlSyntax.IsBareKey(key[i]);
            if (!isBareKey && CsTomlSyntax.IsBackSlash(key[i]))
            {
                firstEscapeSequenceIndex = i;
                break;
            }
        }

        if (isBareKey)
        {
            if (Value is CsTomlInlineTable t)
            {
                return t.RootNode.nodes.TryGetValue(key, out value);
            }

            return nodes.TryGetValue(key, out value);
        }
        else
        {
            var bufferSize = key.Length;
            var reader = new Utf8Reader(key);
            using var bufferWriter = new ArrayPoolBufferWriter<byte>(bufferSize);
            var writer = new Utf8Writer(bufferWriter);

            writer.Write(key[..firstEscapeSequenceIndex]);
            reader.Skip(firstEscapeSequenceIndex + 1);

            if (reader.TryPeek(out var FirstCh))
            {
                var result = CsTomlString.TryFormatEscapeSequence(ref reader, ref writer, false, false);
                if (result == CsTomlString.EscapeSequenceResult.Failure)
                {
                    reader.Skip(1);
                }
            }

            while (reader.TryPeek(out var ch))
            {
                if (CsTomlSyntax.IsBackSlash(ch))
                {
                    var result = CsTomlString.TryFormatEscapeSequence(ref reader, ref writer, false, false);
                    if (result == CsTomlString.EscapeSequenceResult.Failure)
                    {
                        reader.Skip(1);
                    }
                    continue;
                }

                writer.Write(ch);
                reader.Skip(1);
            }

            if (Value is CsTomlInlineTable t2)
            {
                return t2.RootNode.nodes.TryGetValue(bufferWriter.WrittenSpan, out value);
            }

            return nodes.TryGetValue(bufferWriter.WrittenSpan, out value);
        }
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

    [DebuggerDisplay("NodeCount: {currentNode.NodeCount}")]
    private sealed class NodeCountDebuggerValue : CsTomlValue
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly CsTomlTableNode currentNode;

        internal NodeCountDebuggerValue(CsTomlTableNode node) : base(CsTomlType.None)
        {
            currentNode = node;
        }
    }
}

