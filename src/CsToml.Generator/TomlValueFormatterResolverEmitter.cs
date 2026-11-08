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
                return;
            case TomlSerializationKind.PrimitiveArray:
                if (type is IArrayTypeSymbol { Rank: 1 } primitiveArrayType &&
                    FormatterTypeMetaData.TryGetNullableParameterType(primitiveArrayType.ElementType, out _))
                {
                    AppendRegister(fullTypeName, $"ArrayFormatter<{primitiveArrayType.ElementType.ToFullFormatString()}>");
                }
                return;
            case TomlSerializationKind.Enum:
                AppendRegister(fullTypeName, $"EnumFormatter<{fullTypeName}>");
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
                builder.AppendLine($"{indent}        TomlValueFormatterResolver.Register<{fullTypeName}>();");
                return;
            case TomlSerializationKind.TomlSerializedObjectArray:
                var arrayNamedType = (IArrayTypeSymbol)type;
                var elementType = arrayNamedType.ElementType;
                AppendRegister(fullTypeName, $"ArrayFormatter<{elementType.ToFullFormatString()}>");
                return;
            case TomlSerializationKind.TomlSerializedObjectCollection:
                if (type is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType)
                {
                    // Nullable<T> is a special case.
                    var typeSymbol = namedTypeSymbol.ConstructUnboundGenericType();
                    if (typeSymbol.ToDisplayString() == "T?")
                    {
                        AppendRegister(fullTypeName, $"NullableFormatter<{namedTypeSymbol.TypeArguments[0].ToFullFormatString()}>");
                        return;
                    }

                    if (FormatterTypeMetaData.TryGetGenericFormatterType(typeSymbol.ToFullFormatString(), out var typeFormatter) != GenericFormatterType.None)
                    {
                        var typeParameters = string.Join(",", namedTypeSymbol.TypeArguments.Select(x => x.ToFullFormatString()));
                        AppendRegister(fullTypeName, typeFormatter!.Replace("TYPEPARAMETER", typeParameters));
                        return;
                    }
                }
                else
                {
                    if (FormatterTypeMetaData.TryGetGenericFormatterType(type, out var formatter) != GenericFormatterType.None)
                    {
                        var collectionNamedType = (INamedTypeSymbol)type;
                        var typeParameters = string.Join(",", collectionNamedType.TypeArguments.Select(x => x.ToFullFormatString()));
                        AppendRegister(fullTypeName, formatter!.Replace("TYPEPARAMETER", typeParameters));
                    }
                }

                return;
            case TomlSerializationKind.Dictionary:
                if (FormatterTypeMetaData.TryGetGenericFormatterType(type, out var dictFormatter) != GenericFormatterType.None)
                {
                    var dictNamedType = (INamedTypeSymbol)type;
                    var typeParameters = string.Join(",", dictNamedType.TypeArguments.Select(x => x.ToFullFormatString()));
                    AppendRegister(fullTypeName, dictFormatter!.Replace("TYPEPARAMETER", typeParameters));
                }
                return;
            case TomlSerializationKind.NullableStructWithTypeParameter:
                if (type is not INamedTypeSymbol namedType) return;

                AppendRegister(namedType.ToFullFormatString(), $"NullableFormatter<{namedType.TypeArguments[0].ToFullFormatString()}>");
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
                        AppendRegister(fullTypeName, $"NullableFormatter<{namedTypeSymbol2.TypeArguments[0].ToFullFormatString()}>");
                        return;
                    }

                    if (FormatterTypeMetaData.TryGetGenericFormatterType(typeSymbol.ToFullFormatString(), out var typeFormatter) != GenericFormatterType.None)
                    {
                        var typeParameters = string.Join(",", namedTypeSymbol2.TypeArguments.Select(x => x.ToFullFormatString()));
                        AppendRegister(fullTypeName, typeFormatter!.Replace("TYPEPARAMETER", typeParameters));
                        return;
                    }
                }
                return;
        }
    }

    private void AppendRegister(string typeName, string formatterTypeName)
    {
        builder.AppendLine($"{indent}        TomlValueFormatterResolver.Register<{typeName}, {formatterTypeName}>();");
    }

    public override string ToString()
    {
        return builder.ToString();
    }
}
