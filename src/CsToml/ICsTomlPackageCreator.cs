
namespace CsToml;

public interface ICsTomlPackageCreator<TPackage>
    where TPackage : CsTomlPackage
{
    static abstract TPackage CreatePackage();
}

