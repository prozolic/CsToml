
using CsToml.Values;
using System.Diagnostics;

namespace CsToml.Debugger;

internal sealed class TomlArrayDebugView(TomlArray csTomlArray)
{
    private readonly TomlArray array = csTomlArray;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public TomlValue[] Items => array.Values.ToArray();
}


