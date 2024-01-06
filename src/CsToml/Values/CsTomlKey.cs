using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlKey: {DotKeyStrings}")]
internal sealed class CsTomlKey : CsTomlValue
{
    private List<CsTomlString> dotKeyStrings = [];

    public IReadOnlyList<CsTomlString> DotKeyStrings => dotKeyStrings;

    internal CsTomlKey(IReadOnlyList<CsTomlString> keys) : base(CsTomlType.String)
    {
        if (keys?.Count != 0)
        {
            dotKeyStrings.AddRange(keys!);
        }
    }

    public void Add(string key)
    {
        //dotKeyStrings.Add();
    }
}
