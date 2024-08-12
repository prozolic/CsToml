using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CsToml.Generator;

[Generator(LanguageNames.CSharp)]
public partial class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource("TomlSerializedObjectGenerator.cs", """
using System;

namespace CsToml;

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
internal sealed class TomlSerializedObjectAttribute : Attribute
{}

/// <summary>
/// 
/// </summary>
internal enum TomlValueType
{
    None = -1,
    KeyValue = 0,
    Array = 1,
    InlineTable = 2,
    Table = 3,
///    ArrayOfTables = 4,
}

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class TomlValueOnSerializedAttribute : Attribute
{
    public TomlValueType Type { get; }

    public TomlValueOnSerializedAttribute(TomlValueType value) { this.Type = value; }

}

/// <summary>
/// 
/// </summary>
///[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
///internal sealed class TomlArrayOfTablesKeyAttribute : Attribute
///{
///    public string KeyName { get; }

///    public int Index { get;}

///    public CsTomlArrayOfTablesKeyAttribute(string keyName, int index) { this.KeyName = keyName; this.Index = index;}
///}

""");
        });

        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            "CsToml.TomlSerializedObjectAttribute",
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

        var generator = new CsTomlSerializationGenerator(typeSymbol, typeNode!);

        var code = $$"""
#nullable enable
#pragma warning disable CS0219 // The variable 'variable' is assigned but its value is never used
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument for parameter.

using CsToml;
using System.Buffers;

namespace {{ns}};

partial class {{typeSymbol.Name}} : ITomlSerializedObject<{{typeSymbol.Name}}>
{
    static void ITomlSerializedObject<{{typeSymbol.Name}}>.Serialize<TBufferWriter, ITomlValueSerializer>(ref TBufferWriter writer, {{typeSymbol.Name}}? target, CsTomlSerializerOptions? options)
    {
{{generator.GenerateSerializeProcessCode(context)}}
    }

    static {{typeSymbol.Name}} ITomlSerializedObject<{{typeSymbol.Name}}>.Deserialize<ITomlValueSerializer>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options)
    {
{{generator.GenerateDeserializeProcessCode(context)}}
    }

    static {{typeSymbol.Name}} ITomlSerializedObject<{{typeSymbol.Name}}>.Deserialize<ITomlValueSerializer>(in ReadOnlySequence<byte> tomlText, CsTomlSerializerOptions? options)
    {
        // TODO: implemented...
{{generator.GenerateDeserializeProcessCode(context)}}
    }
}

""";
        context.AddSource($"{typeSymbol.Name}_generated.g.cs", code);
    }

}

