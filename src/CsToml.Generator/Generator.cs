using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
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

        if (!typeNode!.IsPartial())
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.NoPartial,
                    typeNode.Identifier.GetLocation(),
                    typeSymbol!.Name));
            return;
        }
        if (typeNode!.IsNested())
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.NoPartial,
                    typeNode.Identifier.GetLocation(),
                    typeSymbol!.Name));
            return;
        }

        var ns = typeSymbol!.ContainingNamespace.IsGlobalNamespace ?
            string.Empty :
            $"{typeSymbol.ContainingNamespace}";

        var generator2 = new DeserializeGenerator(typeSymbol, typeNode!);
        var generator3 = new SerializeGenerator(typeSymbol, typeNode!);

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
using System.Buffers;

namespace {{ns}};

partial class {{typeSymbol.Name}} : ITomlSerializedObject<{{typeSymbol.Name}}>
{

    static void ITomlSerializedObject<{{typeSymbol.Name}}>.Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, {{typeSymbol.Name}} target, CsTomlSerializerOptions options)
    {
{{generator3.GenerateSerializeProcessCode(context)}}
    }

    static {{typeSymbol.Name}} ITomlSerializedObject<{{typeSymbol.Name}}>.Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        // TODO: implemented...
{{generator2.GenerateDeserializeProcessCode(context)}}
    }

    static void ITomlSerializedObjectRegister.Register()
    {
        TomlSerializedObjectFormatterResolver.Register(new TomlSerializedObjectFormatter<{{typeSymbol.Name}}>());
    }
}

""";
        context.AddSource($"{typeSymbol.Name}_generated.g.cs", code);
    }

    internal sealed class DeserializeGenerator
    {
        private INamedTypeSymbol typeSymbol;
        private TypeDeclarationSyntax typeNode;
        private (IPropertySymbol, TomlValueType)[] valueMembers;
        private StringBuilder serializerBuilder;
        private StringBuilder deserializerBuilder;

        public DeserializeGenerator(INamedTypeSymbol typeSymbol, TypeDeclarationSyntax typeNode)
        {
            this.typeSymbol = typeSymbol;
            this.typeNode = typeNode;

            var allmembers = SymbolUtility.GetProperties(typeSymbol);
            valueMembers = SymbolUtility.FilterTomlDocumentValueMembers(allmembers, "TomlValueOnSerializedAttribute").ToArray();
            serializerBuilder = new StringBuilder();
            deserializerBuilder = new StringBuilder();
        }

        public string GenerateDeserializeProcessCode(SourceProductionContext context)
        {
            deserializerBuilder.Clear();
            GenerateDeserializeCore(context, string.Empty, typeSymbol, valueMembers);
            return deserializerBuilder.ToString();
        }

        private void GenerateDeserializeCore(SourceProductionContext context, string accessName, INamedTypeSymbol typeSymbol, (IPropertySymbol, TomlValueType)[] properties)
        {
            deserializerBuilder.AppendLine($"        var target = new {this.typeSymbol}();");

            var arrayOftableProperties = new List<(IPropertySymbol, TomlValueType, AttributeData)>();
            foreach (var (property, type) in properties)
            {
                var propertyType = property.Type;
                var attribute = propertyType.GetAttributes();

                var kind = SymbolUtility.GetTomlSerializationKind(property.Type);
                if (kind == TomlSerializationKind.NotAvailable)
                {
                    continue;
                }

                var accessNames = new List<string> { property.Name };
                GenerateDeserializeForKeyValue(context, accessNames, typeSymbol, property, type, kind);
            }

            deserializerBuilder.AppendLine($"        return target;");
        }

        private void GenerateDeserializeForKeyValue(SourceProductionContext context, List<string> accessName, INamedTypeSymbol typeSymbol, IPropertySymbol property, TomlValueType type, TomlSerializationKind kind)
        {
            var findName = SymbolUtility.FormatUtf8PropertyName(accessName);
            var propertyName = SymbolUtility.GetPropertyAccessName(accessName, ".");
            var valueName = SymbolUtility.GetPropertyAccessName(accessName, "_");

            if (kind == TomlSerializationKind.Dictionary)
            {
                ImmutableArray<ITypeSymbol> elementType;
                if (property.Type is INamedTypeSymbol namedTypeSymbol) // collection interface
                {
                    elementType = namedTypeSymbol.TypeArguments;
                }

                if (!DictionaryMetaData.VerifyKeyValueType(elementType))
                {
                    return;
                }
            }

            deserializerBuilder.AppendLine($"        var __{propertyName}__formatter = TomlValueFormatterResolver.GetFormatter<{property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>();");
            deserializerBuilder.AppendLine($"        var __{propertyName}__RootNode = rootNode[{findName}];");
            deserializerBuilder.AppendLine($"        target.{propertyName} = __{propertyName}__formatter?.Deserialize(ref __{propertyName}__RootNode, options) ?? default!;");
        }

    }

    internal sealed class SerializeGenerator
    {
        private INamedTypeSymbol typeSymbol;
        private TypeDeclarationSyntax typeNode;
        private (IPropertySymbol, TomlValueType)[] valueMembers;
        private ClassDeclarationSyntax[] allclassDeclarationSyntax;
        private StringBuilder serializerBuilder;

        public SerializeGenerator(INamedTypeSymbol typeSymbol, TypeDeclarationSyntax typeNode)
        {
            this.typeSymbol = typeSymbol;
            this.typeNode = typeNode;
            this.allclassDeclarationSyntax = this.typeNode.SyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().ToArray();

            var allmembers = SymbolUtility.GetProperties(typeSymbol);
            valueMembers = SymbolUtility.FilterTomlDocumentValueMembers(allmembers, "TomlValueOnSerializedAttribute").ToArray();
            serializerBuilder = new StringBuilder();
        }

        public string GenerateSerializeProcessCode(SourceProductionContext context)
        {
            serializerBuilder.Clear();
            GenerateSerializeCore(context, string.Empty, typeSymbol, valueMembers);
            return serializerBuilder.ToString();
        }

        private void GenerateSerializeCore(SourceProductionContext context, string accessName, INamedTypeSymbol typeSymbol, (IPropertySymbol, TomlValueType)[] properties)
        {
            var arrayOftableProperties = new List<(IPropertySymbol, TomlValueType, AttributeData)>();
            foreach (var (property, type) in properties)
            {
                var propertyType = property.Type;
                var attribute = propertyType.GetAttributes();

                var kind = SymbolUtility.GetTomlSerializationKind(property.Type);
                if (kind == TomlSerializationKind.NotAvailable)
                {
                    continue;
                }

                var accessNames = new List<string> { property.Name };
                GenerateSerializeForKeyValue(context, accessNames, typeSymbol, property, type, kind);

            }
        }

        private void GenerateSerializeForKeyValue(SourceProductionContext context, List<string> accessName, INamedTypeSymbol typeSymbol, IPropertySymbol property, TomlValueType type, TomlSerializationKind kind)
        {
            var findName = SymbolUtility.FormatUtf8PropertyName(accessName);
            var propertyName = SymbolUtility.GetPropertyAccessName(accessName, ".");
            var valueName = SymbolUtility.GetPropertyAccessName(accessName, "_");

            if (kind == TomlSerializationKind.Primitive || kind == TomlSerializationKind.Object)
            {
                serializerBuilder.AppendLine($"        writer.WriteKey({findName});");
                serializerBuilder.AppendLine($"        writer.WriteEqual();");
                serializerBuilder.AppendLine($"        var __{propertyName}__formatter = TomlValueFormatterResolver.GetFormatter<{property.Type.Name}>();");
                serializerBuilder.AppendLine($"        __{propertyName}__formatter.Serialize(ref writer, target.{propertyName}, options);");
                serializerBuilder.AppendLine($"        writer.EndKeyValue();");
            }
            else if (kind == TomlSerializationKind.TomlSerializedObject)
            {
                serializerBuilder.AppendLine($"        writer.PushKey({findName});");
                serializerBuilder.AppendLine($"        var __{propertyName}__formatter = TomlValueFormatterResolver.GetFormatter<{property.Type.Name}>();");
                serializerBuilder.AppendLine($"        __{propertyName}__formatter.Serialize(ref writer, target.{propertyName}, options);");
                serializerBuilder.AppendLine($"        writer.PopKey();");
            }
            else if (kind == TomlSerializationKind.ArrayOfITomlSerializedObject)
            {
                serializerBuilder.AppendLine($"        writer.WriteKey({findName});");
                serializerBuilder.AppendLine($"        writer.WriteEqual();");
                serializerBuilder.AppendLine($"        writer.BeginCurrentState(TomlValueState.ArrayOfTable);");
                serializerBuilder.AppendLine($"        var __{propertyName}__formatter = TomlValueFormatterResolver.GetFormatter<{property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>();");
                serializerBuilder.AppendLine($"        __{propertyName}__formatter.Serialize(ref writer, target.{propertyName}, options);");
                serializerBuilder.AppendLine($"        writer.EndCurrentState();");
                serializerBuilder.AppendLine($"        writer.EndKeyValue();");
            }
            else
            {
                serializerBuilder.AppendLine($"        writer.WriteKey({findName});");
                serializerBuilder.AppendLine($"        writer.WriteEqual();");
                serializerBuilder.AppendLine($"        var __{propertyName}__formatter = TomlValueFormatterResolver.GetFormatter<{property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>();");
                serializerBuilder.AppendLine($"        __{propertyName}__formatter.Serialize(ref writer, target.{propertyName}, options);");
                serializerBuilder.AppendLine($"        writer.EndKeyValue();");
            }
        }
    }
}

