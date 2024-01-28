
namespace CsToml.Values;

[Flags]
internal enum CsTomlTableNodeType : byte
{
    None = 0,
    GroupingProperty = 1,
    TableHeader = 2,
    TableArrayHeader = 4,
}

internal static class CsTomlTableNodeTypeExtensions
{
    public static void Add(ref CsTomlTableNodeType target, CsTomlTableNodeType flag)
    {
        target |= flag;
    }

    public static void Remove(ref CsTomlTableNodeType target, CsTomlTableNodeType flag)
    {
        target &= ~flag;
    }

    public static bool Has(this CsTomlTableNodeType target, CsTomlTableNodeType flag)
    {
        return (target & flag) == flag;
    }

}
