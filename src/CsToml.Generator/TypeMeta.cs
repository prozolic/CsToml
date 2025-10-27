using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;

namespace CsToml.Generator;

internal sealed class TypeMeta
{
    private INamedTypeSymbol symbol;
    private TypeDeclarationSyntax syntax;

    public ImmutableArray<TomlValueOnSerializedData> Members { get; }
    public ImmutableArray<(ITypeSymbol, TomlSerializationKind)> DefinedTypes { get; }
    public string NameSpace { get; }
    public TomlSerializedObjectType Type { get; }
    public string TypeName { get; }
    public string FullTypeName { get; }
    public string TypeKeyword { get; }
    public string GenericTypeParameterName { get; }
    public TomlNamingConvention NamingConvention { get; }
    public bool IsReferenceType => !symbol.IsValueType;

    public TypeMeta(INamedTypeSymbol symbol, TypeDeclarationSyntax syntax)
    {
        this.symbol = symbol;
        this.syntax = syntax;

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

        Members = symbol.GetPublicProperties().FilterTomlValueOnSerializedMembers(NamingConvention).OrderBy(m => m.SerializationKind).ToImmutableArray();

        var typesymbols = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var member in Members)
        {
            SearchTypeSymbol(typesymbols, member.Symbol.Type);
        }
        DefinedTypes = typesymbols.Select(t => (t, FormatterTypeMetaData.GetTomlSerializationKind(t))).ToImmutableArray();
    }

    public bool Validate(SourceProductionContext context)
    {
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
        foreach (var member in Members)
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
            foreach (var propetryParameter in namedSymbol.GetPublicProperties().FilterTomlValueOnSerializedMembers(TomlNamingConvention.None))
            {
                SearchTypeSymbol(typesymbols, propetryParameter.Symbol.Type);
            }
        }
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
            if (constructor.parameters.All(p => typeMeta.Members.Any(m => m.Symbol.Type.Equals(p.Type, SymbolEqualityComparer.Default) && m.Symbol.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase))))
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
        foreach (var member in typeMeta.Members)
        {
            if (!ConstructorParameters.Any(c => c.Type.Equals(member.Symbol.Type, SymbolEqualityComparer.Default) && c.Name.Equals(member.Symbol.Name, StringComparison.OrdinalIgnoreCase)))
            {
                membersOfObjectInitialisers.Add(member.Symbol);
            }
        }
        foreach (var member in constructorParameters!)
        {
            var property = typeMeta.Members.FirstOrDefault(m => m.Symbol.Type.Equals(member.Type, SymbolEqualityComparer.Default) && m.Symbol.Name.Equals(member.Name, StringComparison.OrdinalIgnoreCase));
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
// This matches the public TomlIgnoreCondition in CsToml library
internal enum TomlIgnoreCondition
{
    Never = 0,
    Always = 1,
    WhenWritingNull = 2
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

    public TomlIgnoreCondition IgnoreCondition { get; init; }
}
