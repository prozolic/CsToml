using Microsoft.CodeAnalysis;

namespace CsToml.Generator;

internal static class SymbolUtility
{
    public static IEnumerable<AttributeData> GetAttribute(this IPropertySymbol property, string namespaceName, string attributeName)
        => property.GetAttributes().Where(a =>
            a.AttributeClass!.ContainingNamespace.Name == namespaceName &&
            a.AttributeClass!.Name == attributeName);

    public static IEnumerable<AttributeData> GetCsTomlArrayOfTablesKeyAttribute(this IPropertySymbol property)
        => property.GetAttribute("CsToml", "CsTomlArrayOfTablesKeyAttribute");

    public static (IPropertySymbol, CsTomlValueType)[] FilterCsTomlPackageValueMembers(
        IEnumerable<IPropertySymbol> symbols,
        string attribute)
    {
        var members = new List<(IPropertySymbol, CsTomlValueType)>();
        foreach (var symbol in symbols)
        {
            var attr = symbol.GetAttribute("CsToml", attribute).FirstOrDefault();
            if (attr == null) continue;
            members.Add((symbol, (CsTomlValueType)attr.ConstructorArguments[0].Value!));
        }
        members.Sort(static (x, y) => x.Item2 - y.Item2);
        return members.ToArray();
    }

    public static IEnumerable<IPropertySymbol> GetPropertyAllMembers(ITypeSymbol namedTypeSymbol)
    {
        return namedTypeSymbol.GetMembers().Where(m => m is IPropertySymbol
                and
        {
            IsStatic: false,
            DeclaredAccessibility: Accessibility.Public,
            IsImplicitlyDeclared: false,
            CanBeReferencedByName: true
        }).Select(i => (IPropertySymbol)i);
    }

    public static CsTomlTypeKind GetCsTomlTypeKind(ITypeSymbol type)
    {
        switch (type.SpecialType)
        {
            case SpecialType.System_Boolean:
            case SpecialType.System_SByte:
            case SpecialType.System_Int16:
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
            case SpecialType.System_Byte:
            case SpecialType.System_UInt16:
            case SpecialType.System_UInt32:
            case SpecialType.System_UInt64:
            case SpecialType.System_Double:
            case SpecialType.System_String:
            case SpecialType.System_Char:
            case SpecialType.System_DateTime:
                return CsTomlTypeKind.Primitive;
            case SpecialType.System_Object: // Unknown
                return CsTomlTypeKind.Unknown;
            case SpecialType.System_Single: // float is not supported
                return CsTomlTypeKind.Error;
            case SpecialType.System_Collections_Generic_IEnumerable_T:
            case SpecialType.System_Collections_Generic_ICollection_T:
            case SpecialType.System_Collections_Generic_IList_T:
            case SpecialType.System_Collections_Generic_IReadOnlyCollection_T:
            case SpecialType.System_Collections_Generic_IReadOnlyList_T:
                return CsTomlTypeKind.Collection;
            default:
                switch (type.TypeKind)
                {
                    case TypeKind.Array:
                        return CsTomlTypeKind.Collection;
                    case TypeKind.Enum:
                        return CsTomlTypeKind.Primitive;
                    case TypeKind.Error:
                        return CsTomlTypeKind.Error;
                    case TypeKind.Class:
                        if (CollectionMetaData.IsGenericCollection(type))
                        {
                            return CsTomlTypeKind.Collection;
                        }
                        return CsTomlTypeKind.TableOrArrayOfTables;
                    case TypeKind.Struct:
                        switch (type.Name)
                        {
                            case "DateOnly":
                            case "TimeOnly":
                            case "DateTimeOffset":
                                return CsTomlTypeKind.Primitive;
                        }
                        return CsTomlTypeKind.TableOrArrayOfTables;
                }

                return CsTomlTypeKind.Error;
        }
    }

}
