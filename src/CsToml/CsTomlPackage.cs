using CsToml.Values;
using CsToml.Error;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CsToml;

[DebuggerDisplay("{table}")]
public partial class CsTomlPackage : 
    ICsTomlPackageCreator<CsTomlPackage>
{
    private readonly CsTomlTable table;

    public CsTomlPackageNode this[ReadOnlySpan<byte> key]
        => new(table[key]);

    public long LineNumber { get; internal set; }

    public static CsTomlPackage CreatePackage() => new();

    public CsTomlPackage()
    {
        table = new();
        LineNumber = 0;
    }
}
