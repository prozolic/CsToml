using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace CsToml.Generator;

internal class CsTomlPackageGenerator
{
    public static void Generate(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource("CsTomlPackagePartAttribute.cs", """
using System;

namespace CsToml;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
internal sealed class CsTomlPackagePartAttribute : Attribute
{
}

internal enum CsTomlValueType : byte
{
    KeyValue = 0,
    Array = 1,
    Table = 2,
    ArrayOfTables = 3
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class CsTomlPackageValueAttribute : Attribute
{
    public CsTomlValueType Type { get; }

    public CsTomlPackageValueAttribute(CsTomlValueType value) { this.Type = value; }

}

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

        var generator = new CsTomlValueSerializerGenerator(typeSymbol, typeNode!);
        var serializerProcessCode = generator.Generate(context);

        var param = serializerProcessCode.Length > 0 ? $$"""
        var equals = " = "u8;
        var linefeed = "\n"u8;
        var tableStart = "["u8;
        var tableEnd = "]"u8;
        var arrayOfTableStart = "[["u8;
        var arrayOfTableEnd = "]]"u8;
""" : string.Empty;

        var code = $$"""
#nullable enable
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument for parameter.

#pragma warning disable CS0219 // The variable 'variable' is assigned but its value is never used

using CsToml;
using System.Buffers;

namespace {{ns}};

partial class {{typeSymbol.Name}} : ICsTomlPackagePart<{{typeSymbol.Name}}>
{
    static void ICsTomlPackagePart<{{typeSymbol.Name}}>.Serialize<TBufferWriter, ICsTomlValueSerializer>(ref TBufferWriter writer, ref {{typeSymbol.Name}} target)
    {
{{param}}

{{serializerProcessCode}}

    }
}

""";
        context.AddSource($"{typeSymbol.Name}_generated.g.cs", code);
    }

    private sealed class CsTomlValueSerializerGenerator
    {
        private INamedTypeSymbol typeSymbol;
        private TypeDeclarationSyntax typeNode;
        private StringBuilder builder;

        public CsTomlValueSerializerGenerator(INamedTypeSymbol typeSymbol, TypeDeclarationSyntax typeNode)
        {
            this.typeSymbol = typeSymbol;
            this.typeNode = typeNode;
            builder = new StringBuilder();
        }

        public string Generate(SourceProductionContext context)
        {
            var allmembers = SymbolUtility.GetPropertyAllMembers(typeSymbol);
            var valuemembers = SymbolUtility.FilterCsTomlPackageValueMembers(allmembers, "CsTomlPackageValueAttribute");

            GenerateProcessCodeCore(context, string.Empty, typeSymbol, valuemembers);

            return builder.ToString();
        }

        private void GenerateProcessCodeCore(SourceProductionContext context, string accessName, INamedTypeSymbol typeSymbol, IEnumerable<(IPropertySymbol, CsTomlValueType)> properties)
        {
            foreach (var (property, type) in properties)
            {
                var kind = SymbolUtility.GetCsTomlTypeKind(property.Type);
                if (kind == CsTomlTypeKind.Error)
                {
                    continue;
                }

                if (type == CsTomlValueType.KeyValue)
                {
                    GenerateProcessCodeKeyValue(context, accessName, typeSymbol, property, type, kind);
                    continue;
                }
                else if (type == CsTomlValueType.Array)
                {
                    if (kind != CsTomlTypeKind.Collection)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.ArrayError,
                                property.Locations[0],
                                typeSymbol.Name, property.Name));
                        continue;
                    }
                    builder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof({typeSymbol.Name}.{property.Name}));");
                    builder.AppendLine($"        writer.Write(equals);");
                    builder.AppendLine($"        ICsTomlValueSerializer.Serialize(ref writer, target.{accessName}{property.Name});");
                    builder.AppendLine($"        writer.Write(linefeed);");
                }
                else if (type == CsTomlValueType.Table)
                {
                    builder.AppendLine($"        writer.Write(linefeed);");
                    builder.AppendLine($"        writer.Write(tableStart);");
                    builder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof(target.{property.Name}));");
                    builder.AppendLine($"        writer.Write(tableEnd);");
                    builder.AppendLine($"        writer.Write(linefeed);");
                    GenerateProcessCodeTable(
                        context, $"{property.Name}.", 
                        (INamedTypeSymbol)property.Type, 
                        SymbolUtility.FilterCsTomlPackageValueMembers(SymbolUtility.GetPropertyAllMembers(property.Type), "CsTomlPackageValueAttribute"));
                }
                else if (type == CsTomlValueType.ArrayOfTables)
                {
                    if (kind != CsTomlTypeKind.TableOrArrayOfTables)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.ArrayOfTablesError,
                                property.Locations[0],
                                typeSymbol.Name, property.Name));
                        continue;
                    }

                    var attr = property.GetAttributes().Where(a =>
                        a.AttributeClass!.ContainingNamespace.Name == "CsToml" &&
                        a.AttributeClass!.Name == "CsTomlArrayOfTablesKeyAttribute").FirstOrDefault();
                    if (attr == null) continue;

                    builder.AppendLine($"        writer.Write(linefeed);");
                    builder.AppendLine($"        writer.Write(arrayOfTableStart);");
                    builder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, \"{attr.ConstructorArguments[0].Value!}\");");
                    builder.AppendLine($"        writer.Write(arrayOfTableEnd);");
                    builder.AppendLine($"        writer.Write(linefeed);");

                    GenerateProcessCodeCore(
                        context, $"{property.Name}.",
                        (INamedTypeSymbol)property.Type,
                        SymbolUtility.FilterCsTomlPackageValueMembers(SymbolUtility.GetPropertyAllMembers(property.Type), "CsTomlPackageValueAttribute"));
                }
            }
        }

        private void GenerateProcessCodeTable(SourceProductionContext context, string accessName, INamedTypeSymbol typeSymbol, IEnumerable<(IPropertySymbol, CsTomlValueType)> properties)
        {
            foreach (var (property, type) in properties)
            {
                var kind = SymbolUtility.GetCsTomlTypeKind(property.Type);
                if (kind == CsTomlTypeKind.Unknown || kind == CsTomlTypeKind.Error)
                {
                    continue;
                }

                if (type == CsTomlValueType.KeyValue)
                {
                    if (kind != CsTomlTypeKind.Primitive)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.KeyValueError,
                                property.Locations[0],
                                typeSymbol.Name, property.Name));
                        continue;
                    }
                    builder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof({typeSymbol.Name}.{property.Name}));");
                    builder.AppendLine($"        writer.Write(equals);");
                    if (property.Type.SpecialType == SpecialType.System_String)
                    {
                        builder.AppendLine($@"        if (string.IsNullOrEmpty(target.{accessName}{property.Name}))");
                        builder.AppendLine($@"            ICsTomlValueSerializer.Serialize(ref writer, []);");
                        builder.AppendLine($@"        else");
                        builder.AppendLine($@"            ICsTomlValueSerializer.Serialize(ref writer, target.{accessName}{property.Name}.AsSpan());");
                    }
                    else
                    {
                        builder.AppendLine($"        ICsTomlValueSerializer.Serialize(ref writer, target.{accessName}{property.Name});");
                    }
                    builder.AppendLine($"        writer.Write(linefeed);");

                }
                else if (type == CsTomlValueType.Array)
                {
                    if (kind != CsTomlTypeKind.Collection)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.ArrayError,
                                property.Locations[0],
                                typeSymbol.Name, property.Name));
                        continue;
                    }
                    builder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof({typeSymbol.Name}.{property.Name}));");
                    builder.AppendLine($"        writer.Write(equals);");
                    builder.AppendLine($"        ICsTomlValueSerializer.Serialize(ref writer, target.{accessName}{property.Name});");
                    builder.AppendLine($"        writer.Write(linefeed);");
                }
            }
        }

        private void GenerateProcessCodeKeyValue(SourceProductionContext context, string accessName, INamedTypeSymbol typeSymbol, IPropertySymbol property, CsTomlValueType type, CsTomlTypeKind kind)
        {
            if (kind == CsTomlTypeKind.Primitive)
            {
                builder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof({typeSymbol.Name}.{property.Name}));");
                builder.AppendLine($"        writer.Write(equals);");
                if (property.Type.SpecialType == SpecialType.System_String)
                {
                    builder.AppendLine($@"        if (string.IsNullOrEmpty(target.{accessName}{property.Name}))");
                    builder.AppendLine($@"            ICsTomlValueSerializer.Serialize(ref writer, []);");
                    builder.AppendLine($@"        else");
                    builder.AppendLine($@"            ICsTomlValueSerializer.Serialize(ref writer, target.{accessName}{property.Name}.AsSpan());");
                }
                else
                {
                    builder.AppendLine($"        ICsTomlValueSerializer.Serialize(ref writer, target.{accessName}{property.Name});");
                }
                builder.AppendLine($"        writer.Write(linefeed);");
            }
            else if (kind == CsTomlTypeKind.Unknown)
            {
                try
                {
                    var unknownBuilder = new StringBuilder();
                    unknownBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof({typeSymbol.Name}.{property.Name}));");
                    unknownBuilder.AppendLine($"        writer.Write(equals);");
                    unknownBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeDynamic(ref writer, target.{accessName}{property.Name});");
                    unknownBuilder.AppendLine($"        writer.Write(linefeed);");
                    builder.AppendLine(unknownBuilder.ToString());
                }
                catch (Exception)
                { }
            }
            else
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.KeyValueError, 
                        property.Locations[0], 
                        typeSymbol.Name, property.Name));
            }
            return;
        }
    }

}

