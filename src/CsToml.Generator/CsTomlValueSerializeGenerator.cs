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

        var allmembers = SymbolUtility.GetProperties(typeSymbol);
        valueMembers = SymbolUtility.FilterCsTomlPackageValueMembers(allmembers, "CsTomlValueOnSerializedAttribute").ToArray();
        serializerBuilder = new StringBuilder();
        deserializerBuilder = new StringBuilder();
    }

    public string GenerateSerializeProcessCode(SourceProductionContext context)
    {
        GenerateSerializerCore(context, typeSymbol, valueMembers);

        return serializerBuilder.ToString();
    }

    private void GenerateSerializerCore(SourceProductionContext context, INamedTypeSymbol typeSymbol, (IPropertySymbol, CsTomlValueType)[] properties)
    {
        foreach (var (property, type) in properties)
        {
            var kind = SymbolUtility.GetCsTomlTypeKind(property.Type);
            if (kind == CsTomlTypeKind.Error) 
            {
                continue;
            }

            var accessNames = new List<string> { property.Name };
            switch (type)
            {
                case CsTomlValueType.KeyValue:
                case CsTomlValueType.Array:
                    GenerateSerializeForKeyValue(context, accessNames, typeSymbol, property, type, kind);
                    break;
                case CsTomlValueType.Table:
                    switch (kind)
                    {
                        case CsTomlTypeKind.Primitive:
                        case CsTomlTypeKind.Collection:
                        case CsTomlTypeKind.ArrayOfTables:
                            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.TableError, property.Locations[0], typeSymbol.Name, property.Name));
                            continue;
                    }

                    GenerateSerializeForTable(
                        context, accessNames,
                        (INamedTypeSymbol)property.Type,
                        SymbolUtility.FilterCsTomlPackageValueMembers(SymbolUtility.GetProperties(property.Type), "CsTomlValueOnSerializedAttribute"));
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
                    GenerateSerializeForInlineTable(
                        context, accessNames, (INamedTypeSymbol)property.Type, property,
                        SymbolUtility.FilterCsTomlPackageValueMembers(SymbolUtility.GetProperties(property.Type), "CsTomlValueOnSerializedAttribute")
                        , true);
                    break;
            }

        }

        return;
    }

    private void GenerateSerializeForKeyValue(SourceProductionContext context, List<string> accessName, INamedTypeSymbol typeSymbol, IPropertySymbol property, CsTomlValueType type, CsTomlTypeKind kind)
    {
        var propertyName = SymbolUtility.GetPropertyAccessName(accessName, ".");

        if (kind == CsTomlTypeKind.Primitive)
        {
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, \"{property.Name}\");");
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
            if (property.Type.SpecialType == SpecialType.System_String)
            {
                serializerBuilder.AppendLine($@"        if (string.IsNullOrEmpty(target.{propertyName}))");
                serializerBuilder.AppendLine($@"            ICsTomlValueSerializer.Serialize(ref writer, Span<char>.Empty);");
                serializerBuilder.AppendLine($@"        else");
                serializerBuilder.AppendLine($@"            ICsTomlValueSerializer.Serialize(ref writer, target.{propertyName}.AsSpan());");
            }
            else
            {
                serializerBuilder.AppendLine($"        ICsTomlValueSerializer.Serialize(ref writer, target.{propertyName});");
            }
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeNewLine(ref writer);");
        }
        else if (kind == CsTomlTypeKind.Collection)
        {
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, \"{property.Name}\");");
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.Serialize(ref writer, target.{propertyName});");
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeNewLine(ref writer);");
        }
        else if (kind == CsTomlTypeKind.Unknown)
        {
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, \"{property.Name}\");");
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeDynamic(ref writer, target.{propertyName});");
            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeNewLine(ref writer);");
        }
        else
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.KeyValueError, property.Locations[0], typeSymbol.Name, property.Name));
        }
    }

    private void GenerateSerializeForTable(SourceProductionContext context, List<string> accessNames, INamedTypeSymbol typeSymbol, (IPropertySymbol, CsTomlValueType)[] properties)
    {
        var findName = SymbolUtility.GetFindKey(accessNames);
        var propertyName = SymbolUtility.GetPropertyAccessName(accessNames, ".");

        serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeNewLine(ref writer);");
        serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeTableHeader(ref writer, \"{propertyName}\");");

        foreach (var (property, type) in properties)
        {
            var kind = SymbolUtility.GetCsTomlTypeKind(property.Type);
            if (kind == CsTomlTypeKind.Error)
            {
                context.ReportDiagnostic(Diagnostic.Create( DiagnosticDescriptors.TableError, property.Locations[0], typeSymbol.Name, property.Name));
                continue;
            }

            switch (type)
            {
                case CsTomlValueType.KeyValue:
                case CsTomlValueType.Array:
                    accessNames.Add(property.Name);
                    GenerateSerializeForKeyValue(context, accessNames, typeSymbol, property, type, kind);
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
                case CsTomlValueType.Table:
                    // To prevent infinite loops
                    if (property.Type.Name == typeSymbol.Name)
                    {
                        var declaringSyntax = property.DeclaringSyntaxReferences[0].GetSyntax();
                        var syntaxTrees = declaringSyntax.ChildNodes().Where(n => n is IdentifierNameSyntax).FirstOrDefault();
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ThrowNotBeDefinedBySameClass, syntaxTrees?.GetLocation() ?? property.Locations[0], property.Type.Name));
                        continue;
                    }

                    accessNames.Add(property.Name);
                    GenerateSerializeForTable(
                        context, accessNames,
                        (INamedTypeSymbol)property.Type,
                        SymbolUtility.FilterCsTomlPackageValueMembers(SymbolUtility.GetProperties(property.Type), "CsTomlValueOnSerializedAttribute"));
                    accessNames.RemoveAt(accessNames.Count - 1);
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
                    accessNames.Add(property.Name);
                    GenerateSerializeForInlineTable(
                        context, accessNames,
                        (INamedTypeSymbol)property.Type,
                        property,
                        SymbolUtility.FilterCsTomlPackageValueMembers(SymbolUtility.GetProperties(property.Type), "CsTomlValueOnSerializedAttribute")
                        , true);
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
            }
        }
    }

    private void GenerateSerializeForInlineTable(SourceProductionContext context, List<string> accessNames, INamedTypeSymbol typeSymbol, IPropertySymbol property, (IPropertySymbol, CsTomlValueType)[] propertiesArray, bool newline)
    {
        serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, \"{property.Name}\");");
        serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
        serializerBuilder.AppendLine("        ICsTomlValueSerializer.SerializeAnyByte(ref writer, \"{\"u8);");

        for (int i = 0; i < propertiesArray.Length; i++)
        {
            var inlineProperty = propertiesArray[i].Item1;
            var type = propertiesArray[i].Item2;

            // To prevent infinite loops
            if (inlineProperty.Type.Name == typeSymbol.Name)
            {
                var declaringSyntax = inlineProperty.DeclaringSyntaxReferences[0].GetSyntax();
                var syntaxTrees = declaringSyntax.ChildNodes().Where(n => n is IdentifierNameSyntax).FirstOrDefault();
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ThrowNotBeDefinedBySameClass, syntaxTrees?.GetLocation() ?? inlineProperty.Locations[0], property.Type.Name));
                continue;
            }

            var kind = SymbolUtility.GetCsTomlTypeKind(inlineProperty.Type);
            if (kind == CsTomlTypeKind.Error)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.KeyValueError, inlineProperty.Locations[0], typeSymbol.Name, inlineProperty.Name));
                continue;
            }

            accessNames.Add(inlineProperty.Name);
            var inlineTablePropertyName = SymbolUtility.GetPropertyAccessName(accessNames, ".");
            accessNames.RemoveAt(accessNames.Count - 1);

            switch (type)
            {
                case CsTomlValueType.KeyValue:
                    if (kind == CsTomlTypeKind.Primitive)
                    {
                        serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, \"{inlineProperty.Name}\");");
                        serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
                        if (inlineProperty.Type.SpecialType == SpecialType.System_String)
                        {
                            serializerBuilder.AppendLine($@"        if (string.IsNullOrEmpty(target.{inlineTablePropertyName}))");
                            serializerBuilder.AppendLine($@"            ICsTomlValueSerializer.Serialize(ref writer, Span<char>.Empty);");
                            serializerBuilder.AppendLine($@"        else");
                            serializerBuilder.AppendLine($@"            ICsTomlValueSerializer.Serialize(ref writer, target.{inlineTablePropertyName}.AsSpan());");
                        }
                        else
                        {
                            serializerBuilder.AppendLine($"        ICsTomlValueSerializer.Serialize(ref writer, target.{inlineTablePropertyName});");
                        }
                    }
                    else if (kind  == CsTomlTypeKind.Unknown)
                    {
                        serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, \"{inlineProperty.Name}\");");
                        serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
                        serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeDynamic(ref writer, target.{inlineTablePropertyName});");
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.KeyValueError, inlineProperty.Locations[0], typeSymbol.Name, inlineProperty.Name));
                    }

                    break;
                case CsTomlValueType.Array:
                    if (kind != CsTomlTypeKind.Collection)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ArrayError,inlineProperty.Locations[0],typeSymbol.Name, inlineProperty.Name));
                        continue;
                    }
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeKey(ref writer, \"{inlineProperty.Name}\");");
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.SerializeEqual(ref writer);");
                    serializerBuilder.AppendLine($"        ICsTomlValueSerializer.Serialize(ref writer, target.{inlineTablePropertyName});");
                    break;
                case CsTomlValueType.InlineTable:
                    switch (kind)
                    {
                        case CsTomlTypeKind.Primitive:
                        case CsTomlTypeKind.Collection:
                        case CsTomlTypeKind.ArrayOfTables:
                            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InlineTableError, inlineProperty.Locations[0], typeSymbol.Name, inlineProperty.Name));
                            continue;
                    }
                    accessNames.Add(inlineProperty.Name);
                    GenerateSerializeForInlineTable(
                        context, accessNames,
                        (INamedTypeSymbol)inlineProperty.Type,
                        inlineProperty,
                        SymbolUtility.FilterCsTomlPackageValueMembers(SymbolUtility.GetProperties(inlineProperty.Type), "CsTomlValueOnSerializedAttribute"),
                        false);
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
                case CsTomlValueType.Table:
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InlineTableError2, property.Locations[0], property.Type.Name,inlineProperty.Name));
                    continue;
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

    public string GenerateDeserializeProcessCode(SourceProductionContext context)
    {
        GenerateDeserializeCore(context, string.Empty, typeSymbol, valueMembers);

        return deserializerBuilder.ToString();
    }

    private void GenerateDeserializeCore(SourceProductionContext context, string accessName, INamedTypeSymbol typeSymbol, (IPropertySymbol, CsTomlValueType)[] properties)
    {
        deserializerBuilder.AppendLine($"        var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText, options);");
        deserializerBuilder.AppendLine($"        var target = new {this.typeSymbol}();");

        var arrayOftableProperties = new List<(IPropertySymbol, CsTomlValueType, AttributeData)>();
        foreach (var (property, type) in properties)
        {
            var kind = SymbolUtility.GetCsTomlTypeKind(property.Type);
            if (kind == CsTomlTypeKind.Error)
            {
                continue;
            }

            var accessNames = new List<string> { property.Name };
            switch(type)
            {
                case CsTomlValueType.KeyValue:
                    GenerateDeserializeForKeyValue(context, accessNames, typeSymbol, property, type, kind);
                    break;
                case CsTomlValueType.Array:
                    GenerateDeserializeArray(context, accessNames, typeSymbol, property, type, kind);
                    break;
                case CsTomlValueType.Table:
                    GenerateDeserializeForTable(context, accessNames,
                    (INamedTypeSymbol)property.Type,
                    SymbolUtility.FilterCsTomlPackageValueMembers(SymbolUtility.GetProperties(property.Type), "CsTomlValueOnSerializedAttribute"));
                    break;
                case CsTomlValueType.InlineTable:
                    GenerateDeserializeForInlineTable(context, accessNames,
                    (INamedTypeSymbol)property.Type,
                    SymbolUtility.FilterCsTomlPackageValueMembers(SymbolUtility.GetProperties(property.Type), "CsTomlValueOnSerializedAttribute"));
                    break;
            }


        }

        if (arrayOftableProperties.Count > 0)
        {
            //GenerateDeserializeForArrayOfTables(arrayOftableProperties, context);
        }
        deserializerBuilder.AppendLine($"        return target;");
    }

    private void GenerateDeserializeForKeyValue(SourceProductionContext context, List<string> accessName, INamedTypeSymbol typeSymbol, IPropertySymbol property, CsTomlValueType type, CsTomlTypeKind kind)
    {
        if (kind == CsTomlTypeKind.Primitive)
        {
            var findName = SymbolUtility.GetFindKey(accessName);
            var propertyName = SymbolUtility.GetPropertyAccessName(accessName, ".");
            var valueName = SymbolUtility.GetPropertyAccessName(accessName, "_");

            switch (property.Type.SpecialType)
            {
                case SpecialType.System_Boolean:
                    deserializerBuilder.AppendLine($"        if (package.TryFind({findName}, out var _{valueName}))");
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
                    deserializerBuilder.AppendLine($"        if (package.TryFind({findName}, out var _{valueName}))");
                    deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetInt64();");
                    break;
                case SpecialType.System_String:
                    deserializerBuilder.AppendLine($"        if (package.TryFind({findName}, out var _{valueName}))");
                    deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetString();");
                    break;
                case SpecialType.System_Double:
                    deserializerBuilder.AppendLine($"        if (package.TryFind({findName}, out var _{valueName}))");
                    deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetDouble();");
                    break;
                case SpecialType.System_DateTime:
                    deserializerBuilder.AppendLine($"        if (package.TryFind({findName}, out var _{valueName}))");
                    deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetDateTime();");
                    break;
                default:
                    if (property.Type.TypeKind == TypeKind.Struct)
                    {
                        switch (property.Type.Name)
                        {
                            case "DateTimeOffset":
                                deserializerBuilder.AppendLine($"        if (package.TryFind({findName}, out var _{valueName}))");
                                deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetDateTimeOffset();");
                                break;
                            case "DateOnly":
                                deserializerBuilder.AppendLine($"        if (package.TryFind({findName}, out var _{valueName}))");
                                deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetDateOnly();");
                                break;
                            case "TimeOnly":
                                deserializerBuilder.AppendLine($"        if (package.TryFind({findName}, out var _{valueName}))");
                                deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetTimeOnly();");
                                break;
                        }
                    }
                    break;
            }
        }
        else if (kind == CsTomlTypeKind.Collection)
        {
            var findName = SymbolUtility.GetFindKey(accessName);
            var propertyName = SymbolUtility.GetPropertyAccessName(accessName, ".");
            var valueName = SymbolUtility.GetPropertyAccessName(accessName, "_");

            deserializerBuilder.AppendLine($"        if (package.TryFind({findName}, out var _{valueName}))");
            deserializerBuilder.AppendLine($"            target.{propertyName} = _{valueName}!.GetObject();");
        }
        else if (kind == CsTomlTypeKind.Unknown)
        {
            var findName = SymbolUtility.GetFindKey(accessName);
            var propertyName = SymbolUtility.GetPropertyAccessName(accessName, ".");
            var valueName = SymbolUtility.GetPropertyAccessName(accessName, "_");

            deserializerBuilder.AppendLine($"        if (package.TryFind({findName}, out var _{valueName}))");
            deserializerBuilder.AppendLine($"            target.{propertyName} = _{valueName}!.GetObject();");
        }
        else
        {
            context.ReportDiagnostic(Diagnostic.Create( DiagnosticDescriptors.KeyValueError, property.Locations[0], typeSymbol.Name, property.Name));
        }
    }

    private void GenerateDeserializeArray(SourceProductionContext context, List<string> accessNames, INamedTypeSymbol typeSymbol, IPropertySymbol property, CsTomlValueType type, CsTomlTypeKind kind)
    {
        if (kind != CsTomlTypeKind.Collection)
        {
            return;
        }

        var findName = SymbolUtility.GetFindKey(accessNames);
        var propertyName = SymbolUtility.GetPropertyAccessName(accessNames, ".");
        var valueName = SymbolUtility.GetPropertyAccessName(accessNames, "_");

        ITypeSymbol elementType = null;
        if (property.Type is IArrayTypeSymbol arrayProperty) // array 
        {
            elementType = arrayProperty.ElementType;
        }
        else if (property.Type is INamedTypeSymbol namedTypeSymbol) // collection interface
        {
            elementType = namedTypeSymbol.TypeArguments[0];
        }

        switch (elementType?.SpecialType ?? SpecialType.None)
        {
            case SpecialType.System_Boolean:
            case SpecialType.System_Byte:
            case SpecialType.System_SByte:
            case SpecialType.System_Int16:
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt16:
            case SpecialType.System_UInt32:
            case SpecialType.System_UInt64:
            case SpecialType.System_String:
            case SpecialType.System_Double:
            case SpecialType.System_DateTime:
                deserializerBuilder.AppendLine($"        if (package.TryFind({findName}, out var _{valueName}))");

                if (CollectionMetaData.FromArray(property.Type))
                {
                    deserializerBuilder.AppendLine($"            target.{propertyName} = _{valueName}!.GetValue<{elementType!.Name}[]>();");
                }
                else
                {
                    deserializerBuilder.AppendLine($"            target.{propertyName} = new (_{valueName}!.GetValue<{elementType!.Name}[]>());");
                }
                break;
            default:
                if (property.Type.TypeKind == TypeKind.Struct)
                {
                    switch (property.Type.Name)
                    {
                        case "DateTimeOffset":
                        case "DateOnly":
                        case "TimeOnly":
                            deserializerBuilder.AppendLine($"        if (package.TryFind({findName}, out var _{valueName}))");
                            if (CollectionMetaData.FromArray(property.Type))
                            {
                                deserializerBuilder.AppendLine($"            target.{propertyName} = _{valueName}!.GetValue<{elementType!.Name}[]>();");
                            }
                            else
                            {
                                deserializerBuilder.AppendLine($"            target.{propertyName} = new (_{valueName}!.GetValue<{elementType!.Name}[]>());");
                            }
                            break;
                    }
                }
                break;
        }
    }

    private void GenerateDeserializeForTable(SourceProductionContext context, List<string> accessNames, INamedTypeSymbol typeSymbol, (IPropertySymbol, CsTomlValueType)[] properties)
    {
        foreach (var (property, type) in properties)
        {
            var kind = SymbolUtility.GetCsTomlTypeKind(property.Type);
            if (kind == CsTomlTypeKind.Error)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.TableError, property.Locations[0], typeSymbol.Name, property.Name));
                continue;
            }

            switch(type)
            {
                case CsTomlValueType.KeyValue:
                    accessNames.Add(property.Name);
                    GenerateDeserializeForKeyValue(context, accessNames, (INamedTypeSymbol)property.Type, property, CsTomlValueType.None, kind);
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
                case CsTomlValueType.Array:
                    accessNames.Add(property.Name);
                    GenerateDeserializeArray(context, accessNames, typeSymbol, property, type, kind);
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
                case CsTomlValueType.Table:
                    // To prevent infinite loops
                    if (property.Type.Name == typeSymbol.Name)
                    {
                        var declaringSyntax = property.DeclaringSyntaxReferences[0].GetSyntax();
                        var syntaxTrees = declaringSyntax.ChildNodes().Where(n => n is IdentifierNameSyntax).FirstOrDefault();
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ThrowNotBeDefinedBySameClass,syntaxTrees?.GetLocation() ?? property.Locations[0], property.Type.Name));
                        continue;
                    }
                    accessNames.Add(property.Name);
                    GenerateDeserializeForTable(context, accessNames,
                    (INamedTypeSymbol)property.Type,
                    SymbolUtility.FilterCsTomlPackageValueMembers(SymbolUtility.GetProperties(property.Type), "CsTomlValueOnSerializedAttribute"));
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
            }
        }
    }

    private void GenerateDeserializeForInlineTable(SourceProductionContext context, List<string> accessNames, INamedTypeSymbol typeSymbol, (IPropertySymbol, CsTomlValueType)[] properties)
    {
        foreach (var (property, type) in properties)
        {
            var kind = SymbolUtility.GetCsTomlTypeKind(property.Type);
            if (kind == CsTomlTypeKind.Error)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InlineTableError, property.Locations[0], typeSymbol.Name, property.Name));
                continue;
            }
            switch (type)
            {
                case CsTomlValueType.KeyValue:
                    accessNames.Add(property.Name);
                    GenerateDeserializeForKeyValue(context, accessNames, (INamedTypeSymbol)property.Type, property, CsTomlValueType.None, kind);
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
                case CsTomlValueType.Array:
                    accessNames.Add(property.Name);
                    GenerateDeserializeArray(context, accessNames, typeSymbol, property, type, kind);
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
                case CsTomlValueType.InlineTable:
                    // To prevent infinite loops
                    if (property.Type.Name == typeSymbol.Name)
                    {
                        var declaringSyntax = property.DeclaringSyntaxReferences[0].GetSyntax();
                        var syntaxTrees = declaringSyntax.ChildNodes().Where(n => n is IdentifierNameSyntax).FirstOrDefault();
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ThrowNotBeDefinedBySameClass, syntaxTrees?.GetLocation() ?? property.Locations[0], property.Type.Name));
                        continue;
                    }
                    accessNames.Add(property.Name);
                    GenerateDeserializeForInlineTable(context, accessNames,
                    (INamedTypeSymbol)property.Type,
                    SymbolUtility.FilterCsTomlPackageValueMembers(SymbolUtility.GetProperties(property.Type), "CsTomlValueOnSerializedAttribute"));
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
            }
        }
    }
}


