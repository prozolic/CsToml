using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace CsToml.Generator;

internal sealed class TypeMeta
{
    private INamedTypeSymbol symbol;
    private TypeDeclarationSyntax syntax;

    public (IPropertySymbol, TomlSerializationKind, string?)[] Members { get; }
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

        Members = symbol.GetProperties().FilterMembers();
        Array.Sort(Members, static (x, y) => x.Item2 - y.Item2);

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
        foreach (var (property, kind, aliasName) in Members)
        {
            if (kind == TomlSerializationKind.Error)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.ErrorType,
                        GetPropertyLocation(property, syntax),
                        symbol!.Name));
                error = true;
            }

            if (property.IsReadOnly)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.MustBeSetter,
                        GetPropertyLocation(property, syntax),
                        symbol!.Name));
                error = true;
            }
        }

        return !error;
    }

    public Location GetPropertyLocation(IPropertySymbol propertySymbol, TypeDeclarationSyntax syntax)
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
