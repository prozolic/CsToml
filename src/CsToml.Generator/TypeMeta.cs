using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace CsToml.Generator;

internal sealed class TypeMeta
{
    private INamedTypeSymbol symbol;
    private TypeDeclarationSyntax syntax;

    public IReadOnlyCollection<(IPropertySymbol, TomlSerializationKind, string?)> Members { get; }
    public ImmutableArray<(ITypeSymbol, TomlSerializationKind)> DefinedTypes { get; }
    public string NameSpace { get; }
    public TomlSerializedObjectType Type { get; }
    public string TypeName { get; }
    public string FullTypeName { get; }
    public string TypeKeyword { get; }

    public TypeMeta(INamedTypeSymbol symbol, TypeDeclarationSyntax syntax)
    {
        this.symbol = symbol;
        this.syntax = syntax;

        NameSpace = symbol!.ContainingNamespace.IsGlobalNamespace ?
            string.Empty :
            $"{symbol.ContainingNamespace}";

        var members = symbol.GetProperties().FilterMembers();
        Array.Sort(members, static (x, y) => x.Item2 - y.Item2);
        Members = members;

        var typesymbols = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var member in Members)
        {
            SearchTypeSymbol(typesymbols, member.Item1.Type);
        }
        DefinedTypes = typesymbols.Select(t => (t, t.GetTomlSerializationKind())).ToImmutableArray();

        TypeName = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        FullTypeName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        if (symbol.IsRecord)
            TypeKeyword = symbol.IsValueType ? "record struct" : "record";
        else
            TypeKeyword = symbol.IsValueType ? "struct" : "class";
    }

    public bool Validate(SourceProductionContext context)
    {
        if (!syntax.IsPartial())
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.MustBePartial,
                    syntax.Identifier.GetLocation(),
                    symbol!.Name));
            return false;
        }
        if (syntax.IsNested())
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.MustNotBeNestedType,
                    syntax.Identifier.GetLocation(),
                    symbol!.Name));
            return false;
        }
        if (symbol.IsAbstract)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.MustNotBeAbstract,
                    syntax.Identifier.GetLocation(),
                    symbol!.Name));
            return false;
        }

        var error = false;
        var keyTable = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (property, kind, aliasName) in Members)
        {
            if (string.IsNullOrEmpty(aliasName))
            {
                if (!keyTable.Contains(property.Name))
                {
                    keyTable.Add(property.Name);
                }
                else
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.DefiningKeyMultipleTimesForProperty,
                            GetPropertyLocation(property, syntax),
                            property.Name));
                    error = true;
                }
            }
            else
            {
                if (!keyTable.Contains(aliasName!))
                {
                    keyTable.Add(aliasName!);
                }
                else
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.DefiningKeyMultipleTimesForAliasName,
                            GetPropertyLocation(property, syntax),
                            aliasName));
                    error = true;
                }
            }

            if (kind == TomlSerializationKind.Error)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.ErrorType,
                        GetPropertyLocation(property, syntax),
                        symbol!.Name));
                error = true;
            }
        }

        return !error;
    }

    private Location GetPropertyLocation(IPropertySymbol propertySymbol, TypeDeclarationSyntax syntax)
    {
        return propertySymbol.Locations.FirstOrDefault() ?? syntax.Identifier.GetLocation();
    }

    private void SearchTypeSymbol(HashSet<ITypeSymbol> typesymbols, ITypeSymbol rootTypeSymbol)
    {
        typesymbols.Add(rootTypeSymbol);
        if (rootTypeSymbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            var elementType = arrayTypeSymbol.ElementType;
            SearchTypeSymbol(typesymbols, elementType);
        }
        else if (rootTypeSymbol is INamedTypeSymbol namedSymbol)
        {
            if (namedSymbol.IsGenericType)
            {
                foreach (var typeParameter in namedSymbol.TypeArguments)
                {
                    SearchTypeSymbol(typesymbols, typeParameter);
                }
            }
            foreach (var propetryParameter in namedSymbol.GetProperties().FilterMembers())
            {
                SearchTypeSymbol(typesymbols, propetryParameter.Item1.Type);
            }
        }
    }
}

internal sealed class ConstructorMeta
{
    private INamedTypeSymbol symbol;
    private TypeDeclarationSyntax syntax;
    private readonly List<(IMethodSymbol, ImmutableArray<IParameterSymbol>)> instanceConstructors;

    public bool IsImplicitlyDeclared { get; }
    public bool IncludeParameterless { get; }
    public bool IsParameterlessOnly { get; }
    public IReadOnlyList<(IMethodSymbol ctor, ImmutableArray<IParameterSymbol> parameters)> InstanceConstructors => instanceConstructors;
    public IReadOnlyList<IParameterSymbol> ConstructorParameters { get; }
    public IReadOnlyList<IPropertySymbol> ConstructorParameterProperties { get; }
    public IReadOnlyList<IPropertySymbol> MembersOfObjectInitialisers { get; }

    public ConstructorMeta(INamedTypeSymbol symbol, TypeDeclarationSyntax syntax, TypeMeta typeMeta)
    {
        this.symbol = symbol;
        this.syntax = syntax;

        var instanceConstructors = new List<(IMethodSymbol ctor, ImmutableArray<IParameterSymbol> parameters)>(symbol.InstanceConstructors.Length);
        foreach (var constructor in symbol.InstanceConstructors.Where(c => c is IMethodSymbol and { DeclaredAccessibility: Accessibility.Public}))
        {
            if (constructor.Parameters.Any(p => p.Type.MetadataName == symbol.MetadataName))
            {
                continue;
            }

            instanceConstructors.Add((constructor, constructor.Parameters));
        }
        this.instanceConstructors = instanceConstructors;

        // except the default constructor for a class or struct.
        IsImplicitlyDeclared = instanceConstructors.All(c => c.ctor.IsImplicitlyDeclared);
        IncludeParameterless = instanceConstructors.Any(c => c.parameters.Length == 0);
        IsParameterlessOnly = instanceConstructors.Count == 1 && IncludeParameterless;

        IParameterSymbol[] constructorParameters = [];
        foreach (var constructor in this.InstanceConstructors)
        {
            if (constructor.parameters.All(p => typeMeta.Members.Any(m => m.Item1.Type.Equals(p.Type, SymbolEqualityComparer.Default) && m.Item1.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase))))
            {
                if ((constructorParameters?.Length ?? 0) <= constructor.parameters.Length)
                {
                    constructorParameters = [.. constructor.parameters];
                }
            }
        }
        this.ConstructorParameters = constructorParameters!;

        var constructorParameterProperties = new List<IPropertySymbol>();
        var membersOfObjectInitialisers = new List<IPropertySymbol>();
        foreach (var member in typeMeta.Members)
        {
            if (!ConstructorParameters.Any(c => c.Type.Equals(member.Item1.Type, SymbolEqualityComparer.Default) && c.Name.Equals(member.Item1.Name, StringComparison.OrdinalIgnoreCase)))
            {
                membersOfObjectInitialisers.Add(member.Item1);
            }
        }
        foreach (var member in constructorParameters!)
        {
            var property = typeMeta.Members.FirstOrDefault(m => m.Item1.Type.Equals(member.Type, SymbolEqualityComparer.Default) && m.Item1.Name.Equals(member.Name, StringComparison.OrdinalIgnoreCase));
            constructorParameterProperties.Add(property.Item1);
        }

        this.MembersOfObjectInitialisers = membersOfObjectInitialisers;
        this.ConstructorParameterProperties = constructorParameterProperties;
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
                        DiagnosticDescriptors.MustBeSetter,
                        GetPropertyLocation(property, syntax),
                        symbol!.Name));
                error = true;
                continue;
            }

            if (property.SetMethod!.DeclaredAccessibility == Accessibility.Private)
            {
                context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.MustBePublicOrInit,
                    GetPropertyLocation(property, syntax),
                    symbol!.Name));
                error = true;
            }

        }

        if (ConstructorParameters.Count == 0 && !IncludeParameterless)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.NoConstructor,
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
