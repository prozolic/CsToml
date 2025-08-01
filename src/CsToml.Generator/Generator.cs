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
using System.Diagnostics.CodeAnalysis;

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
        var constructorMeta = new ConstructorMeta(symbol, typeNode, typeMeta);

        if (!(typeMeta.Validate(context) && constructorMeta.Validate(context)))
            return;

        context.AddSource($"{typeMeta.TypeName}_generated.g.cs", Generate(typeMeta, constructorMeta));
    }

    private string Generate(TypeMeta typeMeta, ConstructorMeta constructorMeta)
    {
        // Check if it belongs to the global namespace.
        var namespaceTag = string.IsNullOrWhiteSpace(typeMeta.NameSpace) ? string.Empty : $"namespace {typeMeta.NameSpace};";

        var code = $$"""
// <auto-generated> This .cs file is generated by CsToml.Generator. </auto-generated>
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
{{GenerateDeserializePart(typeMeta, constructorMeta)}}    }

    static void ITomlSerializedObject<{{typeMeta.TypeName}}>.Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, {{typeMeta.TypeName}} target, CsTomlSerializerOptions options)
    {
{{GenerateSerializePart(typeMeta)}}    }

    static void ITomlSerializedObjectRegister.Register()
    {
{{GenerateRegisterPart(typeMeta)}}
    }
}
""";

        return code;
    }

    private string GenerateDeserializePart(TypeMeta typeMeta, ConstructorMeta constructorMeta)
    {
        var builder = new StringBuilder();

        foreach (var member in typeMeta.Members)
        {
            var propertyName = member.DefinedName;
            var accessName = member.CanAliasName ? member.AliasName : propertyName;

            builder.AppendLine($"        var __{propertyName}__RootNode = rootNode[{$"@\"{accessName}\"u8"}];");
            builder.AppendLine($"        var __{propertyName}__ = options.Resolver.GetFormatter<{member.Symbol.Type.ToFullFormatString()}>()!.Deserialize(ref __{propertyName}__RootNode, options);");
        }

        builder.AppendLine();

        if (constructorMeta.IsImplicitlyDeclared || constructorMeta.IsParameterlessOnly || (constructorMeta.ConstructorParameters.Length == 0 && constructorMeta.IncludeParameterless))
        {
            builder.AppendLine($"        var target = new {typeMeta.TypeName}(){{");
            foreach (var member in typeMeta.Members)
            {
                var propertyName = member.DefinedName;
                builder.AppendLine($"            {propertyName} = __{propertyName}__,");
            }
            builder.AppendLine($"        }};");
            builder.AppendLine();
            builder.AppendLine($"        return target;");
        }
        else
        {
            builder.Append($"        var target = new {typeMeta.TypeName}(");

            for (var i = 0; i < constructorMeta.ConstructorParameterProperties.Length; i++)
            {
                var p = constructorMeta.ConstructorParameterProperties[i];
                var propertyName = p.Name;
                builder.Append($"__{propertyName}__");
                if (i < constructorMeta.ConstructorParameters.Length - 1)
                {
                    builder.Append(", ");
                }
            }
            builder.Append($")");

            if (constructorMeta.MembersOfObjectInitialisers.Length > 0)
            {
                builder.AppendLine($"{{");
                foreach (var property in constructorMeta.MembersOfObjectInitialisers)
                {
                    var propertyName = property.Name;
                    builder.AppendLine($"            {propertyName} = __{propertyName}__,");
                }
                builder.AppendLine($"        }};");
            }
            else
            {
                builder.AppendLine(";");
            }
            builder.AppendLine($"        return target;");

        }

        return builder.ToString();
    }

    private string GenerateSerializePart(TypeMeta typeMeta)
    {
        var builder = new StringBuilder();

        var members = typeMeta.Members;
        var onlyTomlSerializedObject = members.Length == 1 && members[0].SerializationKind == TomlSerializationKind.TomlSerializedObject;
        if (!onlyTomlSerializedObject)
        {
            builder.AppendLine("        writer.BeginScope();");
        }

        var memberCount = 0;
        foreach (var member in members)
        {
            var propertyName = member.DefinedName;
            var accessName = member.CanAliasName ? member.AliasName : propertyName;
            var kind = member.SerializationKind;
            var symbol = member.Symbol;

            memberCount++;

            if (kind == TomlSerializationKind.Primitive || kind == TomlSerializationKind.Object)
            {
                builder.AppendLine($"        writer.WriteKey({$"@\"{accessName}\"u8"});");
                builder.AppendLine($"        writer.WriteEqual();");
                builder.AppendLine($"        options.Resolver.GetFormatter<{symbol.Type.ToFullFormatString()}>()!.Serialize(ref writer, target.{propertyName}, options);");
                if (memberCount == members.Length)
                {
                    builder.AppendLine($"        writer.EndKeyValue(true);");
                }
                else
                {
                    builder.AppendLine($"        writer.EndKeyValue();");
                }
            }
            else if (kind == TomlSerializationKind.TomlSerializedObject)
            {
                builder.AppendLine($"        if (options.SerializeOptions.TableStyle == TomlTableStyle.Header && (writer.State == TomlValueState.Default || writer.State == TomlValueState.Table)){{");
                builder.AppendLine($"            writer.WriteTableHeader({$"@\"{accessName}\"u8"});");
                builder.AppendLine($"            writer.WriteNewLine();");
                builder.AppendLine($"            writer.BeginCurrentState(TomlValueState.Table);");
                builder.AppendLine($"            writer.PushKey({$"@\"{accessName}\"u8"});");
                builder.AppendLine($"            options.Resolver.GetFormatter<{symbol.Type.ToFullFormatString()}>()!.Serialize(ref writer, target.{propertyName}, options);");
                builder.AppendLine($"            writer.PopKey();");
                builder.AppendLine($"            writer.EndCurrentState();");
                builder.AppendLine($"        }}");
                builder.AppendLine($"        else");
                builder.AppendLine($"        {{");
                builder.AppendLine($"            writer.PushKey({$"@\"{accessName}\"u8"});");
                builder.AppendLine($"            options.Resolver.GetFormatter<{symbol.Type.ToFullFormatString()}>()!.Serialize(ref writer, target.{propertyName}, options);");
                builder.AppendLine($"            writer.PopKey();");
                builder.AppendLine($"        }}");
            }
            else if (kind == TomlSerializationKind.Dictionary)
            {
                builder.AppendLine($"        if (options.SerializeOptions.TableStyle == TomlTableStyle.Header && (writer.State == TomlValueState.Default || writer.State == TomlValueState.Table)){{");
                builder.AppendLine($"            writer.WriteTableHeader({$"@\"{accessName}\"u8"});");
                builder.AppendLine($"            writer.WriteNewLine();");
                builder.AppendLine($"            writer.BeginCurrentState(TomlValueState.Table);");
                builder.AppendLine($"            writer.PushKey({$"@\"{accessName}\"u8"});");
                builder.AppendLine($"            options.Resolver.GetFormatter<{symbol.Type.ToFullFormatString()}>()!.Serialize(ref writer, target.{propertyName}, options);");
                builder.AppendLine($"            writer.PopKey();");
                builder.AppendLine($"            writer.EndCurrentState();");
                builder.AppendLine($"        }}");
                builder.AppendLine($"        else");
                builder.AppendLine($"        {{");
                builder.AppendLine($"            writer.WriteKey({$"@\"{accessName}\"u8"});");
                builder.AppendLine($"            writer.WriteEqual();");
                builder.AppendLine($"            writer.BeginCurrentState(TomlValueState.ArrayOfTable);");
                builder.AppendLine($"            options.Resolver.GetFormatter<{symbol.Type.ToFullFormatString()}>()!.Serialize(ref writer, target.{propertyName}, options);");
                builder.AppendLine($"            writer.EndCurrentState();");
                if (memberCount == members.Length)
                {
                    builder.AppendLine($"            writer.EndKeyValue(true);");
                }
                else
                {
                    builder.AppendLine($"            writer.EndKeyValue();");
                }
                builder.AppendLine($"        }}");
            }
            else if (kind == TomlSerializationKind.ArrayOfITomlSerializedObject || kind == TomlSerializationKind.CollectionOfITomlSerializedObject)
            {
                builder.AppendLine($"        writer.WriteKey({$"@\"{accessName}\"u8"});");
                builder.AppendLine($"        writer.WriteEqual();");
                builder.AppendLine($"        writer.BeginCurrentState(TomlValueState.ArrayOfTable);");
                builder.AppendLine($"        options.Resolver.GetFormatter<{symbol.Type.ToFullFormatString()}>()!.Serialize(ref writer, target.{propertyName}, options);");
                builder.AppendLine($"        writer.EndCurrentState();");
                if (memberCount == members.Length)
                {
                    builder.AppendLine($"        writer.EndKeyValue(true);");
                }
                else
                {
                    builder.AppendLine($"        writer.EndKeyValue();");
                }
            }
            else
            {
                builder.AppendLine($"        writer.WriteKey({$"@\"{accessName}\"u8"});");
                builder.AppendLine($"        writer.WriteEqual();");
                builder.AppendLine($"        options.Resolver.GetFormatter<{symbol.Type.ToFullFormatString()}>()!.Serialize(ref writer, target.{propertyName}, options);");
                if (memberCount == typeMeta.Members.Length)
                {
                    builder.AppendLine($"        writer.EndKeyValue(true);");
                }
                else
                {
                    builder.AppendLine($"        writer.EndKeyValue();");
                }
            }
        }
        if (!onlyTomlSerializedObject)
        {
            builder.AppendLine("        writer.EndScope();");
        }
        return builder.ToString();
    }

    private string GenerateRegisterPart(TypeMeta typeMeta)
    {
        var builder = new StringBuilder();
        foreach (var (type, kind) in typeMeta.DefinedTypes)
        {
            switch (kind)
            {
                case TomlSerializationKind.Primitive:
                case TomlSerializationKind.PrimitiveArray:
                    continue;
                case TomlSerializationKind.Enum:
                    builder.AppendLine($$"""
        if (!TomlValueFormatterResolver.IsRegistered<{{type.ToFullFormatString()}}>())
        {
            TomlValueFormatterResolver.Register(new EnumFormatter<{{type.ToFullFormatString()}}>());
        }
""");
                    break;
                case TomlSerializationKind.TomlSerializedObject:
                    builder.AppendLine($$"""
        if (!TomlValueFormatterResolver.IsRegistered<{{type.ToFullFormatString()}}>())
        {
            TomlValueFormatterResolver.Register<{{type.ToFullFormatString()}}>();
        }
""");
                    continue;
                case TomlSerializationKind.ArrayOfITomlSerializedObject:
                    var arrayNamedType = (IArrayTypeSymbol)type;
                    var elementType = arrayNamedType.ElementType;

                    builder.AppendLine($$"""
        if (!TomlValueFormatterResolver.IsRegistered<{{type.ToFullFormatString()}}>())
        {
            TomlValueFormatterResolver.Register(new ArrayFormatter<{{elementType.ToFullFormatString()}}>());
        }
""");
                    break;
                case TomlSerializationKind.CollectionOfITomlSerializedObject:
                    if (FormatterTypeMetaData.TryGetGenericFormatterType(type, out var formatter) != GenericFormatterType.None)
                    {
                        var collectionNamedType = (INamedTypeSymbol)type;
                        var typeParameters = string.Join(",", collectionNamedType.TypeArguments.Select(x => x.ToFullFormatString()));
                        formatter = formatter!.Replace("TYPEPARAMETER", typeParameters);

                        builder.AppendLine($$"""
        if (!TomlValueFormatterResolver.IsRegistered<{{type.ToFullFormatString()}}>())
        {
            TomlValueFormatterResolver.Register(new {{formatter}}());
        }
""");
                    }
                    break;
                case TomlSerializationKind.Dictionary:
                    if (FormatterTypeMetaData.TryGetGenericFormatterType(type, out var dictFormatter) != GenericFormatterType.None)
                    {
                        var dictNamedType = (INamedTypeSymbol)type;
                        var typeParameters = string.Join(",", dictNamedType.TypeArguments.Select(x => x.ToFullFormatString()));
                        dictFormatter = dictFormatter!.Replace("TYPEPARAMETER", typeParameters);

                        builder.AppendLine($$"""
        if (!TomlValueFormatterResolver.IsRegistered<{{type.ToFullFormatString()}}>())
        {
            TomlValueFormatterResolver.Register(new {{dictFormatter}}());
        }
""");
                    }
                    break;
                default:
                    if (FormatterTypeMetaData.ContainsBuiltInFormatterType(type))
                        break;

                    if (type is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType)
                    {
                        // Nullable<T> is a special case.
                        var typeSymbol = namedTypeSymbol.ConstructUnboundGenericType();
                        if (typeSymbol.ToDisplayString() == "T?")
                        {
                            builder.AppendLine($$"""
        if (!TomlValueFormatterResolver.IsRegistered<{{type.ToFullFormatString()}}>())
        {
            TomlValueFormatterResolver.Register(new NullableFormatter<{{namedTypeSymbol.TypeArguments[0].ToFullFormatString()}}>());
        }
""");
                            break;
                        }

                        if (FormatterTypeMetaData.TryGetGenericFormatterType(typeSymbol.ToFullFormatString(), out var typeFormatter) != GenericFormatterType.None)
                        {
                            var typeParameters = string.Join(",", namedTypeSymbol.TypeArguments.Select(x => x.ToFullFormatString()));
                            typeFormatter = typeFormatter.Replace("TYPEPARAMETER", typeParameters);

                            builder.AppendLine($$"""
        if (!TomlValueFormatterResolver.IsRegistered<{{type.ToFullFormatString()}}>())
        {
            TomlValueFormatterResolver.Register(new {{typeFormatter}}());
        }
""");
                            break;
                        }
                    }
                    break;
            }
        }

        var code = $$"""
        if (!TomlValueFormatterResolver.IsRegistered<{{typeMeta.TypeName}}>())
        {
            TomlValueFormatterResolver.Register(new TomlSerializedObjectFormatter<{{typeMeta.TypeName}}>());
        }

        // Register Formatter in advance.
{{builder}}
""";
        return code;
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
