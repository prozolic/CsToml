using System.Diagnostics;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlKey: {DotKeys}")]
internal sealed class CsTomlKey : CsTomlValue
{
    private List<CsTomlString> dotKeys = [];

    public IReadOnlyList<CsTomlString> DotKeys => dotKeys;

    internal CsTomlKey() : base(CsTomlType.String){}

    internal CsTomlKey(IReadOnlyList<CsTomlString> keys) : base(CsTomlType.String)
    {
        if (keys?.Count != 0)
        {
            dotKeys.AddRange(keys!);
        }
    }

    public void Add(CsTomlString key)
        => dotKeys.Add(key);
}
