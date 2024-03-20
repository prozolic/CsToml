using Microsoft.CodeAnalysis;

namespace CsToml.Generator;

internal static class DiagnosticDescriptors
{
    const string Category = "CsTomlError";

    public static readonly DiagnosticDescriptor KeyValueError = new(
        id: "CsTomlError1",
        title: "type", 
        messageFormat: "'{0}.{1}' uses a type that cannot be specified with CsTomlValueType.KeyValue.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ArrayError = new(
        id: "CsTomlError2",
        title: "type",
        messageFormat: "'{0}.{1}' uses a type that is not a collection type with CsTomlValueType.Array.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ArrayOfTablesError = new(
        id: "CsTomlError3",
        title: "type",
        messageFormat: "'{0}.{1}' uses a type that cannot be specified with CsTomlValueType.ArrayOfTables.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}


