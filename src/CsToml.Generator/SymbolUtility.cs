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

    public static string FormatUtf8PropertyName(IEnumerable<string> keys)
    {
        var builder = new StringBuilder();
        if (keys is List<string> keyList)
        {
            var keyListSpan = CollectionsMarshal.AsSpan(keyList);
            if (keyListSpan.Length == 0) return "[]";

            var lastKey = keyListSpan[keyListSpan.Length - 1];
            if (keyListSpan.Length == 1) return $"\"{lastKey}\"u8";

            builder.Append("[");
            for ( var i = 0; i < keyListSpan.Length - 1; i++)
            {
                builder.Append($"\"{keyListSpan[i]}\"u8, ");
            }
            builder.Append($"\"{lastKey}\"u8");
            builder.Append("]");
        }
        return builder.ToString();
    }

    public static string GetPropertyAccessName(IEnumerable<string> accessName, string separateChar)
    {
        var builder = new StringBuilder();
        if (accessName is List<string> accessNameList)
        {
            var keyListSpan = CollectionsMarshal.AsSpan(accessNameList);
            if (keyListSpan.Length == 1) return keyListSpan[0];

            var lastKey = keyListSpan[keyListSpan.Length - 1];
            for (var i = 0; i < keyListSpan.Length - 1; i++)
            {
                builder.Append($"{keyListSpan[i]}{separateChar}");
            }
            builder.Append($"{lastKey}");
        }
        return builder.ToString();
    }

    public static (IPropertySymbol, TomlValueType)[] FilterTomlDocumentValueMembers(
        IEnumerable<IPropertySymbol> symbols,
        string attribute)
    {
        var members = new List<(IPropertySymbol, TomlValueType)>();
        foreach (var symbol in symbols)
        {
            var attr = symbol.GetAttribute("CsToml", attribute).FirstOrDefault();
            if (attr == null) continue;
            members.Add((symbol, (TomlValueType)attr.ConstructorArguments[0].Value!));
        }
        members.Sort(static (x, y) => x.Item2 - y.Item2);
        return members.ToArray();
    }

    public static IEnumerable<IPropertySymbol> GetProperties(ITypeSymbol namedTypeSymbol)
    {
        return namedTypeSymbol.GetMembers().Where(m => m is IPropertySymbol and
        {
            IsStatic: false,
            DeclaredAccessibility: Accessibility.Public,
            IsImplicitlyDeclared: false,
            CanBeReferencedByName: true
        }).Select(i => (IPropertySymbol)i);
    }

    public static TomlSerializationKind GetTomlSerializationKind(ITypeSymbol type)
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
                    return TomlSerializationKind.Collection;
                }
                return TomlSerializationKind.ArrayOfITomlSerializedObject;
            default:
                switch (type.TypeKind)
                {
                    case TypeKind.Array:
                        if (IsElementType(type, TomlSerializationKind.Primitive))
                        {
                            return TomlSerializationKind.Array;
                        }
                        return TomlSerializationKind.ArrayOfITomlSerializedObject;
                    case TypeKind.Enum:
                        return TomlSerializationKind.Enum;
                    case TypeKind.Error:
                        return TomlSerializationKind.NotAvailable;
                    case TypeKind.Class:
                        if (CollectionMetaData.IsSystemCollections(type))
                        {
                            if (IsElementType(type, TomlSerializationKind.Primitive))
                            {
                                return TomlSerializationKind.Collection;
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
                                return TomlSerializationKind.Collection;
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

                return TomlSerializationKind.NotAvailable;
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
