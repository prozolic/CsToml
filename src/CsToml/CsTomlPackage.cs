using CsToml.Values;
using CsToml.Error;
using System.Collections.ObjectModel;

namespace CsToml;

public partial class CsTomlPackage : 
    ICsTomlPackageCreator<CsTomlPackage>
{
    private readonly CsTomlTable table;
    private readonly List<CsTomlException> exceptions;

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

