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
        this IEnumerable<IPropertySymbol> symbols,
        TomlNamingConvention namingConvention = TomlNamingConvention.None)
    {
        foreach (var symbol in symbols)
        {
            var attr = symbol.GetAttributeData("CsToml", "TomlValueOnSerializedAttribute").FirstOrDefault();
            if (attr == null) continue;

            var serializationKind = FormatterTypeMetaData.GetTomlSerializationKind(symbol.Type);

            // Check for NullHandling property in TomlValueOnSerializedAttribute
            var nullHandling = TomlNullHandling.Error;
            if (attr.NamedArguments.Length > 0)
            {
                var nullHandlingArg = attr.NamedArguments.FirstOrDefault(arg => arg.Key == "NullHandling");
                if (nullHandlingArg.Value.Value is int nullHandlingValue)
                {
                    nullHandling = (TomlNullHandling)nullHandlingValue;
                }
            }

            // Check for AliasName property in TomlValueOnSerializedAttribute
            // If AliasName is set, it always takes precedence over the value of TomlNamingConvention.
            var enableAliasName = false;
            var aliasName = "";
            if (attr.NamedArguments.Length > 0)
            {
                var aliasNameArg = attr.NamedArguments.FirstOrDefault(arg => arg.Key == "AliasName");
                if (aliasNameArg.Value.Value is string strValue)
                {
                    aliasName = (string)strValue;
                    enableAliasName = true;
                }
            }

            if (enableAliasName)
            {
                // Use explicit alias name (takes precedence over naming convention)
                yield return new TomlValueOnSerializedData()
                {
                    Symbol = symbol,
                    SerializationKind = serializationKind,
                    DefinedName = symbol.Name,
                    TomlValueOnSerializedAttributeData = attr,
                    AliasName = aliasName!,
                    CanAliasName = true,
                    NullHandling = nullHandling,
                    // Check if the property type is nullable (reference type or Nullable<T>)
                    IsNullable = symbol.Type.NullableAnnotation == NullableAnnotation.Annotated
                        || (symbol.Type is INamedTypeSymbol namedSymbol && namedSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                };
            }
            else
            {
                // Apply naming convention if specified
                var convertedName = namingConvention != TomlNamingConvention.None
                    ? NamingConventionConverter.Convert(symbol.Name, namingConvention)
                    : null;

                yield return new TomlValueOnSerializedData()
                {
                    Symbol = symbol,
                    SerializationKind = serializationKind,
                    DefinedName = symbol.Name,
                    TomlValueOnSerializedAttributeData = attr,
                    AliasName = convertedName,
                    CanAliasName = convertedName != null,
                    NullHandling = nullHandling,
                    // Check if the property type is nullable (reference type or Nullable<T>)
                    IsNullable = symbol.Type.NullableAnnotation == NullableAnnotation.Annotated
                        || (symbol.Type is INamedTypeSymbol namedSymbol && namedSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
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
