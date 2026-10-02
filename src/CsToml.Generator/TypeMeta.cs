using CsToml.Generator.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace CsToml.Generator;

internal sealed class TypeMeta
{
    private INamedTypeSymbol symbol;
    private TypeDeclarationSyntax syntax;
    private readonly List<UnionDiagnosticInfo> unionDiagnostics = new();
    private bool hasUnionErrorReportedElsewhere;
    public ImmutableArray<TomlValueOnSerializedData> OrderedMembers { get; }
    public ImmutableArray<(ITypeSymbol, TomlSerializationKind)> DefinedTypes { get; }
    public string NameSpace { get; }
    public TomlSerializedObjectType Type { get; }
    public string TypeName { get; }
    public string FullTypeName { get; }
    public string TypeKeyword { get; }
    public string GenericTypeParameterName { get; }
    public TomlNamingConvention NamingConvention { get; }
    public bool IsReferenceType => !symbol.IsValueType;
    public bool IsUnion { get; }
    public EquatableArray<UnionMeta> UnionMetas { get; }
    public HashSet<TomlSerializationKind> TomlSerializationKindLookup { get; }

    public TypeMeta(INamedTypeSymbol symbol, TypeDeclarationSyntax syntax)
    {
        this.symbol = symbol;
        this.syntax = syntax;
        IsUnion = UnionSymbolAnalyzer.IsUnionType(symbol);

        TypeName = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        FullTypeName = symbol.ToFullFormatString();
        GenericTypeParameterName = symbol.IsValueType ? TypeName : $"{TypeName}?";

        if (symbol.IsRecord)
            TypeKeyword = symbol.IsValueType ? "record struct" : "record";
        else
            TypeKeyword = symbol.IsValueType ? "struct" : "class";

        NameSpace = symbol!.ContainingNamespace.IsGlobalNamespace ?
            string.Empty :
            $"{symbol.ContainingNamespace}";

        // Get naming convention from attribute
        NamingConvention = GetNamingConvention(symbol);

        OrderedMembers = symbol.GetPublicProperties().FilterTomlValueOnSerializedMembers(NamingConvention).OrderBy(m => m.SerializationKind).ToImmutableArray();

        TomlSerializationKindLookup = new HashSet<TomlSerializationKind>([
            TomlSerializationKind.TomlSerializedObjectArray,
            TomlSerializationKind.TomlSerializedObjectCollection,
            TomlSerializationKind.TypeParameter,
            TomlSerializationKind.NullableStructWithTypeParameter,
            TomlSerializationKind.Dictionary,
            TomlSerializationKind.TomlSerializedObject
        ]);

        // A single closure walk per member: the per-member set feeds DefinedTypes and, in the same pass,
        // records the first member that reaches each union (for diagnostic locations), so no second
        // walk is needed when the union descriptors are built.
        var typesymbols = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        var unionOrigins = new Dictionary<INamedTypeSymbol, TomlValueOnSerializedData>(SymbolEqualityComparer.Default);
        foreach (var member in OrderedMembers)
        {
            var reachable = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            SymbolUtility.SearchReachableTypes(reachable, member.Symbol.Type);

            HashSet<ITypeSymbol>? pinnedCaseReachable = null;
            if (member.MemberPinnedCaseType != null)
            {
                pinnedCaseReachable = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
                SymbolUtility.SearchReachableTypes(pinnedCaseReachable, member.MemberPinnedCaseType);
                reachable.UnionWith(pinnedCaseReachable);
            }
            typesymbols.UnionWith(reachable);
            foreach (var type in reachable)
            {
                if (type is not INamedTypeSymbol unionSymbol || unionOrigins.ContainsKey(unionSymbol) ||
                    !UnionSymbolAnalyzer.IsUnionWithCases(unionSymbol))
                {
                    continue;
                }
                // A member that pins its own union type directly is not an unpinned origin for it
                // (an invalid pin is already CsTomlError012); a later member reaching the union through
                // a wrapper claims the diagnostic instead.
                if ((member.MemberPinnedUnionSymbol != null || member.PinnedCaseInvalid) &&
                    SymbolEqualityComparer.Default.Equals(unionSymbol, member.Symbol.Type) &&
                    !(pinnedCaseReachable?.Contains(unionSymbol) ?? false))
                {
                    continue;
                }
                unionOrigins[unionSymbol] = member;
            }
        }
        DefinedTypes = typesymbols.Select(t => (t, FormatterTypeMetaData.GetTomlSerializationKind(t))).ToImmutableArray();

        UnionMetas = CollectUnionMetas(unionOrigins);
    }

    private EquatableArray<UnionMeta> CollectUnionMetas(Dictionary<INamedTypeSymbol, TomlValueOnSerializedData> unionOrigins)
    {
        var unionMetaMap = new Dictionary<string, UnionMeta>(StringComparer.Ordinal);

        foreach (var member in OrderedMembers)
        {
            if (member.MemberPinnedUnionSymbol != null && member.MemberPinnedCaseType != null &&
                UnionMetaFactory.TryCreate(member.MemberPinnedUnionSymbol, member.MemberPinnedCaseType, isTypeLevel: false, GetPropertyLocation(member.Symbol, syntax), unionDiagnostics, out var memberMeta))
            {
                unionMetaMap[memberMeta!.IdentityKey] = memberMeta;
            }
        }

        // Every reachable union must be pinned at type level; each union is analyzed once and any
        // diagnostic is attributed to the first member that reaches it.
        foreach (var pair in unionOrigins)
        {
            var unionSymbol = pair.Key;
            var member = pair.Value;
            var location = GetPropertyLocation(member.Symbol, syntax);

            var typeLevelPinnedCase = UnionSymbolAnalyzer.GetTypeLevelPinnedCase(unionSymbol);
            if (typeLevelPinnedCase != null)
            {
                // Unions declared in this compilation get their CsTomlError012/016 from the [TomlUnion<T>]
                // pipeline (once, at the union); only unions from referenced assemblies are reported here.
                var declaredInSource = unionSymbol.DeclaringSyntaxReferences.Length > 0;
                var sink = declaredInSource ? new List<UnionDiagnosticInfo>() : unionDiagnostics;
                if (UnionMetaFactory.TryCreate(unionSymbol, typeLevelPinnedCase, isTypeLevel: true, location, sink, out var typeMeta))
                {
                    unionMetaMap[typeMeta!.IdentityKey] = typeMeta;
                }
                else if (declaredInSource)
                {
                    hasUnionErrorReportedElsewhere = true;
                }
                continue;
            }

            unionDiagnostics.Add(new UnionDiagnosticInfo(
                DiagnosticDescriptors.UnionRequiresTomlUnionAttribute,
                location,
                [member.Symbol.Name, unionSymbol.ToDisplayString()]));
        }

        if (unionMetaMap.Count == 0)
            return EquatableArray<UnionMeta>.Empty;

        return new EquatableArray<UnionMeta>(
            unionMetaMap.OrderBy(kv => kv.Key, StringComparer.Ordinal).Select(kv => kv.Value).ToImmutableArray());
    }

    public bool Validate(SourceProductionContext context)
    {
        if (IsUnion)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.UnionCannotBeTomlSerializedObject,
                    syntax.Identifier.GetLocation(),
                    symbol!.Name));
            return false;
        }
        if (!syntax.IsPartial())
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.TypeMustBePartial,
                    syntax.Identifier.GetLocation(),
                    symbol!.Name));
            return false;
        }
        if (syntax.IsNested())
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.TypeCannotBeNested,
                    syntax.Identifier.GetLocation(),
                    symbol!.Name));
            return false;
        }
        if (symbol.IsAbstract)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.TypeCannotBeAbstract,
                    syntax.Identifier.GetLocation(),
                    symbol!.Name));
            return false;
        }

        var error = false;
        var keyTable = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in OrderedMembers)
        {
            var property = member.Symbol;
            var kind = member.SerializationKind;

            if (member.CanAliasName)
            {
                var aliasName = member.AliasName!;
                if (!keyTable.Contains(aliasName))
                {
                    keyTable.Add(aliasName!);
                }
                else
                {
                    var arguments = member.Arguments;

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.DuplicateAliasKey,
                            arguments[0].GetLocation(),
                            aliasName));
                    error = true;
                }

                // if the alias name contains a newline or carriage return, it is invalid.
                if (aliasName.Contains("\n") || aliasName.Contains("\r"))
                {
                    var arguments = member.Arguments;

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.AliasNameCannotContainNewlines,
                            arguments[0].GetLocation(),
                            aliasName));
                    error = true;
                }
            }
            else
            {
                var name = member.DefinedName!;
                if (!keyTable.Contains(name))
                {
                    keyTable.Add(name);
                }
                else
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.DuplicatePropertyKey,
                            GetPropertyLocation(property, syntax),
                            name));
                    error = true;
                }
            }

            if (kind == TomlSerializationKind.Error)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.InvalidSerializationType,
                        GetPropertyLocation(property, syntax),
                        symbol!.Name));
                error = true;
            }

            if (member.PinnedCaseInvalid)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.UnionPinnedTypeIsNotCase,
                        GetPropertyLocation(property, syntax),
                        property.Type.ToDisplayString(),
                        member.MemberPinnedTypeDisplayName));
                error = true;
            }

            if (member.WrappedUnionType != null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.TomlUnionAttributeOnWrappedUnionMember,
                        GetPropertyLocation(property, syntax),
                        property.Name,
                        member.WrappedUnionType.ToDisplayString(),
                        property.Type.ToDisplayString()));
                error = true;
            }

            if (member.HasTomlUnionAttributeOnNonUnion)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.TomlUnionAttributeOnNonUnion,
                        GetPropertyLocation(property, syntax),
                        property.Type.ToDisplayString()));
                error = true;
            }
        }

        foreach (var unionDiagnostic in unionDiagnostics)
        {
            context.ReportDiagnostic(unionDiagnostic.ToDiagnostic());
            error = true;
        }
        if (hasUnionErrorReportedElsewhere)
        {
            error = true;
        }

        return !error;
    }

    private TomlNamingConvention GetNamingConvention(INamedTypeSymbol symbol)
    {
        var attr = symbol.GetAttributeData("CsToml", "TomlSerializedObjectAttribute").FirstOrDefault();
        if (attr == null)
            return TomlNamingConvention.None;

        // Check for constructor argument
        if (attr.ConstructorArguments.Length > 0)
        {
            var value = attr.ConstructorArguments[0].Value;
            if (value is int intValue)
            {
                return (TomlNamingConvention)intValue;
            }
        }

        // Check for named argument
        foreach (var namedArg in attr.NamedArguments)
        {
            if (namedArg.Key == "NamingConvention" && namedArg.Value.Value is int namedValue)
            {
                return (TomlNamingConvention)namedValue;
            }
        }

        return TomlNamingConvention.None;
    }

    private Location GetPropertyLocation(IPropertySymbol propertySymbol, TypeDeclarationSyntax syntax)
    {
        return propertySymbol.Locations.FirstOrDefault() ?? syntax.Identifier.GetLocation();
    }

}

internal sealed class ConstructorMeta
{
    private INamedTypeSymbol symbol;
    private TypeDeclarationSyntax syntax;

    public bool IsImplicitlyDeclared { get; }
    public bool IncludeParameterless { get; }
    public bool IsParameterlessOnly { get; }
    public ImmutableArray<(IMethodSymbol ctor, ImmutableArray<IParameterSymbol> parameters)> InstanceConstructors { get; }
    public ImmutableArray<IParameterSymbol> ConstructorParameters { get; }
    public ImmutableArray<IPropertySymbol> ConstructorParameterProperties { get; }
    public ImmutableArray<IPropertySymbol> MembersOfObjectInitialisers { get; }

    public ConstructorMeta(INamedTypeSymbol symbol, TypeDeclarationSyntax syntax, TypeMeta typeMeta)
    {
        this.symbol = symbol;
        this.syntax = syntax;

        var instanceConstructors = new List<(IMethodSymbol ctor, ImmutableArray<IParameterSymbol> parameters)>(symbol.InstanceConstructors.Length);
        foreach (var constructor in symbol.InstanceConstructors.Where(c => c is IMethodSymbol and { DeclaredAccessibility: Accessibility.Public }))
        {
            if (constructor.Parameters.Any(p => p.Type.MetadataName == symbol.MetadataName))
            {
                continue;
            }

            instanceConstructors.Add((constructor, constructor.Parameters));
        }
        InstanceConstructors = instanceConstructors.ToImmutableArray();

        // except the default constructor for a class or struct.
        IsImplicitlyDeclared = instanceConstructors.All(c => c.ctor.IsImplicitlyDeclared);
        IncludeParameterless = instanceConstructors.Any(c => c.parameters.Length == 0);
        IsParameterlessOnly = instanceConstructors.Count == 1 && IncludeParameterless;

        IParameterSymbol[] constructorParameters = [];
        foreach (var constructor in this.InstanceConstructors)
        {
            if (constructor.parameters.All(p => typeMeta.OrderedMembers.Any(m => m.Symbol.Type.Equals(p.Type, SymbolEqualityComparer.Default) && m.Symbol.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase))))
            {
                if ((constructorParameters?.Length ?? 0) <= constructor.parameters.Length)
                {
                    constructorParameters = [.. constructor.parameters];
                }
            }
        }
        this.ConstructorParameters = constructorParameters!.ToImmutableArray();

        var constructorParameterProperties = new List<IPropertySymbol>();
        var membersOfObjectInitialisers = new List<IPropertySymbol>();
        foreach (var member in typeMeta.OrderedMembers)
        {
            if (!ConstructorParameters.Any(c => c.Type.Equals(member.Symbol.Type, SymbolEqualityComparer.Default) && c.Name.Equals(member.Symbol.Name, StringComparison.OrdinalIgnoreCase)))
            {
                membersOfObjectInitialisers.Add(member.Symbol);
            }
        }
        foreach (var member in constructorParameters!)
        {
            var property = typeMeta.OrderedMembers.FirstOrDefault(m => m.Symbol.Type.Equals(member.Type, SymbolEqualityComparer.Default) && m.Symbol.Name.Equals(member.Name, StringComparison.OrdinalIgnoreCase));
            constructorParameterProperties.Add(property.Symbol);
        }

        this.MembersOfObjectInitialisers = membersOfObjectInitialisers.ToImmutableArray();
        this.ConstructorParameterProperties = constructorParameterProperties.ToImmutableArray(); ;
    }

    public bool Validate(SourceProductionContext context)
    {
        var error = false;

        foreach (var property in MembersOfObjectInitialisers)
        {
            if (property.IsReadOnly)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.PropertyMustHaveSetter,
                        GetPropertyLocation(property, syntax),
                        symbol!.Name));
                error = true;
                continue;
            }

            if (property.SetMethod!.DeclaredAccessibility == Accessibility.Private)
            {
                context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.SetterMustBePublicOrInit,
                    GetPropertyLocation(property, syntax),
                    symbol!.Name));
                error = true;
            }

        }

        if (ConstructorParameters.Length == 0 && !IncludeParameterless)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.NoBindableConstructor,
                    syntax.Identifier.GetLocation(),
                    symbol!.Name));
            error = true;
        }

        return !error;
    }

    private Location GetPropertyLocation(IPropertySymbol propertySymbol, TypeDeclarationSyntax syntax)
    {
        return propertySymbol.Locations.FirstOrDefault() ?? syntax.Identifier.GetLocation();
    }
}

internal enum TomlSerializedObjectType
{
    Class,
    Struct,
    Record,
}

// Internal enum used by the generator for code generation
// This matches the public TomlNullHandling in CsToml library
internal enum TomlNullHandling
{
    Error = 0,
    Ignore = 1
}

[StructLayout(LayoutKind.Auto)]
internal struct TomlValueOnSerializedData
{
    public IPropertySymbol Symbol { get; init; }

    public TomlSerializationKind SerializationKind { get; init; }

    public string? DefinedName { get; init; }

    public AttributeData? TomlValueOnSerializedAttributeData { get; init; }

    public readonly SeparatedSyntaxList<AttributeArgumentSyntax> Arguments =>
        TomlValueOnSerializedAttributeData?.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax attributeSyntax
            ? attributeSyntax.ArgumentList?.Arguments ?? default
            : default;

    public string? AliasName { get; init; }

    public bool CanAliasName { get; init; }

    public TomlNullHandling NullHandling { get; init; }

    public bool IsNullable { get; init; }

    public string? FormatterAccess { get; init; }

    public INamedTypeSymbol? MemberPinnedUnionSymbol { get; init; }

    public ITypeSymbol? MemberPinnedCaseType { get; init; }

    public string? MemberPinnedTypeDisplayName { get; init; }

    public bool PinnedCaseInvalid { get; init; }

    public bool HasTomlUnionAttributeOnNonUnion { get; init; }

    public ITypeSymbol? WrappedUnionType { get; init; }
}
