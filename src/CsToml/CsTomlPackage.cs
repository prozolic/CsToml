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
    private readonly List<CsTomlException> exceptions;

    public CsTomlPackageNode this[ReadOnlySpan<byte> key]
        => new(table[key]);

    public long LineNumber { get; internal set; }

    public ReadOnlyCollection<CsTomlException> Exceptions => exceptions.AsReadOnly();

    public static CsTomlPackage CreatePackage() => new();

    public CsTomlPackage()
    {
        table = new();
        exceptions = [];
        LineNumber = 0;
    }
}
