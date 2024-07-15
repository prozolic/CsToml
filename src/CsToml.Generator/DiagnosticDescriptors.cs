using Microsoft.CodeAnalysis;

namespace CsToml.Generator;

internal static class DiagnosticDescriptors
{
    const string Category = "CsTomlError";

    public static readonly DiagnosticDescriptor KeyValueError = new(
        id: "CsTomlValueSerializerError_K1",
        title: "type", 
        messageFormat: "'{0}.{1}' uses a type that cannot be specified with CsTomlValueType.KeyValue.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor Table = new(
        id: "CsTomlValueSerializerError_T1",
        title: "type",
        messageFormat: "'{0}.{1}' uses a type that cannot be specified with CsTomlValueType.Table.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ArrayError = new(
        id: "CsTomlValueSerializerError_A1",
        title: "type",
        messageFormat: "'{0}.{1}' uses a type that is not a collection type with CsTomlValueType.Array.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ArrayOfTablesError = new(
        id: "CsTomlValueSerializerError_A2",
        title: "type",
        messageFormat: "'{0}.{1}' uses a type that cannot be specified with CsTomlValueType.ArrayOfTables.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ArrayOfTablesError2 = new(
        id: "CsTomlValueSerializerError_A3",
        title: "type",
        messageFormat: "If CsTomlValueType.ArrayOfTables is specified, the CsTomlArrayOfTablesKeyAttribute must also be added.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InlineTableError = new(
        id: "CsTomlValueSerializerError_I1",
        title: "type",
        messageFormat: "'{0}.{1}' uses a type that cannot be specified with CsTomlValueType.InlineTable",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ThrowNotBeDefinedBySameClass = new(
        id: "CsTomlValueSerializerError_TH1",
        title: "type",
        messageFormat: "'{0}.{1}' uses a type that cannot be specified with CsTomlValueType.InlineTable. '{0}' cannot be used for '{0}' properties.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

}


