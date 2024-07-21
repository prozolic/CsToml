using Microsoft.CodeAnalysis;

namespace CsToml.Generator;

internal static class DiagnosticDescriptors
{
    const string Category = "CsTomlError";

    public static readonly DiagnosticDescriptor KeyValueError = new(
        id: "CsTomlError_K1",
        title: "type", 
        messageFormat: "'{0}.{1}' uses a type that cannot be specified with CsTomlValueType.KeyValue.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TableError = new(
        id: "CsTomlError_T1",
        title: "type",
        messageFormat: "'{0}.{1}' uses a type that cannot be specified with CsTomlValueType.Table.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ArrayError = new(
        id: "CsTomlError_A1",
        title: "type",
        messageFormat: "'{0}.{1}' uses a type that is not a collection type with CsTomlValueType.Array.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ArrayOfTablesError = new(
        id: "CsTomlError_AT1",
        title: "type",
        messageFormat: "'{0}.{1}' uses a type that cannot be specified with CsTomlValueType.ArrayOfTables.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ArrayOfTablesError2 = new(
        id: "CsTomlError_AT2",
        title: "type",
        messageFormat: "If CsTomlValueType.ArrayOfTables is specified, the CsTomlArrayOfTablesKeyAttribute must also be added.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ArrayOfTablesError3 = new(
        id: "CsTomlError_AT3",
        title: "type",
        messageFormat: "The specified table array has already been specified.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InlineTableError = new(
        id: "CsTomlError_I1",
        title: "type",
        messageFormat: "'{0}.{1}' uses a type that cannot be specified with CsTomlValueType.InlineTable",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InlineTableError2 = new(
        id: "CsTomlError_I2",
        title: "type",
        messageFormat: "CsTomlValueType.Table cannot be specified as a property ('{0}.{1}') of a class that specifies CsTomlValueType.InlineTable.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ThrowNotBeDefinedBySameClass = new(
        id: "CsTomlError_TH1",
        title: "type",
        messageFormat: "'{0}' cannot be defined as a property within '{0}'.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

}


