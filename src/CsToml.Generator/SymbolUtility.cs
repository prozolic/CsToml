using Microsoft.CodeAnalysis;

namespace CsToml.Generator;

internal static class SymbolUtility
{
    public static IEnumerable<(IPropertySymbol, CsTomlValueType)> FilterCsTomlPackageValueMembers(
        IEnumerable<IPropertySymbol> symbols,
        string attribute)
    {
        return symbols.Where(m => m.GetAttributes()
            .Where(a =>
                a.AttributeClass!.ContainingNamespace.Name == "CsToml" &&
                a.AttributeClass!.Name == attribute)
                .Any())
            .Select(m =>
            {
                var attr = m.GetAttributes().Where(a =>
                        a.AttributeClass!.ContainingNamespace.Name == "CsToml" &&
                        a.AttributeClass!.Name == attribute)
                    .FirstOrDefault();
                return (m, (CsTomlValueType)attr.ConstructorArguments[0].Value!);
            });
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
