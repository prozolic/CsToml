using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace CsToml.Generator;

internal sealed class CollectionMetaData
{
    private static readonly HashSet<string> CollectionFullName = new HashSet<string>();
    private static readonly HashSet<string> CollectionInterfaceFullName = new HashSet<string>();

    static CollectionMetaData()
    {
        CollectionFullName.Add("global::System.Collections.Generic.List");
        CollectionFullName.Add("global::System.Collections.Generic.Stack");
        CollectionFullName.Add("global::System.Collections.Generic.HashSet");
        CollectionFullName.Add("global::System.Collections.Generic.SortedSet");
        CollectionFullName.Add("global::System.Collections.Generic.Queue");
        CollectionFullName.Add("global::System.Collections.Generic.LinkedList");
        CollectionFullName.Add("global::System.Collections.Generic.PriorityQueue");
        CollectionFullName.Add("global::System.Collections.Concurrent.ConcurrentQueue");
        CollectionFullName.Add("global::System.Collections.Concurrent.ConcurrentStack");
        CollectionFullName.Add("global::System.Collections.Concurrent.ConcurrentBag");
        CollectionFullName.Add("global::System.Collections.Concurrent.BlockingCollection");
        CollectionFullName.Add("global::System.Collections.ObjectModel.ReadOnlyCollection");

        CollectionInterfaceFullName.Add("global::System.Collections.IEnumerable");
        CollectionInterfaceFullName.Add("global::System.Collections.ICollection");
        CollectionInterfaceFullName.Add("global::System.Collections.IList");
        CollectionInterfaceFullName.Add("global::System.Collections.Generic.IEnumerable");
        CollectionInterfaceFullName.Add("global::System.Collections.Generic.ICollection");
        CollectionInterfaceFullName.Add("global::System.Collections.Generic.IReadOnlyCollection");
        CollectionInterfaceFullName.Add("global::System.Collections.Generic.IReadOnlyList");
        CollectionInterfaceFullName.Add("global::System.Collections.Generic.ISet");
        CollectionInterfaceFullName.Add("global::System.Collections.Generic.IReadOnlySet");
    }

    public static bool IsSystemCollectionClass(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol nameType)
            return false;
        if (!nameType.IsGenericType)
            return false;

        return CollectionFullName.Contains($"global::{nameType.ContainingNamespace.ToDisplayString()}.{nameType.Name}");
    }

    public static bool IsSystemCollections(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol nameType)
            return false;
        if (!nameType.IsGenericType)
            return false;

        var fullName = $"global::{nameType.ContainingNamespace.ToDisplayString()}.{nameType.Name}";
        if (CollectionFullName.Contains(fullName) || CollectionInterfaceFullName.Contains(fullName))
            return true;

        return type.AllInterfaces.OfType<INamedTypeSymbol>().Any(i =>
        {
            var fullInterfaceName = $"global::{i.ContainingNamespace.ToDisplayString()}.{i.Name}";
            return CollectionFullName.Contains(fullInterfaceName) || CollectionInterfaceFullName.Contains(fullInterfaceName);
        });
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

internal sealed class DictionaryMetaData
{
    private static readonly HashSet<string> DictionaryFullName = new HashSet<string>();
    private static readonly HashSet<string> DictionaryInterfaceFullName = new HashSet<string>();

    static DictionaryMetaData()
    {
        DictionaryFullName.Add("global::System.Collections.Generic.Dictionary");
        DictionaryFullName.Add("global::System.Collections.Generic.SortedDictionary");
        DictionaryFullName.Add("global::System.Collections.Concurrent.ConcurrentDictionary");
        DictionaryFullName.Add("global::System.Collections.ObjectModel.ReadOnlyDictionary");

        DictionaryInterfaceFullName.Add("global::System.Collections.Generic.IDictionary");
        DictionaryInterfaceFullName.Add("global::System.Collections.Generic.IReadOnlyDictionary");
    }

    public static bool IsDictionaryClass(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol nameType)
            return false;
        if (!nameType.IsGenericType)
            return false;

        return DictionaryFullName.Contains($"global::{nameType.ContainingNamespace.ToDisplayString()}.{nameType.Name}");
    }

    public static bool IsDictionary(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol nameType)
            return false;
        if (!nameType.IsGenericType)
            return false;

        var fullName = $"global::{nameType.ContainingNamespace.ToDisplayString()}.{nameType.Name}";

        return DictionaryFullName.Contains(fullName) || DictionaryInterfaceFullName.Contains(fullName);
    }

    public static bool VerifyKeyValueType(ImmutableArray<ITypeSymbol> typeSymbols)
    {
        if (typeSymbols.Length != 2)
            return false;

        // key = string, value = object
        return typeSymbols[0].Name.Equals("String") && typeSymbols[1].Name.Equals("Object");
    }
}

internal sealed class TomlSerializedObjectMetaData
{
    public static bool IsTomlSerializedObject(ITypeSymbol type)
    {
        return type.GetAttributes().Any(a => a.AttributeClass!.Name == "TomlSerializedObjectAttribute");
    }
}