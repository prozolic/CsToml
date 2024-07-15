using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CsToml.Generator;

internal class CsTomlPackageGenerator
{
    public static void Generate(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource("CsTomlPackagePartGenerator.cs", """
using System;

namespace CsToml;

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
internal sealed class CsTomlPackagePartAttribute : Attribute
{}

/// <summary>
/// 
/// </summary>
internal enum CsTomlValueType
{
    None = -1,
    KeyValue = 0,
    Array = 1,
    InlineTable = 2,
    Table = 3,
    ArrayOfTables = 4,
}

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class CsTomlValueOnSerializedAttribute : Attribute
{
    public CsTomlValueType Type { get; }

    public CsTomlValueOnSerializedAttribute(CsTomlValueType value) { this.Type = value; }

}

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class CsTomlArrayOfTablesKeyAttribute : Attribute
{
    public string KeyName { get; }

    public CsTomlArrayOfTablesKeyAttribute(string keyName) { this.KeyName = keyName; }
}

""");
        });

        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            "CsToml.CsTomlPackagePartAttribute",
            static (node, token) => true,
            static (context, token) => context);

        context.RegisterSourceOutput(source, Emit);
    }

    private static void Emit(SourceProductionContext context, GeneratorAttributeSyntaxContext source)
    {
        var typeSymbol = source.TargetSymbol as INamedTypeSymbol;
        var typeNode = source.TargetNode as TypeDeclarationSyntax;

        var ns = typeSymbol!.ContainingNamespace.IsGlobalNamespace ? 
            string.Empty : 
            $"{typeSymbol.ContainingNamespace}";

        var generator = new CsTomlValueSerializeGenerator(typeSymbol, typeNode!);
        var serializerProcessCode = generator.GenerateSerializeProcessCode(context);
        var deserializerProcessCode = generator.GenerateDeserializeProcessCode(context);

        var code = $$"""
#nullable enable
#pragma warning disable CS0219 // The variable 'variable' is assigned but its value is never used
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument for parameter.

using CsToml;

namespace {{ns}};

partial class {{typeSymbol.Name}} : ICsTomlPackagePart<{{typeSymbol.Name}}>
{
    static void ICsTomlPackagePart<{{typeSymbol.Name}}>.Serialize<TBufferWriter, ICsTomlValueSerializer>(ref TBufferWriter writer, ref {{typeSymbol.Name}} target, CsTomlSerializerOptions? options)
    {
{{serializerProcessCode}}
    }

    static {{typeSymbol.Name}} ICsTomlPackagePart<{{typeSymbol.Name}}>.Deserialize<ICsTomlValueSerializer>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options)
    {
{{deserializerProcessCode}}
    }
}

""";
        context.AddSource($"{typeSymbol.Name}_generated.g.cs", code);
    }

}

