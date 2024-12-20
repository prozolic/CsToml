﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

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

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
internal sealed class TomlSerializedObjectAttribute : Attribute
{}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class TomlValueOnSerializedAttribute : Attribute
{
    public string AliasName { get; }

    public TomlValueOnSerializedAttribute() {  }

    public TomlValueOnSerializedAttribute(string aliasName) { this.AliasName = aliasName; }
}

""");
        });

        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            "CsToml.TomlSerializedObjectAttribute",
            static (node, token) =>
            {
                return node is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax;
            },
            static (context, token) => context).
            Combine(context.CompilationProvider).
            WithComparer(Comparer.Instance);

        context.RegisterSourceOutput(source, Emit);
    }

    private void Emit(SourceProductionContext context, (GeneratorAttributeSyntaxContext, Compilation) source)
    {
        var syntaxContext = source.Item1;
        var symbol = (INamedTypeSymbol)syntaxContext.TargetSymbol;
        var typeNode = (TypeDeclarationSyntax)syntaxContext.TargetNode;

        var typeMeta = new TypeMeta(symbol, typeNode);
        if (!typeMeta.Validate(context))
            return;

        context.AddSource($"{typeMeta.TypeName}_generated.g.cs", Generate(typeMeta));
    }

    private string Generate(TypeMeta typeMeta)
    {
        // Check if it belongs to the global namespace.
        var namespaceTag = string.IsNullOrWhiteSpace(typeMeta.NameSpace) ? string.Empty : $"namespace {typeMeta.NameSpace};";

        var code = $$"""
#nullable enable
#pragma warning disable CS0219 // The variable 'variable' is assigned but its value is never used
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument for parameter.
#pragma warning disable CS8619 // Possible null reference assignment fix

using CsToml;
using CsToml.Formatter;
using CsToml.Formatter.Resolver;

{{namespaceTag}}

partial {{typeMeta.TypeKeyword}} {{typeMeta.TypeName}} : ITomlSerializedObject<{{typeMeta.TypeName}}>
{

    static {{typeMeta.TypeName}} ITomlSerializedObject<{{typeMeta.TypeName}}>.Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
{{GenerateDeserializePart(typeMeta)}}
    }

    static void ITomlSerializedObject<{{typeMeta.TypeName}}>.Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, {{typeMeta.TypeName}} target, CsTomlSerializerOptions options)
    {
{{GenerateSerializePart(typeMeta)}}
    }

    static void ITomlSerializedObjectRegister.Register()
    {
        TomlSerializedObjectFormatterResolver.Register(new TomlSerializedObjectFormatter<{{typeMeta.TypeName}}>());
    }
}
""";

        return code;
    }

    private string GenerateDeserializePart(TypeMeta typeMeta)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"        var target = new {typeMeta.TypeName}();");

        foreach (var (property, kind, aliasName) in typeMeta.Members)
        {
            var accessName = string.IsNullOrWhiteSpace(aliasName) ? property.Name : aliasName;
            var propertyName = property.Name;

            builder.AppendLine($"        var __{propertyName}__RootNode = rootNode[{$"\"{accessName}\"u8"}];");
            builder.AppendLine($"        target.{propertyName} = options.Resolver.GetFormatter<{property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>()!.Deserialize(ref __{propertyName}__RootNode, options);");
        }

        builder.AppendLine($"        return target;");
        return builder.ToString();
    }

    private string GenerateSerializePart(TypeMeta typeMeta)
    {
        var builder = new StringBuilder();
        foreach (var (property, kind, aliasName) in typeMeta.Members)
        {
            var accessName = string.IsNullOrWhiteSpace(aliasName) ? property.Name : aliasName;
            var propertyName = property.Name;

            if (kind == TomlSerializationKind.Primitive || kind == TomlSerializationKind.Object)
            {
                builder.AppendLine($"        writer.WriteKey({$"\"{accessName}\"u8"});");
                builder.AppendLine($"        writer.WriteEqual();");
                builder.AppendLine($"        options.Resolver.GetFormatter<{property.Type.Name}>()!.Serialize(ref writer, target.{propertyName}, options);");
                builder.AppendLine($"        writer.EndKeyValue();");
            }
            else if (kind == TomlSerializationKind.TomlSerializedObject)
            {
                builder.AppendLine($"        if (options.SerializeOptions.TableStyle == TomlTableStyle.Header && (writer.State == TomlValueState.Default || writer.State == TomlValueState.Table)){{");
                builder.AppendLine($"            writer.WriteTableHeader({$"\"{accessName}\"u8"});");
                builder.AppendLine($"            writer.WriteNewLine();");
                builder.AppendLine($"            writer.BeginCurrentState(TomlValueState.Table);");
                builder.AppendLine($"            writer.PushKey({$"\"{accessName}\"u8"});");
                builder.AppendLine($"            options.Resolver.GetFormatter<{property.Type.Name}>()!.Serialize(ref writer, target.{propertyName}, options);");
                builder.AppendLine($"            writer.PopKey();");
                builder.AppendLine($"            writer.EndCurrentState();");
                builder.AppendLine($"        }}");
                builder.AppendLine($"        else{{");
                builder.AppendLine($"            writer.PushKey({$"\"{accessName}\"u8"});");
                builder.AppendLine($"            options.Resolver.GetFormatter<{property.Type.Name}>()!.Serialize(ref writer, target.{propertyName}, options);");
                builder.AppendLine($"            writer.PopKey();");
                builder.AppendLine($"        }}");
            }
            else if (kind == TomlSerializationKind.ArrayOfITomlSerializedObject || kind == TomlSerializationKind.Dictionary)
            {
                builder.AppendLine($"        writer.WriteKey({$"\"{accessName}\"u8"});");
                builder.AppendLine($"        writer.WriteEqual();");
                builder.AppendLine($"        writer.BeginCurrentState(TomlValueState.ArrayOfTable);");
                builder.AppendLine($"        options.Resolver.GetFormatter<{property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>()!.Serialize(ref writer, target.{propertyName}, options);");
                builder.AppendLine($"        writer.EndCurrentState();");
                builder.AppendLine($"        writer.EndKeyValue();");
            }
            else
            {
                builder.AppendLine($"        writer.WriteKey({$"\"{accessName}\"u8"});");
                builder.AppendLine($"        writer.WriteEqual();");
                builder.AppendLine($"        options.Resolver.GetFormatter<{property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>()!.Serialize(ref writer, target.{propertyName}, options);");
                builder.AppendLine($"        writer.EndKeyValue();");
            }
        }

        return builder.ToString();
    }
}

internal class Comparer : IEqualityComparer<(GeneratorAttributeSyntaxContext, Compilation)>
{
    public static readonly Comparer Instance = new();

    public bool Equals((GeneratorAttributeSyntaxContext, Compilation) x, (GeneratorAttributeSyntaxContext, Compilation) y)
    {
        return x.Item1.TargetNode.Equals(y.Item1.TargetNode);
    }

    public int GetHashCode((GeneratorAttributeSyntaxContext, Compilation) obj)
    {
        return obj.Item1.TargetNode.GetHashCode();
    }
}
