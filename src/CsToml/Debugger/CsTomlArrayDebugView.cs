
using CsToml.Values;
using System.Diagnostics;

namespace CsToml.Debugger;

internal sealed class CsTomlArrayDebugView(CsTomlArray csTomlArray)
{
    private readonly CsTomlArray array = csTomlArray;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public CsTomlValue[] Items => array.Values.ToArray();
}


