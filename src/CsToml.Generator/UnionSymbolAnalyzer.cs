using CsToml.Generator.Internal;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace CsToml.Generator;

internal sealed record UnionLocationInfo(string FilePath, Microsoft.CodeAnalysis.Text.TextSpan Span, Microsoft.CodeAnalysis.Text.LinePositionSpan LineSpan)
{
    public static UnionLocationInfo? From(Location? location)
    {
        if (location == null || !location.IsInSource)
            return null;

        var lineSpan = location.GetLineSpan();
        return new UnionLocationInfo(lineSpan.Path ?? "", location.SourceSpan, lineSpan.Span);
    }

    public Location ToLocation() => Location.Create(FilePath, Span, LineSpan);
}

internal sealed record UnionDiagnosticInfo(DiagnosticDescriptor Descriptor, UnionLocationInfo? Location, EquatableArray<string> Args)
{
    public UnionDiagnosticInfo(DiagnosticDescriptor descriptor, Location location, object?[] args)
        : this(descriptor, UnionLocationInfo.From(location), new EquatableArray<string>(args.Select(a => a?.ToString() ?? "").ToImmutableArray()))
    {
    }

    public Diagnostic ToDiagnostic()
        => Diagnostic.Create(Descriptor, Location?.ToLocation() ?? Microsoft.CodeAnalysis.Location.None, Args.Values.ToArray());
}

internal static class UnionSymbolAnalyzer
{
    private const string UnionAttributeName = "UnionAttribute";
    private const string UnionInterfaceName = "IUnion";
    private const string UnionMembersInterfaceName = "IUnionMembers";
    private const string TomlUnionAttributeName = "TomlUnionAttribute";

    private static readonly ConditionalWeakTable<INamedTypeSymbol, UnionCases> unionCasesCache = new();

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

    private sealed class UnionCases
    {
        public static readonly UnionCases None = new(ImmutableArray<ITypeSymbol>.Empty, UnionConstructionKind.Constructor);

        public readonly ImmutableArray<ITypeSymbol> CaseTypes;
        public readonly UnionConstructionKind ConstructionKind;

        public UnionCases(ImmutableArray<ITypeSymbol> caseTypes, UnionConstructionKind constructionKind)
        {
            CaseTypes = caseTypes;
            ConstructionKind = constructionKind;
        }
    }

    public static bool TryGetUnionCases(INamedTypeSymbol unionSymbol, out ImmutableArray<ITypeSymbol> unionCreateParameterTypes, out UnionConstructionKind constructionKind)
    {
        // Reject non-unions (the vast majority of callers) before touching the cache.
        if (!IsUnionType(unionSymbol))
        {
            unionCreateParameterTypes = ImmutableArray<ITypeSymbol>.Empty;
            constructionKind = UnionConstructionKind.Constructor;
            return false;
        }

        var cases = unionCasesCache.GetValue(unionSymbol, static symbol =>
            ComputeUnionCases(symbol, out var caseTypes, out var kind) ? new UnionCases(caseTypes, kind) : UnionCases.None);

        unionCreateParameterTypes = cases.CaseTypes;
        constructionKind = cases.ConstructionKind;
        return cases.CaseTypes.Length > 0;
    }

    private static bool ComputeUnionCases(INamedTypeSymbol unionSymbol, out ImmutableArray<ITypeSymbol> unionCreateParameterTypes, out UnionConstructionKind constructionKind)
    {
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

        var constructors = unionSymbol.InstanceConstructors
            .Where(c => c.DeclaredAccessibility == Accessibility.Public &&
                        c.Parameters.Length == 1 &&
                        !SymbolEqualityComparer.Default.Equals(c.Parameters[0].Type, unionSymbol))
            .ToImmutableArray();

        // A union may expose an object-typed storage constructor alongside the per-case ones; it is not a case.
        // Judged on the declared parameter type so that a T case closed over object (Box<object>) is kept and
        // the closed and open case lists stay index-aligned.
        var nonObjectConstructors = constructors
            .Where(c => c.OriginalDefinition.Parameters[0].Type.SpecialType != SpecialType.System_Object)
            .ToImmutableArray();
        if (nonObjectConstructors.Length > 0)
        {
            constructors = nonObjectConstructors;
        }

        var constructorParameterTypes = constructors.Select(c => c.Parameters[0].Type).ToImmutableArray();

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
        // IUnionMembers is generated by the compiler for a union type, so it is not guaranteed to be present in the source.
        var provider = unionSymbol.GetTypeMembers(UnionMembersInterfaceName)
            .FirstOrDefault(t => t.TypeKind == TypeKind.Interface && t.DeclaredAccessibility == Accessibility.Public);

        if (provider == null)
        {
            return null;
        }

        // The generated serializer casts the union to this interface, so a merely declared (not implemented)
        // provider must fall back to the constructor path instead of failing at runtime.
        foreach (var implemented in unionSymbol.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(implemented.OriginalDefinition, provider.OriginalDefinition))
            {
                return provider;
            }
        }
        return null;
    }

    // A union nested inside a generic type is generic for formatter purposes: its full name mentions the
    // containing type's parameters, so they must be declared on the formatter as well.
    public static bool IsGenericUnion(INamedTypeSymbol unionSymbol)
    {
        for (var t = unionSymbol.OriginalDefinition; t is not null; t = t.ContainingType)
        {
            if (t.TypeParameters.Length > 0)
                return true;
        }
        return false;
    }

    public static ImmutableArray<ITypeParameterSymbol> GetAllTypeParameters(INamedTypeSymbol unionSymbol)
    {
        var builder = ImmutableArray.CreateBuilder<ITypeParameterSymbol>();
        AppendChain(unionSymbol.OriginalDefinition, builder, static t => t.TypeParameters);
        return builder.ToImmutable();
    }

    public static ImmutableArray<ITypeSymbol> GetAllTypeArguments(INamedTypeSymbol unionSymbol)
    {
        var builder = ImmutableArray.CreateBuilder<ITypeSymbol>();
        AppendChain(unionSymbol, builder, static t => t.TypeArguments);
        return builder.ToImmutable();
    }

    // Outermost containing type first, matching the order in which the names appear in the full type name.
    private static void AppendChain<T>(INamedTypeSymbol type, ImmutableArray<T>.Builder builder, Func<INamedTypeSymbol, ImmutableArray<T>> select)
    {
        if (type.ContainingType is not null)
            AppendChain(type.ContainingType, builder, select);
        builder.AddRange(select(type));
    }

    public static ITypeSymbol? GetTypeLevelPinnedCase(INamedTypeSymbol unionSymbol)
    {
        if (IsGenericUnion(unionSymbol))
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
            : $"CsToml.Generated.{original.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", "")}";
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

    // SanitizeIdentifier is not injective (N.A_B and N_A.B both become N_A_B), so the case index keeps
    // the name unique per union while the sanitized type name keeps it readable.
    public static string GetPinnedFormatterClassName(INamedTypeSymbol unionSymbol, ITypeSymbol openCaseType, int caseIndex)
    {
        return $"{GetFormatterClassName(unionSymbol)}_{caseIndex}_{SanitizeIdentifier(openCaseType.ToFullFormatString())}";
    }

    public static string GetPinnedFormatterReference(INamedTypeSymbol unionSymbol, int caseIndex)
    {
        TryGetUnionCases(unionSymbol.OriginalDefinition, out var openCaseTypes, out _);

        return $"global::{GetFormatterNamespace(unionSymbol)}.{GetPinnedFormatterClassName(unionSymbol, openCaseTypes[caseIndex], caseIndex)}{GetTypeArgumentList(unionSymbol)}";
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
    {
        var typeArguments = GetAllTypeArguments(unionSymbol);
        return typeArguments.Length > 0
            ? $"<{string.Join(", ", typeArguments.Select(t => t.ToFullFormatString()))}>"
            : "";
    }
}
