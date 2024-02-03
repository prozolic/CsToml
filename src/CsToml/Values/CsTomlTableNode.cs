using CsToml.Debugger;
using CsToml.Error;
using CsToml.Utility;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Values;

[DebuggerTypeProxy(typeof(CsTomlTableNodeDebugView))]
[DebuggerDisplay("{Value}")]
internal class CsTomlTableNode
{
    private readonly Dictionary<CsTomlString, CsTomlTableNode> nodes = [];
    private readonly List<CsTomlString> tomlComments = [];

    private CsTomlTableNodeType nodeType = CsTomlTableNodeType.None;

    internal IReadOnlyList<CsTomlString> Comments => tomlComments;

    internal ReadOnlySpan<CsTomlString> CommentsSpan => CollectionsMarshal.AsSpan(tomlComments);

    internal IReadOnlyDictionary<CsTomlString, CsTomlTableNode> Nodes => nodes;

    internal CsTomlValue? Value { get; set; }

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
        get => nodeType.Has(CsTomlTableNodeType.TableHeader);
        set
        {
            if (value)
            {
                CsTomlTableNodeTypeExtensions.Add(ref nodeType, CsTomlTableNodeType.TableHeader);
            }
            else
            {
                CsTomlTableNodeTypeExtensions.Remove(ref nodeType, CsTomlTableNodeType.TableHeader);
            }
        }
    }

    public bool IsTableArrayHeader 
    {
        get => nodeType.Has(CsTomlTableNodeType.TableArrayHeader);
        set
        {
            if (value)
            {
                CsTomlTableNodeTypeExtensions.Add(ref nodeType, CsTomlTableNodeType.TableArrayHeader);
            }
            else
            {
                CsTomlTableNodeTypeExtensions.Remove(ref nodeType, CsTomlTableNodeType.TableArrayHeader);
            }
        }
    }

    //public CsTomlTableNode this[ReadOnlySpan<byte> key] => Nodes[new Utf8FixString(key)];

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
        tomlComments.AddRange(comments);
    }

    public void AddKeyValue(CsTomlString key, CsTomlValue value, IReadOnlyCollection<CsTomlString> comments)
    {
        if (!IsGroupingProperty || nodes.ContainsKey(key))
        {
            ExceptionHelper.ThrowKeyIsRedefined(key);
        }

        var newNode = new CsTomlTableNode() { Value = value };
        newNode.AddComment(comments);
        nodes.Add(key, newNode);
    }

    public bool TryGetOrAddGroupingPropertyNode(CsTomlString key, out CsTomlTableNode childNode)
    {
        if (nodes.TryGetValue(key, out var addedChildNode))
        {
            childNode = addedChildNode;
            return false;
        }

        childNode = CreateGroupingPropertyNode();
        nodes.Add(key, childNode);
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

        var searchKey = key;
        if (isBareKey)
        {
            var bareKeystring = new CsTomlString(searchKey, CsTomlString.CsTomlStringType.Unquoted);
            if (Value is CsTomlInlineTable t)
            {
                return t.RootNode.nodes.TryGetValue(bareKeystring, out value);
            }

            return nodes.TryGetValue(bareKeystring, out value);
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

            var keystring = new CsTomlString(bufferWriter.WrittenSpan, CsTomlString.CsTomlStringType.Basic);
            if (Value is CsTomlInlineTable t2)
            {
                return t2.RootNode.nodes.TryGetValue(keystring, out value);
            }

            return nodes.TryGetValue(keystring, out value);
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

    [DebuggerDisplay("NodeCount: {currentNode.Nodes.Count}")]
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

