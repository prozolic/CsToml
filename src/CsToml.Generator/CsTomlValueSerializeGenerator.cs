using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Text;

namespace CsToml.Generator;

internal sealed class CsTomlValueSerializeGenerator
{
    private INamedTypeSymbol typeSymbol;
    private TypeDeclarationSyntax typeNode;
    private (IPropertySymbol, CsTomlValueType)[] valueMembers;
    private StringBuilder serializerBuilder;
    private StringBuilder deserializerBuilder;

    public CsTomlValueSerializeGenerator(INamedTypeSymbol typeSymbol, TypeDeclarationSyntax typeNode)
    {
        this.typeSymbol = typeSymbol;
        this.typeNode = typeNode;

        var allmembers = SymbolUtility.GetPropertyAllMembers(typeSymbol);
        valueMembers = SymbolUtility.FilterCsTomlPackageValueMembers(allmembers, "CsTomlValueOnSerializedAttribute").ToArray();
        serializerBuilder = new StringBuilder();
        deserializerBuilder = new StringBuilder();
    }

    public string GenerateSerializeProcessCode(SourceProductionContext context)
    {
        GenerateSerializerCore(context, string.Empty, typeSymbol, valueMembers);

        return serializerBuilder.ToString();
    }

    private void GenerateSerializerCore(SourceProductionContext context, string accessName, INamedTypeSymbol typeSymbol, (IPropertySymbol, CsTomlValueType)[] properties)
    {
        foreach (var (property, type) in properties)
        {
            var kind = SymbolUtility.GetCsTomlTypeKind(property.Type);
            if (kind == CsTomlTypeKind.Error)
            {
                continue;
            }

            switch (type)
            {
                case CsTomlValueType.KeyValue:
                    GenerateSerializeForKeyValue(context, accessName, typeSymbol, property, type, kind);
                    break;
                case CsTomlValueType.Array:
                    if (kind != CsTomlTypeKind.Collection)
                    {
                        context.ReportDiagnostic(Diagnostic.Create( DiagnosticDescriptors.ArrayError, property.Locations[0], typeSymbol.Name, property.Name));
                        continue;
                    }
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof({typeSymbol.Name}.{property.Name}));");
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.Serialize(ref writer, target.{accessName}{property.Name});");
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeNewLine(ref writer);");
                    break;
                case CsTomlValueType.Table:
                    if (kind != CsTomlTypeKind.TableOrArrayOfTables || !string.IsNullOrEmpty(accessName))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Table, property.Locations[0], typeSymbol.Name, property.Name));
                        continue;
                    }
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeNewLine(ref writer);");
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeTableHeader(ref writer, nameof(target.{property.Name}));");
                    GenerateSerializeForTable(
                        context, $"{property.Name}.",
                        (INamedTypeSymbol)property.Type,
                        SymbolUtility.FilterCsTomlPackageValueMembers(SymbolUtility.GetPropertyAllMembers(property.Type), "CsTomlValueOnSerializedAttribute"));
                    break;
                case CsTomlValueType.ArrayOfTables:
                    if (kind != CsTomlTypeKind.TableOrArrayOfTables || !string.IsNullOrEmpty(accessName))
                    {
                        context.ReportDiagnostic( Diagnostic.Create( DiagnosticDescriptors.ArrayOfTablesError, property.Locations[0], typeSymbol.Name, property.Name));
                        continue;
                    }

                    var attr = property.GetCsTomlArrayOfTablesKeyAttribute().FirstOrDefault();
                    if (attr == null)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.ArrayOfTablesError2,
                                property.Locations[0]));
                        continue;
                    }
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeNewLine(ref writer);");
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeArrayOfTablesHeader(ref writer, \"{attr.ConstructorArguments[0].Value!}\");");

                    GenerateSerializerCore(
                        context, $"{property.Name}.",
                        (INamedTypeSymbol)property.Type,
                        SymbolUtility.FilterCsTomlPackageValueMembers(SymbolUtility.GetPropertyAllMembers(property.Type), "CsTomlValueOnSerializedAttribute"));
                    break;
                case CsTomlValueType.InlineTable:
                    switch (kind)
                    {
                        case CsTomlTypeKind.Primitive:
                        case CsTomlTypeKind.Collection:
                        case CsTomlTypeKind.ArrayOfTables:
                            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InlineTableError, property.Locations[0], typeSymbol.Name, property.Name));
                            continue;
                    }
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof({typeSymbol.Name}.{property.Name}));");
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
                    GenerateSerializeForInlineTable(
                        context, $"{accessName}{property.Name}.",
                        (INamedTypeSymbol)property.Type,
                        SymbolUtility.FilterCsTomlPackageValueMembers(SymbolUtility.GetPropertyAllMembers(property.Type), "CsTomlValueOnSerializedAttribute")
                        , true);
                    break;
            }

        }
    }

    private void GenerateSerializeForTable(SourceProductionContext context, string accessName, INamedTypeSymbol typeSymbol, (IPropertySymbol, CsTomlValueType)[] properties)
    {
        foreach (var (property, type) in properties)
        {
            var kind = SymbolUtility.GetCsTomlTypeKind(property.Type);
            if (kind == CsTomlTypeKind.Error)
            {
                context.ReportDiagnostic(Diagnostic.Create( DiagnosticDescriptors.Table, property.Locations[0], typeSymbol.Name, property.Name));
                continue;
            }

            if (type == CsTomlValueType.KeyValue)
            {
                if (kind == CsTomlTypeKind.Primitive)
                {
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof({typeSymbol.Name}.{property.Name}));");
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
                    if (property.Type.SpecialType == SpecialType.System_String)
                    {
                        serializerBuilder.AppendLine($@"        if (string.IsNullOrEmpty(target.{accessName}{property.Name}))");
                        serializerBuilder.AppendLine($@"            ICsTomlValueSerializer.Serialize(ref writer, Span<char>.Empty);");
                        serializerBuilder.AppendLine($@"        else");
                        serializerBuilder.AppendLine($@"            ICsTomlValueSerializer.Serialize(ref writer, target.{accessName}{property.Name}.AsSpan());");
                    }
                    else
                    {
                        serializerBuilder.AppendLine($"        ICsTomlValueSerializer.Serialize(ref writer, target.{accessName}{property.Name});");
                    }
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeNewLine(ref writer);");

                }
                else if (kind == CsTomlTypeKind.Unknown)
                {
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof({typeSymbol.Name}.{property.Name}));");
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeDynamic(ref writer, target.{accessName}{property.Name});");
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeNewLine(ref writer);");
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.KeyValueError, property.Locations[0], typeSymbol.Name, property.Name));
                }
            }
            else if (type == CsTomlValueType.Array)
            {
                if (kind != CsTomlTypeKind.Collection)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ArrayError,property.Locations[0],typeSymbol.Name, property.Name));
                    continue;
                }
                serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof({typeSymbol.Name}.{property.Name}));");
                serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
                serializerBuilder.AppendLine($"        ICsTomlValueSerializer.Serialize(ref writer, target.{accessName}{property.Name});");
                serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeNewLine(ref writer);");
            }
            else if (type == CsTomlValueType.InlineTable)
            {
                switch (kind)
                {
                    case CsTomlTypeKind.Primitive:
                    case CsTomlTypeKind.Collection:
                    case CsTomlTypeKind.ArrayOfTables:
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InlineTableError, property.Locations[0], typeSymbol.Name, property.Name));
                        continue;
                }
                serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof({typeSymbol.Name}.{property.Name}));");
                serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
                GenerateSerializeForInlineTable(
                    context, $"{accessName}{property.Name}.",
                    (INamedTypeSymbol)property.Type,
                    SymbolUtility.FilterCsTomlPackageValueMembers(SymbolUtility.GetPropertyAllMembers(property.Type), "CsTomlValueOnSerializedAttribute")
                    , true);
            }
        }
    }

    private void GenerateSerializeForInlineTable(SourceProductionContext context, string accessName, INamedTypeSymbol typeSymbol, (IPropertySymbol, CsTomlValueType)[] propertiesArray, bool newline)
    {
        serializerBuilder.AppendLine("        ICsTomlValueSerializer.SerializeAnyByte(ref writer, \"{\"u8);");
        for (int i = 0; i < propertiesArray.Length; i++)
        {
            var property = propertiesArray[i].Item1;
            var type = propertiesArray[i].Item2;

            // To prevent infinite loops
            if (property.Type.Name == typeSymbol.Name)
            {
                var declaringSyntax = property.DeclaringSyntaxReferences[0].GetSyntax();
                var syntaxTrees = declaringSyntax.ChildNodes().Where(n => n is IdentifierNameSyntax).FirstOrDefault();
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ThrowNotBeDefinedBySameClass,
                    syntaxTrees?.GetLocation() ?? property.Locations[0],
                    typeSymbol.Name, property.Name));
                continue;
            }

            var kind = SymbolUtility.GetCsTomlTypeKind(property.Type);
            if (kind == CsTomlTypeKind.Error)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.KeyValueError, property.Locations[0], typeSymbol.Name, property.Name));
                continue;
            }

            switch (type)
            {
                case CsTomlValueType.KeyValue:
                    if (kind == CsTomlTypeKind.Primitive)
                    {
                        serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof({typeSymbol.Name}.{property.Name}));");
                        serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
                        if (property.Type.SpecialType == SpecialType.System_String)
                        {
                            serializerBuilder.AppendLine($@"        if (string.IsNullOrEmpty(target.{accessName}{property.Name}))");
                            serializerBuilder.AppendLine($@"            ICsTomlValueSerializer.Serialize(ref writer, Span<char>.Empty);");
                            serializerBuilder.AppendLine($@"        else");
                            serializerBuilder.AppendLine($@"            ICsTomlValueSerializer.Serialize(ref writer, target.{accessName}{property.Name}.AsSpan());");
                        }
                        else
                        {
                            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.Serialize(ref writer, target.{accessName}{property.Name});");
                        }
                    }
                    else if (kind  == CsTomlTypeKind.Unknown)
                    {
                        serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof({typeSymbol.Name}.{property.Name}));");
                        serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
                        serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeDynamic(ref writer, target.{accessName}{property.Name});");
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.KeyValueError, property.Locations[0], typeSymbol.Name, property.Name));
                    }

                    break;
                case CsTomlValueType.Array:
                    if (kind != CsTomlTypeKind.Collection)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ArrayError,property.Locations[0],typeSymbol.Name, property.Name));
                        continue;
                    }
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof({typeSymbol.Name}.{property.Name}));");
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.Serialize(ref writer, target.{accessName}{property.Name});");
                    break;
                case CsTomlValueType.InlineTable:
                    switch (kind)
                    {
                        case CsTomlTypeKind.Primitive:
                        case CsTomlTypeKind.Collection:
                        case CsTomlTypeKind.ArrayOfTables:
                            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InlineTableError, property.Locations[0], typeSymbol.Name, property.Name));
                            continue;
                    }
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof({typeSymbol.Name}.{property.Name}));");
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
                    GenerateSerializeForInlineTable(
                        context, $"{accessName}{property.Name}.",
                        (INamedTypeSymbol)property.Type,
                        SymbolUtility.FilterCsTomlPackageValueMembers(SymbolUtility.GetPropertyAllMembers(property.Type), "CsTomlValueOnSerializedAttribute"),
                        false);
                    break;
            }

            if (i < propertiesArray.Length - 1)
            {
                serializerBuilder.AppendLine("        ICsTomlValueSerializer.SerializeAnyByte(ref writer, \", \"u8);");
            }
        }

        serializerBuilder.AppendLine("        ICsTomlValueSerializer.SerializeAnyByte(ref writer, \"}\"u8);");
        if (newline)
            serializerBuilder.AppendLine("        ICsTomlValueSerializer.SerializeNewLine(ref writer);");
    }

    private void GenerateSerializeForKeyValue(SourceProductionContext context, string accessName, INamedTypeSymbol typeSymbol, IPropertySymbol property, CsTomlValueType type, CsTomlTypeKind kind)
    {
        if (kind == CsTomlTypeKind.Primitive)
        {
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof({typeSymbol.Name}.{property.Name}));");
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
            if (property.Type.SpecialType == SpecialType.System_String)
            {
                serializerBuilder.AppendLine($@"        if (string.IsNullOrEmpty(target.{accessName}{property.Name}))");
                serializerBuilder.AppendLine($@"            ICsTomlValueSerializer.Serialize(ref writer, Span<char>.Empty);");
                serializerBuilder.AppendLine($@"        else");
                serializerBuilder.AppendLine($@"            ICsTomlValueSerializer.Serialize(ref writer, target.{accessName}{property.Name}.AsSpan());");
            }
            else
            {
                serializerBuilder.AppendLine($"        ICsTomlValueSerializer.Serialize(ref writer, target.{accessName}{property.Name});");
            }
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeNewLine(ref writer);");
        }
        else if (kind == CsTomlTypeKind.Unknown)
        {
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, nameof({typeSymbol.Name}.{property.Name}));");
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeDynamic(ref writer, target.{accessName}{property.Name});");
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeNewLine(ref writer);");
        }
        else
        {
            context.ReportDiagnostic( Diagnostic.Create(DiagnosticDescriptors.KeyValueError,property.Locations[0],typeSymbol.Name, property.Name));
        }
    }

    public string GenerateDeserializeProcessCode(SourceProductionContext context)
    {
        GenerateDeserializeCore(context, string.Empty, typeSymbol, valueMembers);

        return deserializerBuilder.ToString();
    }

    private void GenerateDeserializeCore(SourceProductionContext context, string accessName, INamedTypeSymbol typeSymbol, (IPropertySymbol, CsTomlValueType)[] properties)
    {
        deserializerBuilder.AppendLine($"        var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText, options);");
        deserializerBuilder.AppendLine($"        var target = new {this.typeSymbol}();");

        foreach (var (property, type) in properties)
        {
            var kind = SymbolUtility.GetCsTomlTypeKind(property.Type);
            if (kind == CsTomlTypeKind.Error)
            {
                continue;
            }
            if (type == CsTomlValueType.KeyValue)
            {
                GenerateDeserializeForKeyValue(context, accessName, typeSymbol, property, type, kind);
                continue;
            }
            else if (type == CsTomlValueType.Table)
            {
                GenerateDeserializeForTable( context, $"{property.Name}",
                    (INamedTypeSymbol)property.Type,
                    SymbolUtility.FilterCsTomlPackageValueMembers(SymbolUtility.GetPropertyAllMembers(property.Type), "CsTomlValueOnSerializedAttribute"));
                continue;
            }
        }

        deserializerBuilder.AppendLine($"        return target;");
    }

    private void GenerateDeserializeForKeyValue(SourceProductionContext context, string accessName, INamedTypeSymbol typeSymbol, IPropertySymbol property, CsTomlValueType type, CsTomlTypeKind kind)
    {
        if (kind == CsTomlTypeKind.Primitive)
        {
            var propertyName = string.IsNullOrEmpty(accessName) ? property.Name : $"{accessName}.{property.Name}";
            var valueName = string.IsNullOrEmpty(accessName) ? property.Name : $"{accessName}_{property.Name}";

            switch (property.Type.SpecialType)
            {
                case SpecialType.System_Boolean:
                    deserializerBuilder.AppendLine($"        if (package.TryFind(nameof(target.{propertyName}), out var _{valueName}))");
                    deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetBool();");
                    break;
                case SpecialType.System_Byte:
                case SpecialType.System_SByte:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                    deserializerBuilder.AppendLine($"        if (package.TryFind(nameof(target.{propertyName}), out var _{valueName}))");
                    deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetInt64();");
                    break;
                case SpecialType.System_String:
                    deserializerBuilder.AppendLine($"        if (package.TryFind(nameof(target.{propertyName}), out var _{valueName}))");
                    deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetString();");
                    break;
                case SpecialType.System_Double:
                    deserializerBuilder.AppendLine($"        if (package.TryFind(nameof(target.{propertyName}), out var _{valueName}))");
                    deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetDouble();");
                    break;
                case SpecialType.System_DateTime:
                    deserializerBuilder.AppendLine($"        if (package.TryFind(nameof(target.{propertyName}), out var _{valueName}))");
                    deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetDateTime();");
                    break;
                default:
                    if (property.Type.TypeKind == TypeKind.Struct)
                    {
                        switch (property.Type.Name)
                        {
                            case "DateTimeOffset":
                                deserializerBuilder.AppendLine($"        if (package.TryFind(nameof(target.{propertyName}), out var _{valueName}))");
                                deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetDateTimeOffset();");
                                break;
                            case "DateOnly":
                                deserializerBuilder.AppendLine($"        if (package.TryFind(nameof(target.{propertyName}), out var _{valueName}))");
                                deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetDateOnly();");
                                break;
                            case "TimeOnly":
                                deserializerBuilder.AppendLine($"        if (package.TryFind(nameof(target.{propertyName}), out var _{valueName}))");
                                deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetTimeOnly();");
                                break;
                        }
                    }
                    break;
            }
        }
        else if (kind == CsTomlTypeKind.Unknown)
        {
            var propertyName = string.IsNullOrEmpty(accessName) ? property.Name : $"{accessName}.{property.Name}";
            var valueName = string.IsNullOrEmpty(accessName) ? property.Name : $"{accessName}_{property.Name}";

            deserializerBuilder.AppendLine($"        if (package.TryFind(nameof(target.{propertyName}), out var _{valueName}))");
            deserializerBuilder.AppendLine($"            target.{propertyName} = _{valueName}!.GetObject();");
        }
        else
        {
            context.ReportDiagnostic(Diagnostic.Create( DiagnosticDescriptors.KeyValueError, property.Locations[0], typeSymbol.Name, property.Name));
        }
    }

    private void GenerateDeserializeForTable(SourceProductionContext context, string accessName, INamedTypeSymbol typeSymbol, (IPropertySymbol, CsTomlValueType)[] properties)
    {
        foreach (var (property, type) in properties)
        {
            var kind = SymbolUtility.GetCsTomlTypeKind(property.Type);
            if (kind == CsTomlTypeKind.Error)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Table, property.Locations[0], typeSymbol.Name, property.Name));
                continue;
            }

            if (kind == CsTomlTypeKind.Primitive)
            {
                if (type == CsTomlValueType.KeyValue)
                {
                    GenerateDeserializeForKeyValue(context, accessName, (INamedTypeSymbol)property.Type, property, CsTomlValueType.None, kind);
                }
                else if (type == CsTomlValueType.Array)
                {

                }
            }
            else if (kind == CsTomlTypeKind.Unknown)
            {
                GenerateDeserializeForKeyValue(context, accessName, (INamedTypeSymbol)property.Type, property, CsTomlValueType.None, kind);
            }

        }
    }
}


