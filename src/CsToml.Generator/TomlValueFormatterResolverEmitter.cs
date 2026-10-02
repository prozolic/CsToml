using Microsoft.CodeAnalysis;
using System.Text;

namespace CsToml.Generator;

internal sealed class TomlValueFormatterResolverEmitter
{
    private readonly StringBuilder builder = new();
    private readonly string indent;

    public TomlValueFormatterResolverEmitter(string indent = "")
    {
        this.indent = indent;
    }

    public void Append(ITypeSymbol type, TomlSerializationKind kind)
    {
        var fullTypeName = type.ToFullFormatString();
        switch (kind)
        {
            case TomlSerializationKind.Primitive:
            case TomlSerializationKind.PrimitiveArray:
                return;
            case TomlSerializationKind.Enum:
                builder.AppendLine($$"""
{{indent}}        if (!TomlValueFormatterResolver.IsRegistered<{{fullTypeName}}>())
{{indent}}        {
{{indent}}            TomlValueFormatterResolver.Register(new EnumFormatter<{{fullTypeName}}>());
{{indent}}        }
""");
                return;
            case TomlSerializationKind.Union:
                // Only a type-level [TomlUnion<T>] produces a resolver-registered formatter; unions pinned at
                // member level are referenced directly by the member code and have nothing to register.
                if (type is INamedTypeSymbol unionSymbol && UnionSymbolAnalyzer.GetTypeLevelPinnedCase(unionSymbol) != null)
                {
                    builder.AppendLine($"{indent}        {UnionSymbolAnalyzer.GetFormatterReference(unionSymbol)}.Register();");
                }
                return;
            case TomlSerializationKind.TomlSerializedObject:
                builder.AppendLine($$"""
{{indent}}        if (!TomlValueFormatterResolver.IsRegistered<{{fullTypeName}}>())
{{indent}}        {
{{indent}}            TomlValueFormatterResolver.Register<{{fullTypeName}}>();
{{indent}}        }
""");
                return;
            case TomlSerializationKind.TomlSerializedObjectArray:
                var arrayNamedType = (IArrayTypeSymbol)type;
                var elementType = arrayNamedType.ElementType;

                builder.AppendLine($$"""
{{indent}}        if (!TomlValueFormatterResolver.IsRegistered<{{fullTypeName}}>())
{{indent}}        {
{{indent}}            TomlValueFormatterResolver.Register(new ArrayFormatter<{{elementType.ToFullFormatString()}}>());
{{indent}}        }
""");
                return;
            case TomlSerializationKind.TomlSerializedObjectCollection:
                if (type is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType)
                {
                    // Nullable<T> is a special case.
                    var typeSymbol = namedTypeSymbol.ConstructUnboundGenericType();
                    if (typeSymbol.ToDisplayString() == "T?")
                    {
                        builder.AppendLine($$"""
{{indent}}        if (!TomlValueFormatterResolver.IsRegistered<{{fullTypeName}}>())
{{indent}}        {
{{indent}}            TomlValueFormatterResolver.Register(new NullableFormatter<{{namedTypeSymbol.TypeArguments[0].ToFullFormatString()}}>());
{{indent}}        }
""");
                        return;
                    }

                    if (FormatterTypeMetaData.TryGetGenericFormatterType(typeSymbol.ToFullFormatString(), out var typeFormatter) != GenericFormatterType.None)
                    {
                        var typeParameters = string.Join(",", namedTypeSymbol.TypeArguments.Select(x => x.ToFullFormatString()));
                        typeFormatter = typeFormatter!.Replace("TYPEPARAMETER", typeParameters);

                        builder.AppendLine($$"""
{{indent}}        if (!TomlValueFormatterResolver.IsRegistered<{{fullTypeName}}>())
{{indent}}        {
{{indent}}            TomlValueFormatterResolver.Register(new {{typeFormatter}}());
{{indent}}        }
""");
                        return;
                    }
                }
                else
                {
                    if (FormatterTypeMetaData.TryGetGenericFormatterType(type, out var formatter) != GenericFormatterType.None)
                    {
                        var collectionNamedType = (INamedTypeSymbol)type;
                        var typeParameters = string.Join(",", collectionNamedType.TypeArguments.Select(x => x.ToFullFormatString()));
                        formatter = formatter!.Replace("TYPEPARAMETER", typeParameters);

                        builder.AppendLine($$"""
{{indent}}        if (!TomlValueFormatterResolver.IsRegistered<{{fullTypeName}}>())
{{indent}}        {
{{indent}}            TomlValueFormatterResolver.Register(new {{formatter}}());
{{indent}}        }
""");
                    }
                }

                return;
            case TomlSerializationKind.Dictionary:
                if (FormatterTypeMetaData.TryGetGenericFormatterType(type, out var dictFormatter) != GenericFormatterType.None)
                {
                    var dictNamedType = (INamedTypeSymbol)type;
                    var typeParameters = string.Join(",", dictNamedType.TypeArguments.Select(x => x.ToFullFormatString()));
                    dictFormatter = dictFormatter!.Replace("TYPEPARAMETER", typeParameters);

                    builder.AppendLine($$"""
{{indent}}        if (!TomlValueFormatterResolver.IsRegistered<{{fullTypeName}}>())
{{indent}}        {
{{indent}}            TomlValueFormatterResolver.Register(new {{dictFormatter}}());
{{indent}}        }
""");
                }
                return;
            case TomlSerializationKind.NullableStructWithTypeParameter:
                if (type is not INamedTypeSymbol namedType) return;

                var namedTypeName = namedType.ToFullFormatString();
                builder.AppendLine($$"""
{{indent}}        if (!TomlValueFormatterResolver.IsRegistered<{{namedTypeName}}>())
{{indent}}        {
{{indent}}            TomlValueFormatterResolver.Register(new NullableFormatter<{{namedType.TypeArguments[0].ToFullFormatString()}}>());
{{indent}}        }
""");

                return;
            default:
                if (FormatterTypeMetaData.ContainsBuiltInFormatterType(type))
                    return;

                if (type is INamedTypeSymbol namedTypeSymbol2 && namedTypeSymbol2.IsGenericType)
                {
                    // Nullable<T> is a special case.
                    var typeSymbol = namedTypeSymbol2.ConstructUnboundGenericType();
                    if (typeSymbol.ToDisplayString() == "T?")
                    {
                        builder.AppendLine($$"""
{{indent}}        if (!TomlValueFormatterResolver.IsRegistered<{{fullTypeName}}>())
{{indent}}        {
{{indent}}            TomlValueFormatterResolver.Register(new NullableFormatter<{{namedTypeSymbol2.TypeArguments[0].ToFullFormatString()}}>());
{{indent}}        }
""");
                        return;
                    }

                    if (FormatterTypeMetaData.TryGetGenericFormatterType(typeSymbol.ToFullFormatString(), out var typeFormatter) != GenericFormatterType.None)
                    {
                        var typeParameters = string.Join(",", namedTypeSymbol2.TypeArguments.Select(x => x.ToFullFormatString()));
                        typeFormatter = typeFormatter!.Replace("TYPEPARAMETER", typeParameters);

                        builder.AppendLine($$"""
{{indent}}        if (!TomlValueFormatterResolver.IsRegistered<{{fullTypeName}}>())
{{indent}}        {
{{indent}}            TomlValueFormatterResolver.Register(new {{typeFormatter}}());
{{indent}}        }
""");
                        return;
                    }
                }
                return;
        }
    }

    public override string ToString()
    {
        return builder.ToString();
    }
}
