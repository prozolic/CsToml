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
    public Dictionary<Utf8FixString, CsTomlTableNode> Nodes { get; } = [];

    public CsTomlValue? Value { get; internal set; }

    public bool IsGroupingProperty { get; private set; }

    public bool IsTableHeader { get; internal set; }

    public bool IsTableArrayHeader { get; internal set; }

    public CsTomlTableNode this[ReadOnlySpan<byte> key] => Nodes[new Utf8FixString(key)];

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

    public bool TryAddValue(CsTomlString key, CsTomlValue value)
    {
        if (!IsGroupingProperty || Nodes.ContainsKey(key.Value))
        {
            ExceptionHelper.ThrowKeyIsRedefined(key.Value);
            return false;
        }

        var newNode = new CsTomlTableNode() { Value = value };
        Nodes.Add(key.Value, newNode);
        return true;
    }

    public bool TryGetOrAddGroupingPropertyNode(CsTomlString key, out CsTomlTableNode childNode)
    {
        if (Nodes.TryGetValue(key.Value, out var addedChildNode))
        {
            childNode = addedChildNode;
            return false;
        }

        childNode = CreateGroupingPropertyNode();
        Nodes.Add(key.Value, childNode);
        return true;
    }

    public bool TryGetChildNode(ReadOnlySpan<byte> key, out CsTomlTableNode? value)
    {
        var utf8Key = new Utf8FixString(key);
        if (Value is CsTomlInlineTable table)
        {
            return table.RootNode.Nodes.TryGetValue(utf8Key, out value);
        }

        return Nodes.TryGetValue(utf8Key, out value);
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

