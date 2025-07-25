using Microsoft.CodeAnalysis;

namespace CsToml.Generator;
internal static class SymbolUtility
{
    public static IEnumerable<AttributeData> GetAttributeData(this ISymbol symbol, string namespaceName, string attributeName)
        => symbol.GetAttributes().Where(a =>
            a.AttributeClass!.ContainingNamespace.Name == namespaceName &&
            a.AttributeClass!.Name == attributeName);

    public static bool IsTomlSerializedObject(this ISymbol typeSymbol)
        => typeSymbol.GetAttributeData("CsToml", "TomlSerializedObjectAttribute").Any();

    public static IEnumerable<TomlValueOnSerializedData> FilterTomlValueOnSerializedMembers(
        this IEnumerable<IPropertySymbol> symbols)
    {
        foreach (var symbol in symbols)
        {
            var attr = symbol.GetAttributeData("CsToml", "TomlValueOnSerializedAttribute").FirstOrDefault();
            if (attr == null) continue;

            var serializationKind = FormatterTypeMetaData.GetTomlSerializationKind(symbol.Type);
            if (attr.ConstructorArguments.Length > 0 && (attr.ConstructorArguments[0].Value as string) != null)
            {
                yield return new TomlValueOnSerializedData()
                {
                    Symbol = symbol,
                    SerializationKind = serializationKind,
                    DefinedName = symbol.Name,
                    TomlValueOnSerializedAttributeData = attr,
                    AliasName = (string)attr.ConstructorArguments[0].Value!,
                    CanAliasName = true
                };
            }
            else
            {
                yield return new TomlValueOnSerializedData()
                {
                    Symbol = symbol,
                    SerializationKind = serializationKind,
                    DefinedName = symbol.Name,
                    TomlValueOnSerializedAttributeData = attr,
                    AliasName = null,
                    CanAliasName = false
                };
            }
        }
    }

    public static IEnumerable<IPropertySymbol> GetPublicProperties(this ITypeSymbol namedTypeSymbol)
    {
        return namedTypeSymbol.GetMembers().Where(m => m is IPropertySymbol and
        {
            IsStatic: false,
            DeclaredAccessibility: Accessibility.Public,
            IsImplicitlyDeclared: false,
            CanBeReferencedByName: true
        }).Select(i => (IPropertySymbol)i);
    }

    public static string ToFullFormatString(this ITypeSymbol typeSymbol)
        => typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
}
