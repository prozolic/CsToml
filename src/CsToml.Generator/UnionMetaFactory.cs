using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

namespace CsToml.Generator;

internal static class UnionMetaFactory
{
    public static bool TryCreate(
        INamedTypeSymbol unionSymbol,
        ITypeSymbol pinnedCaseType,
        bool isTypeLevel,
        Location location,
        List<UnionDiagnosticInfo> diagnostics,
        out UnionMeta? meta)
    {
        var original = unionSymbol.OriginalDefinition;
        var displayName = original.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        if (!UnionSymbolAnalyzer.TryGetUnionCases(unionSymbol, out var caseTypes, out var constructionKind))
        {
            meta = null;
            return false;
        }

        var caseIndex = SymbolUtility.IndexOfSymbolType(caseTypes, pinnedCaseType);
        if (caseIndex < 0)
        {
            diagnostics.Add(new UnionDiagnosticInfo(
                DiagnosticDescriptors.UnionPinnedTypeIsNotCase,
                location,
                [displayName, pinnedCaseType.ToDisplayString()]));
            meta = null;
            return false;
        }

        UnionSymbolAnalyzer.TryGetUnionCases(original, out var openCaseTypes, out _);
        var openCaseType = openCaseTypes[caseIndex];

        if (FormatterTypeMetaData.GetTomlSerializationKind(pinnedCaseType) == TomlSerializationKind.Error)
        {
            diagnostics.Add(new UnionDiagnosticInfo(
                DiagnosticDescriptors.UnionPinnedCaseCannotBeSerialized,
                location,
                [displayName, pinnedCaseType.ToDisplayString()]));
            meta = null;
            return false;
        }

        var provider = UnionSymbolAnalyzer.GetUnionMembersInterface(original);
        var valueAccess = GetValueAccess(original, provider);
        var hasHasValue = original.GetMembers("HasValue")
            .OfType<IPropertySymbol>()
            .Any(p => !p.IsStatic && p.DeclaredAccessibility == Accessibility.Public &&
                      p.GetMethod != null && p.Type.SpecialType == SpecialType.System_Boolean);

        var patternType = FormatterTypeMetaData.TryGetNullableParameterType(openCaseType, out var underlyingType)
            ? underlyingType!
            : openCaseType;

        var (hasTryGetValue, tryGetValueOutTypeName) = MatchTryGetValue(original, openCaseType, patternType);
        if (!hasTryGetValue && valueAccess == UnionValueAccess.OnlyTryGetValue)
        {
            // The case value is unreachable: no TryGetValue and no readable Value.
            diagnostics.Add(new UnionDiagnosticInfo(
                DiagnosticDescriptors.UnionPinnedCaseCannotBeSerialized,
                location,
                [displayName, pinnedCaseType.ToDisplayString()]));
            meta = null;
            return false;
        }

        meta = new UnionMeta
        {
            FullTypeName = original.ToFullFormatString(),
            DisplayName = displayName,
            FormatterNamespace = UnionSymbolAnalyzer.GetFormatterNamespace(original),
            FormatterClassName = isTypeLevel
                ? UnionSymbolAnalyzer.GetFormatterClassName(original)
                : UnionSymbolAnalyzer.GetPinnedFormatterClassName(original, openCaseType),
            TypeParameterList = original.TypeParameters.Length > 0
                ? $"<{string.Join(", ", original.TypeParameters.Select(t => t.Name))}>"
                : "",
            TypeParameterConstraints = BuildConstraintClauses(original.TypeParameters),
            IsValueType = original.IsValueType,
            HasHasValue = hasHasValue,
            ValueAccess = valueAccess,
            ConstructionKind = constructionKind,
            IsTypeLevel = isTypeLevel,
            Case = new UnionCaseMeta
            {
                PatternTypeName = patternType.ToFullFormatString(),
                HasTryGetValue = hasTryGetValue,
                TryGetValueOutTypeName = tryGetValueOutTypeName,
            },
            DependencyRegistrationCode = isTypeLevel ? BuildDependencyRegistrationCode(original, pinnedCaseType) : "",
        };
        return true;
    }

    private static UnionValueAccess GetValueAccess(INamedTypeSymbol unionSymbol, INamedTypeSymbol? provider)
    {
        if (unionSymbol.GetMembers("Value").OfType<IPropertySymbol>()
            .Any(p => !p.IsStatic && p.DeclaredAccessibility == Accessibility.Public && p.GetMethod != null))
        {
            return UnionValueAccess.ValueProperty;
        }
        if (provider != null && provider.GetMembers("Value").OfType<IPropertySymbol>().Any(p => p.GetMethod != null))
        {
            return UnionValueAccess.IUnionMembersValueProperty;
        }
        return UnionValueAccess.OnlyTryGetValue;
    }

    private static (bool HasTryGetValue, string OutTypeName) MatchTryGetValue(
        INamedTypeSymbol unionSymbol,
        ITypeSymbol caseType,
        ITypeSymbol patternType)
    {
        foreach (var method in unionSymbol.GetMembers("TryGetValue").OfType<IMethodSymbol>())
        {
            if (method.IsStatic || method.DeclaredAccessibility != Accessibility.Public)
                continue;
            if (method.ReturnType.SpecialType != SpecialType.System_Boolean)
                continue;
            if (method.Parameters.Length != 1 || method.Parameters[0].RefKind != RefKind.Out)
                continue;

            var outType = method.Parameters[0].Type;
            if (SymbolEqualityComparer.Default.Equals(outType, caseType) ||
                SymbolEqualityComparer.Default.Equals(outType, patternType))
            {
                return (true, outType.ToFullFormatString());
            }
        }
        return (false, "");
    }

    private static string BuildConstraintClauses(ImmutableArray<ITypeParameterSymbol> typeParameters)
    {
        if (typeParameters.Length == 0)
            return "";

        var builder = new StringBuilder();
        foreach (var typeParameter in typeParameters)
        {
            var parts = new List<string>();
            if (typeParameter.HasUnmanagedTypeConstraint)
                parts.Add("unmanaged");
            else if (typeParameter.HasValueTypeConstraint)
                parts.Add("struct");
            else if (typeParameter.HasReferenceTypeConstraint)
                parts.Add("class");
            else if (typeParameter.HasNotNullConstraint)
                parts.Add("notnull");

            foreach (var constraintType in typeParameter.ConstraintTypes)
            {
                parts.Add(constraintType.ToFullFormatString());
            }

            if (typeParameter.HasConstructorConstraint && !typeParameter.HasValueTypeConstraint && !typeParameter.HasUnmanagedTypeConstraint)
                parts.Add("new()");

            if (parts.Count > 0)
            {
                builder.Append($" where {typeParameter.Name} : {string.Join(", ", parts)}");
            }
        }
        return builder.ToString();
    }

    private static string BuildDependencyRegistrationCode(INamedTypeSymbol unionSymbol, ITypeSymbol pinnedCaseType)
    {
        var closure = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        closure.Add(unionSymbol);
        SymbolUtility.SearchReachableTypes(closure, pinnedCaseType);
        closure.Remove(unionSymbol);

        // Indent for the generated formatter file, where Register() lives inside a namespace block.
        var builder = new TomlValueFormatterResolverEmitter(indent: "    ");
        foreach (var type in closure.OrderBy(t => t.ToFullFormatString(), StringComparer.Ordinal))
        {
            builder.Append(type, FormatterTypeMetaData.GetTomlSerializationKind(type));
        }
        return builder.ToString();
    }
}
