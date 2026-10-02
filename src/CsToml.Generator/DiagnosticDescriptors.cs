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

    public static readonly DiagnosticDescriptor UnionRequiresTomlUnionAttribute = new(
        id: "CsTomlError011",
        title: "Union types used by CsToml must be annotated with TomlUnionAttribute<T>",
        messageFormat: "The union type '{1}' reachable from property '{0}' requires TomlUnionAttribute<T>",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnionPinnedTypeIsNotCase = new(
        id: "CsTomlError012",
        title: "The type argument of TomlUnionAttribute<T> must be a case type of the union",
        messageFormat: "The type '{1}' is not a case type of union '{0}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TomlUnionAttributeOnNonUnion = new(
        id: "CsTomlError013",
        title: "TomlUnionAttribute<T> must be applied to a union type",
        messageFormat: "TomlUnionAttribute<T> is applied to '{0}', which is not a union type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnionCannotBeTomlSerializedObject = new(
        id: "CsTomlError014",
        title: "TomlSerializedObjectAttribute must not be applied to a union type",
        messageFormat: "TomlSerializedObjectAttribute must not be applied to the union type '{0}'. Use TomlUnionAttribute<T> instead.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GenericUnionCannotBePinnedAtTypeLevel = new(
        id: "CsTomlError015",
        title: "TomlUnionAttribute<T> must not be applied to a union type declaration that is generic or nested in a generic type",
        messageFormat: "TomlUnionAttribute<T> must not be applied to the union type '{0}' because it is generic or nested in a generic type. Apply it to a property whose type is the closed union instead.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TomlUnionAttributeOnWrappedUnionMember = new(
        id: "CsTomlError017",
        title: "Member-level TomlUnionAttribute<T> only applies to a property whose type is the union itself",
        messageFormat: "TomlUnionAttribute<T> on property '{0}' cannot pin the union '{1}' wrapped in '{2}'. Apply TomlUnionAttribute<T> to the union type instead.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnionPinnedCaseCannotBeSerialized = new(
        id: "CsTomlError016",
        title: "The pinned union case type cannot be serialized by CsToml",
        messageFormat: "The pinned case type '{1}' of union '{0}' cannot be serialized by CsToml",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnionFormatterNameCollision = new(
        id: "CsTomlError018",
        title: "Generated union formatter names collide",
        messageFormat: "The union formatters generated for '{0}' and '{1}' would both be named '{2}'. Rename one of the union types or its containing type.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnionTypeParameterNameShadowed = new(
        id: "CsTomlError019",
        title: "Union type parameter must not reuse the name of a containing type's type parameter",
        messageFormat: "The union '{0}' reuses a type parameter name of its containing type, so a formatter cannot be generated for it. Rename the type parameter.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}


