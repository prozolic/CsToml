using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CsToml.Generator;

internal static class TypeDeclarationSyntaxExtensions
{
    public static bool IsPartial(this TypeDeclarationSyntax target)
        => target.AnyModifiers(syntaxKind: Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword);

    public static bool AnyModifiers(this TypeDeclarationSyntax target, Microsoft.CodeAnalysis.CSharp.SyntaxKind syntaxKind)
        => target.Modifiers.Any(m => m.IsKind(syntaxKind));

    public static bool IsNested(this TypeDeclarationSyntax target)
        => target.Parent is TypeDeclarationSyntax;

}

