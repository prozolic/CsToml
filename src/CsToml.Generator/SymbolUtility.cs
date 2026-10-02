using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

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

            // Union handling: a member-level [TomlUnion<T>] pins this member alone (via a dedicated
            // formatter instance); a type-level [TomlUnion<T>] on the union pins the registered formatter,
            // so only the emission kind switches to T's kind here. Member-level takes precedence.
            // A union member without either is reported as CsTomlError011 by TypeMeta.
            string? formatterAccess = null;
            INamedTypeSymbol? memberPinnedUnion = null;
            ITypeSymbol? memberPinnedCaseType = null;
            var pinnedCaseInvalid = false;
            var hasTomlUnionAttributeOnNonUnion = false;
            ITypeSymbol? wrappedUnionType = null;

            string? memberPinnedTypeDisplayName = null;
            var memberPinAttribute = symbol.GetAttributes().FirstOrDefault(a =>
                a.AttributeClass is { Name: "TomlUnionAttribute", IsGenericType: true } attributeClass &&
                attributeClass.ContainingNamespace.Name == "CsToml");

            if (memberPinAttribute != null)
            {
                memberPinnedTypeDisplayName = memberPinAttribute.AttributeClass!.TypeArguments[0].ToDisplayString();
                if (serializationKind == TomlSerializationKind.Union && symbol.Type is INamedTypeSymbol memberUnionType)
                {
                    var pinnedType = memberPinAttribute.AttributeClass!.TypeArguments[0];
                    UnionSymbolAnalyzer.TryGetUnionCases(memberUnionType, out var closedCaseTypes, out _);
                    var caseIndex = IndexOfSymbolType(closedCaseTypes, pinnedType);
                    if (caseIndex >= 0)
                    {
                        serializationKind = ResolvePinnedSerializationKind(pinnedType);
                        formatterAccess = $"{UnionSymbolAnalyzer.GetPinnedFormatterReference(memberUnionType, caseIndex)}.Instance";
                        memberPinnedUnion = memberUnionType;
                        memberPinnedCaseType = pinnedType;
                    }
                    else
                    {
                        pinnedCaseInvalid = true;
                    }
                }
                else
                {
                    // A member-level pin only applies to a property typed as the union itself; on List<U>,
                    // U[] or U? the user most likely meant the wrapped union, so report that specifically.
                    wrappedUnionType = FindReachableUnion(symbol.Type);
                    hasTomlUnionAttributeOnNonUnion = wrappedUnionType == null;
                }
            }
            else if (serializationKind == TomlSerializationKind.Union && symbol.Type is INamedTypeSymbol unionType)
            {
                var pinnedType = UnionSymbolAnalyzer.GetTypeLevelPinnedCase(unionType);
                if (pinnedType != null)
                {
                    UnionSymbolAnalyzer.TryGetUnionCases(unionType, out var closedCaseTypes, out _);
                    var caseIndex = IndexOfSymbolType(closedCaseTypes, pinnedType);
                    if (caseIndex >= 0)
                    {
                        // The registered formatter for the union is the pinned one, so only the
                        // emission templates need to follow T's kind.
                        serializationKind = ResolvePinnedSerializationKind(pinnedType);
                    }
                    // An unresolvable T is reported as CsTomlError012 when the union meta is built.
                }
            }

            // Apply naming convention if specified (explicit alias name takes precedence)
            var convertedName = !enableAliasName && namingConvention != TomlNamingConvention.None
                ? NamingConventionConverter.Convert(symbol.Name, namingConvention)
                : null;

            yield return new TomlValueOnSerializedData()
            {
                Symbol = symbol,
                SerializationKind = serializationKind,
                DefinedName = symbol.Name,
                TomlValueOnSerializedAttributeData = attr,
                AliasName = enableAliasName ? aliasName! : convertedName,
                CanAliasName = enableAliasName || convertedName != null,
                NullHandling = nullHandling,
                // Check if the property type is nullable (reference type or Nullable<T>)
                IsNullable = symbol.Type.NullableAnnotation == NullableAnnotation.Annotated
                    || (symbol.Type is INamedTypeSymbol namedSymbol && namedSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T),
                FormatterAccess = formatterAccess,
                MemberPinnedUnionSymbol = memberPinnedUnion,
                MemberPinnedCaseType = memberPinnedCaseType,
                MemberPinnedTypeDisplayName = memberPinnedTypeDisplayName,
                PinnedCaseInvalid = pinnedCaseInvalid,
                HasTomlUnionAttributeOnNonUnion = hasTomlUnionAttributeOnNonUnion,
                WrappedUnionType = wrappedUnionType,
            };
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

    public static void SearchReachableTypes(HashSet<ITypeSymbol> typeSymbols, ITypeSymbol rootTypeSymbol)
    {
        if (!typeSymbols.Add(rootTypeSymbol))
            return;

        // Built-in types resolve directly and need no dependency registration; without this cut-off
        // their interfaces would drag unnecessary formatters into the closure
        // (e.g. string : IEnumerable<char> -> IEnumerableFormatter<char>).
        if (FormatterTypeMetaData.ContainsBuiltInFormatterType(rootTypeSymbol))
            return;

        if (rootTypeSymbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            SearchReachableTypes(typeSymbols, arrayTypeSymbol.ElementType);
        }
        else if (rootTypeSymbol is INamedTypeSymbol namedSymbol)
        {
            if (namedSymbol.IsGenericType)
            {
                foreach (var typeArgument in namedSymbol.TypeArguments)
                {
                    SearchReachableTypes(typeSymbols, typeArgument);
                }
            }
            // Only the type-level pinned case of a union is ever (de)serialized through the resolver;
            // unpinned unions are rejected by CsTomlError011 and member-level pins add their case explicitly.
            if (UnionSymbolAnalyzer.IsUnionWithCases(namedSymbol) &&
                UnionSymbolAnalyzer.GetTypeLevelPinnedCase(namedSymbol) is { } pinnedCaseType)
            {
                SearchReachableTypes(typeSymbols, pinnedCaseType);
            }
            // Only the property types are needed here; FilterTomlValueOnSerializedMembers would also run
            // the union pin analysis (case lookup, formatter-name formatting) whose results are discarded.
            foreach (var property in namedSymbol.GetPublicProperties())
            {
                if (property.GetAttributeData("CsToml", "TomlValueOnSerializedAttribute").Any())
                {
                    SearchReachableTypes(typeSymbols, property.Type);
                }
            }
        }

        foreach (var i in rootTypeSymbol.AllInterfaces.Where(i => i is { DeclaredAccessibility: Accessibility.Public, IsGenericType: true }))
        {
            if (FormatterTypeMetaData.ContainsCollectionInterfaceType(i))
            {
                SearchReachableTypes(typeSymbols, i);
            }
        }
    }

    private static TomlSerializationKind ResolvePinnedSerializationKind(ITypeSymbol pinnedType)
    {
        // Emission kind of a pinned case. When the case is itself a type-level pinned union, follow the
        // pin chain to the leaf case so the member uses the leaf's template (a union-typed leaf would fall
        // through to the value template and emit `Key = Number = 5` for a POCO leaf).
        var kind = FormatterTypeMetaData.GetTomlSerializationKind(pinnedType);
        var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        while (kind == TomlSerializationKind.Union &&
               pinnedType is INamedTypeSymbol unionType &&
               visited.Add(unionType) &&
               UnionSymbolAnalyzer.GetTypeLevelPinnedCase(unionType) is { } nextPinnedType)
        {
            pinnedType = nextPinnedType;
            kind = FormatterTypeMetaData.GetTomlSerializationKind(pinnedType);
        }
        return kind;
    }

    private static ITypeSymbol? FindReachableUnion(ITypeSymbol type)
    {
        var reachable = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        SearchReachableTypes(reachable, type);
        return reachable.FirstOrDefault(t => UnionSymbolAnalyzer.IsUnionWithCases(t));
    }

    public static int IndexOfSymbolType(ImmutableArray<ITypeSymbol> caseTypes, ITypeSymbol type)
    {
        for (var i = 0; i < caseTypes.Length; i++)
        {
            if (SymbolEqualityComparer.Default.Equals(caseTypes[i], type))
            {
                return i;
            }
        }
        return -1;
    }

    public static string ToFullFormatString(this ITypeSymbol typeSymbol)
        => typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

}
