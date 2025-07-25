#pragma warning disable RS2008

using Microsoft.CodeAnalysis;

namespace CsToml.Generator;

internal static class DiagnosticDescriptors
{
    const string Category = "CsTomlError";

    public static readonly DiagnosticDescriptor TypeMustBePartial = new(
        id: "CsTomlError001",
        title: "Serializable type declarations in CsToml must be 'partial'",
        messageFormat: "Serializable type declarations in CsToml must be 'partial': {0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TypeCannotBeAbstract = new (
        id: "CsTomlError002",
        title: "CsToml serializable type must not be 'abstract' type",
        messageFormat: "CsToml serializable type must not be 'abstract' type: {0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TypeCannotBeNested = new(
        id: "CsTomlError003",
        title: "CsToml serializable type must not be nested type",
        messageFormat: "CsToml serializable type must not be nested type: {0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidSerializationType = new(
        id: "CsTomlError004",
        title: "CsToml serializable type must not be error type",
        messageFormat: "CsToml serializable type must not be error type: {0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PropertyMustHaveSetter = new(
        id: "CsTomlError005",
        title: "CsToml serializable property must be setter",
        messageFormat: "CsToml serializable property must be setter: {0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicatePropertyKey = new(
        id: "CsTomlError006",
        title: "Defining the same key multiple times for properties and aliases is invalid",
        messageFormat: "Defining the same key multiple times for properties and aliases is invalid: {0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateAliasKey = new(
        id: "CsTomlError007",
        title: "Defining the same key multiple times for properties and aliases is invalid",
        messageFormat: "Defining the same key multiple times for properties and aliases is invalid: TomlValueOnSerialized(aliasName:\"{0}\")",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor SetterMustBePublicOrInit = new(
        id: "CsTomlError008",
        title: "CsToml serializable setter's property must be public or init accessor",
        messageFormat: "CsToml serializable setter's property must be public in scope: {0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NoBindableConstructor = new(
        id: "CsTomlError009",
        title: "There is no constructor that can bind",
        messageFormat: "There is no constructor that can bind: {0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AliasNameCannotContainNewlines = new(
        id: "CsTomlError010",
        title: "Key cannot contain newline characters",
        messageFormat: "The alias name '{0}' contains newline characters",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}


