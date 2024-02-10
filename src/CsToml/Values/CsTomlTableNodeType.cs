
namespace CsToml.Values;

[Flags]
internal enum CsTomlTableNodeType : byte
{
    None = 0,
    GroupingProperty = 1 << 0,
    TableHeaderProperty = 1 << 1,
    TableHeaderDefinitionPosition = 1 << 2,
    TableArrayHeaderProperty = 1 << 3,
    TableArrayHeaderDefinitionPosition = 1 << 4,
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
