using Microsoft.CodeAnalysis;

namespace CsToml.Generator;

internal sealed class CollectionMetaData
{
    private static readonly HashSet<string> CollectionFullName = new HashSet<string>();
    private static readonly HashSet<string> CollectionInterfaceFullName = new HashSet<string>();

    static CollectionMetaData()
    {
        CollectionFullName.Add("global::System.Collections.Generic.List");
        CollectionFullName.Add("global::System.Collections.ObjectModel.Collection");

        CollectionInterfaceFullName.Add("global::System.Collections.IEnumerable");
        CollectionInterfaceFullName.Add("global::System.Collections.ICollection");
        CollectionInterfaceFullName.Add("global::System.Collections.IList");
        CollectionInterfaceFullName.Add("global::System.Collections.Generic.IEnumerable");
        CollectionInterfaceFullName.Add("global::System.Collections.Generic.ICollection");
        CollectionInterfaceFullName.Add("global::System.Collections.Generic.IReadOnlyCollection");
        CollectionInterfaceFullName.Add("global::System.Collections.Generic.IReadOnlyList");
    }

    public static bool IsCollection(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol nameType)
            return false;
        if (!nameType.IsGenericType)
            return false;

        var fullName = $"global::{nameType.ContainingNamespace.ToDisplayString()}.{nameType.Name}";

        return CollectionFullName.Contains(fullName) || CollectionInterfaceFullName.Contains(fullName);
    }

    public static bool FromArray(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol)
            return true;
        if (type is INamedTypeSymbol nameType)
        {
            var fullName = $"global::{nameType.ContainingNamespace.ToDisplayString()}.{nameType.Name}";
            return CollectionInterfaceFullName.Contains(fullName);
        }
        return false;
    }

}
