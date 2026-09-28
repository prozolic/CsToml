using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

namespace CsToml.Generator;

internal sealed record UnionDiagnosticInfo(DiagnosticDescriptor Descriptor, Location Location, object?[] Args);

internal static class UnionSymbolAnalyzer
{
    private const string UnionAttributeName = "UnionAttribute";
    private const string UnionInterfaceName = "IUnion";
    private const string UnionMembersInterfaceName = "IUnionMembers";
    private const string TomlUnionAttributeName = "TomlUnionAttribute";

    public static bool IsUnionType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named || named.TypeKind != TypeKind.Struct && named.TypeKind != TypeKind.Class)
        {
            return false;
        }

        // Check [System.Runtime.CompilerServices.IUnion].
        foreach (var i in named.AllInterfaces)
        {
            if (i.Name == UnionInterfaceName && IsCompilerServicesNamespace(i.ContainingNamespace))
            {
                return true;
            }
        }

        // Check [System.Runtime.CompilerServices.Union].
        foreach (var attribute in named.GetAttributes())
        {
            if (attribute.AttributeClass is { Name: UnionAttributeName } attributeClass &&
                IsCompilerServicesNamespace(attributeClass.ContainingNamespace))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsCompilerServicesNamespace(INamespaceSymbol? ns)
        => ns is { Name: "CompilerServices", ContainingNamespace: { Name: "Runtime", ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } } };

    public static bool IsUnionWithCases(ITypeSymbol type)
    {
        return type is INamedTypeSymbol named && IsUnionWithCases(named);
    }

    public static bool IsUnionWithCases(INamedTypeSymbol named)
    {
        return TryGetUnionCases(named, out _, out _);
    }

    public static bool TryGetUnionCases(INamedTypeSymbol unionSymbol, out ImmutableArray<ITypeSymbol> unionCreateParameterTypes, out UnionConstructionKind constructionKind)
    {
        if (!IsUnionType(unionSymbol))
        {
            unionCreateParameterTypes = ImmutableArray<ITypeSymbol>.Empty;
            constructionKind = UnionConstructionKind.Constructor;
            return false;
        }

        // Check IUnionMembers.Create<T>(T value).
        var unionMembersInterfaceSymbol = GetUnionMembersInterface(unionSymbol);
        if (unionMembersInterfaceSymbol != null)
        {
            var createParameterTypes = unionMembersInterfaceSymbol.GetMembers("Create")
                .OfType<IMethodSymbol>()
                .Where(m => m.IsStatic &&
                            m.DeclaredAccessibility != Accessibility.Private &&
                            m.Parameters.Length == 1 &&
                            SymbolEqualityComparer.Default.Equals(m.ReturnType, unionSymbol))
                .Select(m => m.Parameters[0].Type)
                .ToImmutableArray();

            if (createParameterTypes.Length == 0)
            {
                unionCreateParameterTypes = ImmutableArray<ITypeSymbol>.Empty;
                constructionKind = UnionConstructionKind.Constructor;
                return false;
            }

            unionCreateParameterTypes = createParameterTypes;
            constructionKind = UnionConstructionKind.IUnionMembersCreate;
            return true;
        }

        var constructorParameterTypes = unionSymbol.InstanceConstructors
            .Where(c => c.DeclaredAccessibility == Accessibility.Public &&
                        c.Parameters.Length == 1 &&
                        !SymbolEqualityComparer.Default.Equals(c.Parameters[0].Type, unionSymbol))
            .Select(c => c.Parameters[0].Type)
            .ToImmutableArray();

        // A union may expose an object-typed storage constructor alongside the per-case ones; it is not a case.
        var nonObjectParameterTypes = constructorParameterTypes
            .Where(t => t.SpecialType != SpecialType.System_Object)
            .ToImmutableArray();
        if (nonObjectParameterTypes.Length > 0)
        {
            constructorParameterTypes = nonObjectParameterTypes;
        }

        if (constructorParameterTypes.Length == 0)
        {
            unionCreateParameterTypes = ImmutableArray<ITypeSymbol>.Empty;
            constructionKind = UnionConstructionKind.Constructor;
            return false;
        }

        unionCreateParameterTypes = constructorParameterTypes;
        constructionKind = UnionConstructionKind.Constructor;
        return true;
    }

    public static INamedTypeSymbol? GetUnionMembersInterface(INamedTypeSymbol unionSymbol)
    {
        return unionSymbol.GetTypeMembers(UnionMembersInterfaceName)
            .FirstOrDefault(t => t.TypeKind == TypeKind.Interface && t.DeclaredAccessibility == Accessibility.Public);
    }

    public static ITypeSymbol? GetTypeLevelPinnedCase(INamedTypeSymbol unionSymbol)
    {
        if (unionSymbol.OriginalDefinition.TypeParameters.Length > 0)
            return null;

        var attribute = unionSymbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass is { Name: TomlUnionAttributeName, IsGenericType: true } attributeClass &&
            attributeClass.ContainingNamespace.Name == "CsToml");

        return attribute?.AttributeClass!.TypeArguments[0];
    }

    public static string GetFormatterNamespace(INamedTypeSymbol unionSymbol)
    {
        var original = unionSymbol.OriginalDefinition;
        return original.ContainingNamespace.IsGlobalNamespace
            ? "CsToml.Generated"
            : $"CsToml.Generated.{original.ContainingNamespace}";
    }

    public static string GetFormatterClassName(INamedTypeSymbol unionSymbol)
    {
        var original = unionSymbol.OriginalDefinition;
        if (original.ContainingType is null)
            return $"{original.Name}Formatter";

        var names = new List<string>();
        for (var t = original; t is not null; t = t.ContainingType)
        {
            names.Insert(0, t.Name);
        }
        return $"{string.Join("_", names)}Formatter";
    }

    public static string GetFormatterReference(INamedTypeSymbol unionSymbol)
    {
        return $"global::{GetFormatterNamespace(unionSymbol)}.{GetFormatterClassName(unionSymbol)}{GetTypeArgumentList(unionSymbol)}";
    }

    public static string GetPinnedFormatterClassName(INamedTypeSymbol unionSymbol, ITypeSymbol openCaseType)
    {
        return $"{GetFormatterClassName(unionSymbol)}_{SanitizeIdentifier(openCaseType.ToFullFormatString())}";
    }

    public static string GetPinnedFormatterReference(INamedTypeSymbol unionSymbol, int caseIndex)
    {
        TryGetUnionCases(unionSymbol.OriginalDefinition, out var openCaseTypes, out _);

        return $"global::{GetFormatterNamespace(unionSymbol)}.{GetPinnedFormatterClassName(unionSymbol, openCaseTypes[caseIndex])}{GetTypeArgumentList(unionSymbol)}";
    }

    public static string SanitizeIdentifier(string typeName)
    {
        var builder = new StringBuilder(typeName.Length);
        var source = typeName.Replace("global::", "");

        foreach (var c in source)
        {
            if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
            {
                builder.Append(c);
            }
            else
            {
                builder.Append('_');
            }
        }
        return builder.ToString();
    }

    private static string GetTypeArgumentList(INamedTypeSymbol unionSymbol)
        => unionSymbol.TypeArguments.Length > 0
            ? $"<{string.Join(", ", unionSymbol.TypeArguments.Select(t => t.ToFullFormatString()))}>"
            : "";
}
