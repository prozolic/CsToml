using CsToml.Utility;
using CsToml.Values;
using System.Diagnostics;
using System.Xml.Linq;

namespace CsToml.Debugger;

[DebuggerDisplay("Key={Key}, Value={Value}")]
internal sealed class CsTomlTableNodeKeyValuePairDebugView(KeyValuePair<CsTomlString, CsTomlTableNode> pair)
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly KeyValuePair<CsTomlString, CsTomlTableNode> rawPair = pair;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public CsTomlString Key => rawPair.Key;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public CsTomlTableNode Value => rawPair.Value;

    public bool IsGroupingProperty => Value.IsGroupingProperty!;

    public bool IsTableHeader => Value.IsTableHeader!;

    public bool IsTableHeaderDefinitionPosition => Value.IsTableHeaderDefinitionPosition!;

    public bool IsArrayOfTablesHeader => Value.IsArrayOfTablesHeader!;

    public bool IsArrayOfTablesHeaderDefinitionPosition => Value.IsArrayOfTablesHeaderDefinitionPosition!;
}