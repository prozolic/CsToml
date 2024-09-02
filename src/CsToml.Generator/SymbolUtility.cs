using CsToml.Generator.Internal;
using Microsoft.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;

namespace CsToml.Generator;
internal static class SymbolUtility
{
    public static IEnumerable<AttributeData> GetAttribute(this IPropertySymbol property, string namespaceName, string attributeName)
        => property.GetAttributes().Where(a =>
            a.AttributeClass!.ContainingNamespace.Name == namespaceName &&
            a.AttributeClass!.Name == attributeName);

    public static (IPropertySymbol, TomlSerializationKind, string?)[] FilterMembers(
        this IEnumerable<IPropertySymbol> symbols)
    {
        var members = new List<(IPropertySymbol, TomlSerializationKind, string?)>();
        foreach (var symbol in symbols)
        {
            var attr = symbol.GetAttribute("CsToml", "TomlValueOnSerializedAttribute").FirstOrDefault();
            if (attr == null) continue;

            var serializationKind = GetTomlSerializationKind(symbol.Type);
            if (attr.ConstructorArguments.Length > 0)
            {
                members.Add((symbol, serializationKind, attr.ConstructorArguments[0].Value! as string));
            }
            else
            {
                members.Add((symbol, serializationKind, string.Empty));
            }
        }
        return members.ToArray();
    }

    public static IEnumerable<IPropertySymbol> GetProperties(this ITypeSymbol namedTypeSymbol)
    {
        return namedTypeSymbol.GetMembers().Where(m => m is IPropertySymbol and
        {
            IsStatic: false,
            DeclaredAccessibility: Accessibility.Public,
            IsImplicitlyDeclared: false,
            CanBeReferencedByName: true
        }).Select(i => (IPropertySymbol)i);
    }

    public static TomlSerializationKind GetTomlSerializationKind(this ITypeSymbol type)
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
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_String:
            case SpecialType.System_Char:
            case SpecialType.System_DateTime:
                return TomlSerializationKind.Primitive;
            case SpecialType.System_Object: // Unknown
                return TomlSerializationKind.Object;
            case SpecialType.System_Collections_Generic_IEnumerable_T:
            case SpecialType.System_Collections_Generic_ICollection_T:
            case SpecialType.System_Collections_Generic_IList_T:
            case SpecialType.System_Collections_Generic_IReadOnlyCollection_T:
            case SpecialType.System_Collections_Generic_IReadOnlyList_T:
                if (IsElementType(type, TomlSerializationKind.Primitive))
                {
                    return TomlSerializationKind.PrimitiveCollection;
                }
                return TomlSerializationKind.ArrayOfITomlSerializedObject;
            default:
                switch (type.TypeKind)
                {
                    case TypeKind.Array:
                        if (IsElementType(type, TomlSerializationKind.Primitive))
                        {
                            return TomlSerializationKind.PrimitiveArray;
                        }
                        return TomlSerializationKind.ArrayOfITomlSerializedObject;
                    case TypeKind.Enum:
                        return TomlSerializationKind.Enum;
                    case TypeKind.Error:
                        return TomlSerializationKind.Error;
                    case TypeKind.Class:
                        if (CollectionMetaData.IsSystemCollections(type))
                        {
                            if (IsElementType(type, TomlSerializationKind.Primitive))
                            {
                                return TomlSerializationKind.PrimitiveCollection;
                            }
                            return TomlSerializationKind.ArrayOfITomlSerializedObject;
                        }
                        if (DictionaryMetaData.IsDictionary(type))
                        {
                            return TomlSerializationKind.Dictionary;
                        }
                        if (TomlSerializedObjectMetaData.IsTomlSerializedObject(type))
                        {
                            return TomlSerializationKind.TomlSerializedObject;
                        }
                        return TomlSerializationKind.Class;
                    case TypeKind.Interface:
                        if (CollectionMetaData.IsSystemCollections(type))
                        {
                            if (IsElementType(type, TomlSerializationKind.Primitive))
                            {
                                return TomlSerializationKind.PrimitiveCollection;
                            }
                            return TomlSerializationKind.ArrayOfITomlSerializedObject;
                        }
                        if (DictionaryMetaData.IsDictionary(type))
                        {
                            return TomlSerializationKind.Dictionary;
                        }
                        return TomlSerializationKind.Interface;
                    case TypeKind.Struct:
                        return TomlSerializationKind.Struct;
                }

                return TomlSerializationKind.Error;
        }
    }

    public static bool IsElementType(ITypeSymbol typeSymbol, TomlSerializationKind kind)
    {
        if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            var symbolKind = GetTomlSerializationKind(arrayTypeSymbol.ElementType);
            return symbolKind == kind;
        }

        return false;
    }
}
