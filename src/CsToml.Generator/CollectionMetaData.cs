using Microsoft.CodeAnalysis;

namespace CsToml.Generator;

internal sealed class CollectionMetaData
{
    private static readonly HashSet<string> CollectionNames = new HashSet<string>();

    static CollectionMetaData()
    {
        CollectionNames.Add("global::System.Collections.Generic.List");
        CollectionNames.Add("global::System.Collections.ObjectModel.ReadOnlyCollection");
        CollectionNames.Add("global::System.Collections.ObjectModel.Collection");
    }

    public static bool IsGenericCollection(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol nameType)
            return false;
        if (!nameType.IsGenericType)
            return false;

        return CollectionNames.Contains($"global::{nameType.ContainingNamespace.ToDisplayString()}.{nameType.Name}");
    }

}