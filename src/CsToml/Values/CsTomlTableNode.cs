using CsToml.Debugger;
using CsToml.Error;
using CsToml.Utility;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace CsToml.Values;

[Flags]
internal enum TableNodeType : byte
{
    None                = 0,
    GroupingProperty    = 1,
    TableHeader         = 2,    
    TableArrayHeader    = 4,
}

internal static class TableNodeTypeExtensions
{
    public static void Add(ref TableNodeType target, TableNodeType flag)
    {
        target |= flag;
    }

    public static void Remove(ref TableNodeType target, TableNodeType flag)
    {
        target &= ~flag;
    }

    public static bool Has(this TableNodeType target, TableNodeType flag)
    {
        return (target & flag) == flag;
    }

}

[DebuggerTypeProxy(typeof(CsTomlTableNodeDebugView))]
[DebuggerDisplay("{Value}")]
internal class CsTomlTableNode
{
    internal Dictionary<Utf8FixString, CsTomlTableNode> Nodes { get; } = [];

    internal CsTomlValue? Value { get; set; }

    internal List<CsTomlString> Comments = [];

    private TableNodeType nodeType = TableNodeType.None;

    public bool IsGroupingProperty 
    {
        get => nodeType.Has(TableNodeType.GroupingProperty);
        set
        {
            if (value)
            {
                TableNodeTypeExtensions.Add(ref nodeType, TableNodeType.GroupingProperty);
            }
            else
            {
                TableNodeTypeExtensions.Remove(ref nodeType, TableNodeType.GroupingProperty);
            }
        }
    }

    public bool IsTableHeader 
    {
        get => nodeType.Has(TableNodeType.TableHeader);
        set
        {
            if (value)
            {
                TableNodeTypeExtensions.Add(ref nodeType, TableNodeType.TableHeader);
            }
            else
            {
                TableNodeTypeExtensions.Remove(ref nodeType, TableNodeType.TableHeader);
            }
        }
    }

    public bool IsTableArrayHeader 
    {
        get => nodeType.Has(TableNodeType.TableArrayHeader);
        set
        {
            if (value)
            {
                TableNodeTypeExtensions.Add(ref nodeType, TableNodeType.TableArrayHeader);
            }
            else
            {
                TableNodeTypeExtensions.Remove(ref nodeType, TableNodeType.TableArrayHeader);
            }
        }
    }

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

    internal void AddComment(IEnumerable<CsTomlString>? comment)
    {
        if (comment == null) return;
        Comments.AddRange(comment);
    }

    public bool TryAddValue(CsTomlString key, CsTomlValue value, IEnumerable<CsTomlString> comments)
    {
        if (!IsGroupingProperty || Nodes.ContainsKey(key.Value))
        {
            ExceptionHelper.ThrowKeyIsRedefined(key.Value);
            return false;
        }

        var newNode = new CsTomlTableNode() { Value = value };
        newNode.AddComment(comments);
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

