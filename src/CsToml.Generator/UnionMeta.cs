using Microsoft.CodeAnalysis;

namespace CsToml.Generator;

internal enum UnionConstructionKind
{
    Constructor,
    IUnionMembersCreate,
}

internal enum UnionValueAccess
{
    OnlyTryGetValue,
    ValueProperty,
    IUnionMembersValueProperty,
}

internal sealed record UnionMeta
{
    public string FullTypeName { get; init; } = "";

    public string DisplayName { get; init; } = "";

    public string FormatterNamespace { get; init; } = "";

    public string FormatterClassName { get; init; } = "";

    public string TypeParameterList { get; init; } = "";

    public string TypeParameterConstraints { get; init; } = "";

    public bool IsValueType { get; init; }

    public bool HasHasValue { get; init; }

    public UnionValueAccess ValueAccess { get; init; }

    public UnionConstructionKind ConstructionKind { get; init; }

    public bool IsTypeLevel { get; init; }

    public UnionCaseMeta Case { get; init; } = new();

    public string DependencyRegistrationCode { get; init; } = "";
}

internal sealed record UnionCaseMeta
{
    // Type used in `is` patterns, GetFormatter<> calls and deserialization
    // (the underlying type for Nullable<T> cases; otherwise the declared case type).
    public string PatternTypeName { get; init; } = "";

    public bool HasTryGetValue { get; init; }

    // Declared out-parameter type of the matched TryGetValue ("" when HasTryGetValue is false).
    public string TryGetValueOutTypeName { get; init; } = "";
}
