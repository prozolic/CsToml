using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Text;

namespace CsToml.Generator;

internal sealed class CsTomlSerializationGenerator
{
    private INamedTypeSymbol typeSymbol;
    private TypeDeclarationSyntax typeNode;
    private (IPropertySymbol, TomlValueType)[] valueMembers;
    private StringBuilder serializerBuilder;
    private StringBuilder deserializerBuilder;

    public CsTomlSerializationGenerator(INamedTypeSymbol typeSymbol, TypeDeclarationSyntax typeNode)
    {
        this.typeSymbol = typeSymbol;
        this.typeNode = typeNode;

        var allmembers = SymbolUtility.GetProperties(typeSymbol);
        valueMembers = SymbolUtility.FilterTomlDocumentValueMembers(allmembers, "TomlValueOnSerializedAttribute").ToArray();
        serializerBuilder = new StringBuilder();
        deserializerBuilder = new StringBuilder();
    }

    public string GenerateSerializeProcessCode(SourceProductionContext context)
    {
        serializerBuilder.Clear();
        GenerateSerializerCore(context, typeSymbol, valueMembers);
        return serializerBuilder.ToString();
    }

    private void GenerateSerializerCore(SourceProductionContext context, INamedTypeSymbol typeSymbol, (IPropertySymbol, TomlValueType)[] properties)
    {
        var accessNames = new List<string>();
        foreach (var (property, type) in properties)
        {
            var kind = SymbolUtility.GetCsTomlTypeKind(property.Type);
            if (kind == TomlTypeKind.Error) 
            {
                continue;
            }
            accessNames.Clear();
            accessNames.Add(property.Name);

            switch (type)
            {
                case TomlValueType.KeyValue:
                case TomlValueType.Array:
                    GenerateSerializeForKeyValue(context, accessNames, typeSymbol, property, type, kind);
                    break;
                case TomlValueType.Table:
                    switch (kind)
                    {
                        case TomlTypeKind.Primitive:
                        case TomlTypeKind.Collection:
                        case TomlTypeKind.ArrayOfTables:
                            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.TableError, property.Locations[0], typeSymbol.Name, property.Name));
                            continue;
                    }

                    GenerateSerializeForTable(
                        context, accessNames,
                        (INamedTypeSymbol)property.Type,
                        SymbolUtility.FilterTomlDocumentValueMembers(SymbolUtility.GetProperties(property.Type), "TomlValueOnSerializedAttribute"));
                    break;
                case TomlValueType.InlineTable:
                    switch (kind)
                    {
                        case TomlTypeKind.Primitive:
                        case TomlTypeKind.Collection:
                        case TomlTypeKind.ArrayOfTables:
                            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InlineTableError, property.Locations[0], typeSymbol.Name, property.Name));
                            continue;
                    }
                    GenerateSerializeForInlineTable(
                        context, accessNames, (INamedTypeSymbol)property.Type, property,
                        SymbolUtility.FilterTomlDocumentValueMembers(SymbolUtility.GetProperties(property.Type), "TomlValueOnSerializedAttribute")
                        , true);
                    break;
            }

        }

        return;
    }

    private void GenerateSerializeForKeyValue(SourceProductionContext context, List<string> accessName, INamedTypeSymbol typeSymbol, IPropertySymbol property, TomlValueType type, TomlTypeKind kind)
    {
        var propertyName = SymbolUtility.GetPropertyAccessName(accessName, ".");

        if (kind == TomlTypeKind.Primitive)
        {
            serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeKey(ref writer, \"{property.Name}\");");
            serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeEqual(ref writer);");
            if (property.Type.SpecialType == SpecialType.System_String)
            {
                serializerBuilder.AppendLine($@"        if (string.IsNullOrEmpty(target.{propertyName}))");
                serializerBuilder.AppendLine($@"            ITomlValueSerializer.Serialize(ref writer, Span<char>.Empty);");
                serializerBuilder.AppendLine($@"        else");
                serializerBuilder.AppendLine($@"            ITomlValueSerializer.Serialize(ref writer, target.{propertyName}.AsSpan());");
            }
            else
            {
                serializerBuilder.AppendLine($"        ITomlValueSerializer.Serialize(ref writer, target.{propertyName});");
            }
            serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeNewLine(ref writer);");
        }
        else if (kind == TomlTypeKind.Collection)
        {
            serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeKey(ref writer, \"{property.Name}\");");
            serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeEqual(ref writer);");
            serializerBuilder.AppendLine($"        ITomlValueSerializer.Serialize(ref writer, target.{propertyName});");
            serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeNewLine(ref writer);");
        }
        else if (kind == TomlTypeKind.Unknown)
        {
            serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeKey(ref writer, \"{property.Name}\");");
            serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeEqual(ref writer);");
            serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeDynamic(ref writer, target.{propertyName});");
            serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeNewLine(ref writer);");
        }
        else
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.KeyValueError, property.Locations[0], typeSymbol.Name, property.Name));
        }
    }

    private void GenerateSerializeForTable(SourceProductionContext context, List<string> accessNames, INamedTypeSymbol typeSymbol, (IPropertySymbol, TomlValueType)[] properties)
    {
        var findName = SymbolUtility.GetFindKey(accessNames);
        var propertyName = SymbolUtility.GetPropertyAccessName(accessNames, ".");

        serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeNewLine(ref writer);");
        serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeTableHeader(ref writer, \"{propertyName}\");");

        foreach (var (property, type) in properties)
        {
            var kind = SymbolUtility.GetCsTomlTypeKind(property.Type);
            if (kind == TomlTypeKind.Error)
            {
                context.ReportDiagnostic(Diagnostic.Create( DiagnosticDescriptors.TableError, property.Locations[0], typeSymbol.Name, property.Name));
                continue;
            }

            switch (type)
            {
                case TomlValueType.KeyValue:
                case TomlValueType.Array:
                    accessNames.Add(property.Name);
                    GenerateSerializeForKeyValue(context, accessNames, typeSymbol, property, type, kind);
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
                case TomlValueType.Table:
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
                        SymbolUtility.FilterTomlDocumentValueMembers(SymbolUtility.GetProperties(property.Type), "TomlValueOnSerializedAttribute"));
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
                case TomlValueType.InlineTable:
                    switch (kind)
                    {
                        case TomlTypeKind.Primitive:
                        case TomlTypeKind.Collection:
                        case TomlTypeKind.ArrayOfTables:
                            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InlineTableError, property.Locations[0], typeSymbol.Name, property.Name));
                            continue;
                    }
                    accessNames.Add(property.Name);
                    GenerateSerializeForInlineTable(
                        context, accessNames,
                        (INamedTypeSymbol)property.Type,
                        property,
                        SymbolUtility.FilterTomlDocumentValueMembers(SymbolUtility.GetProperties(property.Type), "TomlValueOnSerializedAttribute")
                        , true);
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
            }
        }
    }

    private void GenerateSerializeForInlineTable(SourceProductionContext context, List<string> accessNames, INamedTypeSymbol typeSymbol, IPropertySymbol property, (IPropertySymbol, TomlValueType)[] propertiesArray, bool newline)
    {
        serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeKey(ref writer, \"{property.Name}\");");
        serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeEqual(ref writer);");
        serializerBuilder.AppendLine("        ITomlValueSerializer.SerializeAnyByte(ref writer, \"{\"u8);");

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
            if (kind == TomlTypeKind.Error)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.KeyValueError, inlineProperty.Locations[0], typeSymbol.Name, inlineProperty.Name));
                continue;
            }

            accessNames.Add(inlineProperty.Name);
            var inlineTablePropertyName = SymbolUtility.GetPropertyAccessName(accessNames, ".");
            accessNames.RemoveAt(accessNames.Count - 1);

            switch (type)
            {
                case TomlValueType.KeyValue:
                    if (kind == TomlTypeKind.Primitive)
                    {
                        serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeKey(ref writer, \"{inlineProperty.Name}\");");
                        serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeEqual(ref writer);");
                        if (inlineProperty.Type.SpecialType == SpecialType.System_String)
                        {
                            serializerBuilder.AppendLine($@"        if (string.IsNullOrEmpty(target.{inlineTablePropertyName}))");
                            serializerBuilder.AppendLine($@"            ITomlValueSerializer.Serialize(ref writer, Span<char>.Empty);");
                            serializerBuilder.AppendLine($@"        else");
                            serializerBuilder.AppendLine($@"            ITomlValueSerializer.Serialize(ref writer, target.{inlineTablePropertyName}.AsSpan());");
                        }
                        else
                        {
                            serializerBuilder.AppendLine($"        ITomlValueSerializer.Serialize(ref writer, target.{inlineTablePropertyName});");
                        }
                    }
                    else if (kind  == TomlTypeKind.Unknown)
                    {
                        serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeKey(ref writer, \"{inlineProperty.Name}\");");
                        serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeEqual(ref writer);");
                        serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeDynamic(ref writer, target.{inlineTablePropertyName});");
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.KeyValueError, inlineProperty.Locations[0], typeSymbol.Name, inlineProperty.Name));
                    }

                    break;
                case TomlValueType.Array:
                    if (kind != TomlTypeKind.Collection)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ArrayError,inlineProperty.Locations[0],typeSymbol.Name, inlineProperty.Name));
                        continue;
                    }
                    serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeKey(ref writer, \"{inlineProperty.Name}\");");
                    serializerBuilder.AppendLine($"        ITomlValueSerializer.SerializeEqual(ref writer);");
                    serializerBuilder.AppendLine($"        ITomlValueSerializer.Serialize(ref writer, target.{inlineTablePropertyName});");
                    break;
                case TomlValueType.InlineTable:
                    switch (kind)
                    {
                        case TomlTypeKind.Primitive:
                        case TomlTypeKind.Collection:
                        case TomlTypeKind.ArrayOfTables:
                            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InlineTableError, inlineProperty.Locations[0], typeSymbol.Name, inlineProperty.Name));
                            continue;
                    }
                    accessNames.Add(inlineProperty.Name);
                    GenerateSerializeForInlineTable(
                        context, accessNames,
                        (INamedTypeSymbol)inlineProperty.Type,
                        inlineProperty,
                        SymbolUtility.FilterTomlDocumentValueMembers(SymbolUtility.GetProperties(inlineProperty.Type), "TomlValueOnSerializedAttribute"),
                        false);
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
                case TomlValueType.Table:
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InlineTableError2, property.Locations[0], property.Type.Name,inlineProperty.Name));
                    continue;
            }

            if (i < propertiesArray.Length - 1)
            {
                serializerBuilder.AppendLine("        ITomlValueSerializer.SerializeAnyByte(ref writer, \", \"u8);");
            }
        }

        serializerBuilder.AppendLine("        ITomlValueSerializer.SerializeAnyByte(ref writer, \"}\"u8);");
        if (newline)
            serializerBuilder.AppendLine("        ITomlValueSerializer.SerializeNewLine(ref writer);");
    }

    public string GenerateDeserializeProcessCode(SourceProductionContext context)
    {
        GenerateDeserializeCore(context, string.Empty, typeSymbol, valueMembers);

        return deserializerBuilder.ToString();
    }

    private void GenerateDeserializeCore(SourceProductionContext context, string accessName, INamedTypeSymbol typeSymbol, (IPropertySymbol, TomlValueType)[] properties)
    {
        deserializerBuilder.AppendLine($"        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText, options);");
        deserializerBuilder.AppendLine($"        var target = new {this.typeSymbol}();");

        var arrayOftableProperties = new List<(IPropertySymbol, TomlValueType, AttributeData)>();
        foreach (var (property, type) in properties)
        {
            var kind = SymbolUtility.GetCsTomlTypeKind(property.Type);
            if (kind == TomlTypeKind.Error)
            {
                continue;
            }

            var accessNames = new List<string> { property.Name };
            switch(type)
            {
                case TomlValueType.KeyValue:
                    GenerateDeserializeForKeyValue(context, accessNames, typeSymbol, property, type, kind);
                    break;
                case TomlValueType.Array:
                    GenerateDeserializeArray(context, accessNames, typeSymbol, property, type, kind);
                    break;
                case TomlValueType.Table:
                    GenerateDeserializeForTable(context, accessNames,
                    (INamedTypeSymbol)property.Type,
                    SymbolUtility.FilterTomlDocumentValueMembers(SymbolUtility.GetProperties(property.Type), "TomlValueOnSerializedAttribute"));
                    break;
                case TomlValueType.InlineTable:
                    GenerateDeserializeForInlineTable(context, accessNames,
                    (INamedTypeSymbol)property.Type,
                    SymbolUtility.FilterTomlDocumentValueMembers(SymbolUtility.GetProperties(property.Type), "TomlValueOnSerializedAttribute"));
                    break;
            }


        }

        if (arrayOftableProperties.Count > 0)
        {
            //GenerateDeserializeForArrayOfTables(arrayOftableProperties, context);
        }
        deserializerBuilder.AppendLine($"        return target;");
    }

    private void GenerateDeserializeForKeyValue(SourceProductionContext context, List<string> accessName, INamedTypeSymbol typeSymbol, IPropertySymbol property, TomlValueType type, TomlTypeKind kind)
    {
        if (kind == TomlTypeKind.Primitive)
        {
            var findName = SymbolUtility.GetFindKey(accessName);
            var propertyName = SymbolUtility.GetPropertyAccessName(accessName, ".");
            var valueName = SymbolUtility.GetPropertyAccessName(accessName, "_");

            switch (property.Type.SpecialType)
            {
                case SpecialType.System_Boolean:
                    deserializerBuilder.AppendLine($"        if (document.TryFind({findName}, out var _{valueName}))");
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
                    deserializerBuilder.AppendLine($"        if (document.TryFind({findName}, out var _{valueName}))");
                    deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetInt64();");
                    break;
                case SpecialType.System_String:
                    deserializerBuilder.AppendLine($"        if (document.TryFind({findName}, out var _{valueName}))");
                    deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetString();");
                    break;
                case SpecialType.System_Double:
                    deserializerBuilder.AppendLine($"        if (document.TryFind({findName}, out var _{valueName}))");
                    deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetDouble();");
                    break;
                case SpecialType.System_DateTime:
                    deserializerBuilder.AppendLine($"        if (document.TryFind({findName}, out var _{valueName}))");
                    deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetDateTime();");
                    break;
                default:
                    if (property.Type.TypeKind == TypeKind.Struct)
                    {
                        switch (property.Type.Name)
                        {
                            case "DateTimeOffset":
                                deserializerBuilder.AppendLine($"        if (document.TryFind({findName}, out var _{valueName}))");
                                deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetDateTimeOffset();");
                                break;
                            case "DateOnly":
                                deserializerBuilder.AppendLine($"        if (document.TryFind({findName}, out var _{valueName}))");
                                deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetDateOnly();");
                                break;
                            case "TimeOnly":
                                deserializerBuilder.AppendLine($"        if (document.TryFind({findName}, out var _{valueName}))");
                                deserializerBuilder.AppendLine($"            target.{propertyName} = ({property.Type.Name})_{valueName}!.GetTimeOnly();");
                                break;
                        }
                    }
                    break;
            }
        }
        else if (kind == TomlTypeKind.Collection)
        {
            var findName = SymbolUtility.GetFindKey(accessName);
            var propertyName = SymbolUtility.GetPropertyAccessName(accessName, ".");
            var valueName = SymbolUtility.GetPropertyAccessName(accessName, "_");

            deserializerBuilder.AppendLine($"        if (document.TryFind({findName}, out var _{valueName}))");
            deserializerBuilder.AppendLine($"            target.{propertyName} = _{valueName}!.GetObject();");
        }
        else if (kind == TomlTypeKind.Unknown)
        {
            var findName = SymbolUtility.GetFindKey(accessName);
            var propertyName = SymbolUtility.GetPropertyAccessName(accessName, ".");
            var valueName = SymbolUtility.GetPropertyAccessName(accessName, "_");

            deserializerBuilder.AppendLine($"        if (document.TryFind({findName}, out var _{valueName}))");
            deserializerBuilder.AppendLine($"            target.{propertyName} = _{valueName}!.GetObject();");
        }
        else
        {
            context.ReportDiagnostic(Diagnostic.Create( DiagnosticDescriptors.KeyValueError, property.Locations[0], typeSymbol.Name, property.Name));
        }
    }

    private void GenerateDeserializeArray(SourceProductionContext context, List<string> accessNames, INamedTypeSymbol typeSymbol, IPropertySymbol property, TomlValueType type, TomlTypeKind kind)
    {
        if (kind != TomlTypeKind.Collection)
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
                deserializerBuilder.AppendLine($"        if (document.TryFind({findName}, out var _{valueName}))");

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
                            deserializerBuilder.AppendLine($"        if (document.TryFind({findName}, out var _{valueName}))");
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

    private void GenerateDeserializeForTable(SourceProductionContext context, List<string> accessNames, INamedTypeSymbol typeSymbol, (IPropertySymbol, TomlValueType)[] properties)
    {
        foreach (var (property, type) in properties)
        {
            var kind = SymbolUtility.GetCsTomlTypeKind(property.Type);
            if (kind == TomlTypeKind.Error)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.TableError, property.Locations[0], typeSymbol.Name, property.Name));
                continue;
            }

            switch(type)
            {
                case TomlValueType.KeyValue:
                    accessNames.Add(property.Name);
                    GenerateDeserializeForKeyValue(context, accessNames, (INamedTypeSymbol)property.Type, property, TomlValueType.None, kind);
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
                case TomlValueType.Array:
                    accessNames.Add(property.Name);
                    GenerateDeserializeArray(context, accessNames, typeSymbol, property, type, kind);
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
                case TomlValueType.Table:
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
                    SymbolUtility.FilterTomlDocumentValueMembers(SymbolUtility.GetProperties(property.Type), "TomlValueOnSerializedAttribute"));
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
            }
        }
    }

    private void GenerateDeserializeForInlineTable(SourceProductionContext context, List<string> accessNames, INamedTypeSymbol typeSymbol, (IPropertySymbol, TomlValueType)[] properties)
    {
        foreach (var (property, type) in properties)
        {
            var kind = SymbolUtility.GetCsTomlTypeKind(property.Type);
            if (kind == TomlTypeKind.Error)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InlineTableError, property.Locations[0], typeSymbol.Name, property.Name));
                continue;
            }
            switch (type)
            {
                case TomlValueType.KeyValue:
                    accessNames.Add(property.Name);
                    GenerateDeserializeForKeyValue(context, accessNames, (INamedTypeSymbol)property.Type, property, TomlValueType.None, kind);
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
                case TomlValueType.Array:
                    accessNames.Add(property.Name);
                    GenerateDeserializeArray(context, accessNames, typeSymbol, property, type, kind);
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
                case TomlValueType.InlineTable:
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
                    SymbolUtility.FilterTomlDocumentValueMembers(SymbolUtility.GetProperties(property.Type), "TomlValueOnSerializedAttribute"));
                    accessNames.RemoveAt(accessNames.Count - 1);
                    break;
            }
        }
    }
}


