
using Microsoft.CodeAnalysis;

namespace CsToml.Generator;

internal enum GenericFormatterType
{
    None,
    Array,
    Collection,
    Dictionary,
    Other,
}

internal static class FormatterTypeMetaData
{
    private static readonly Dictionary<string, string> builtInCollectionFormatterTypes = new()
    {
        { "global::System.Collections.Generic.List<>", "ListFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.Stack<>", "StackFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.HashSet<>", "HashSetFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.SortedSet<>", "SortedSetFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.Queue<>", "QueueFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.LinkedList<>", "LinkedListFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Concurrent.ConcurrentBag<>", "ConcurrentBagFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Concurrent.ConcurrentQueue<>", "ConcurrentQueueFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Concurrent.ConcurrentStack<>", "ConcurrentStackFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Concurrent.BlockingCollection<>", "BlockingCollectionFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.ObjectModel.ReadOnlyCollection<>", "ReadOnlyCollectionFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.ObjectModel.ReadOnlySet<>", "ReadOnlySetFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Immutable.ImmutableArray<>", "ImmutableArrayFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Immutable.ImmutableList<>", "ImmutableListFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Immutable.ImmutableStack<>", "ImmutableStackFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Immutable.ImmutableQueue<>", "ImmutableQueueFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Immutable.ImmutableHashSet<>", "ImmutableHashSetFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Immutable.ImmutableSortedSet<>", "ImmutableSortedSetFormatter<TYPEPARAMETER>" },

        { "global::System.Collections.Frozen.FrozenSet<>", "FrozenSetFormatter<TYPEPARAMETER>" },
    };

    private static readonly Dictionary<string, string> builtInInterfaceCollectionFormatterTypes = new()
    {
        { "global::System.Collections.Generic.IEnumerable<>", "IEnumerableFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.ICollection<>", "ICollectionFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.IReadOnlyCollection<>", "IReadOnlyCollectionFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.IList<>", "IListFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.IReadOnlyList<>", "IReadOnlyListFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.ISet<>", "ISetFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.IReadOnlySet<>", "IReadOnlySetFormatter<TYPEPARAMETER>" },
        { "global::System.Linq.ILookup<,>", "ILookupFormatter<TYPEPARAMETER>" },
        { "global::System.Linq.IGrouping<,>", "IGroupingFormatter<TYPEPARAMETER>" },

        { "global::System.Collections.Immutable.IImmutableList<>", "IImmutableListFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Immutable.IImmutableStack<>", "IImmutableStackFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Immutable.IImmutableQueue<>", "IImmutableQueueFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Immutable.IImmutableSet<>", "IImmutableSetFormatter<TYPEPARAMETER>" },
    };

    private static readonly Dictionary<string, string> builtInDictionaryFormatterTypes = new()
    {
        { "global::System.Collections.Generic.Dictionary<,>", "DictionaryFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.SortedList<,>", "SortedListFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.PriorityQueue<,>", "PriorityQueueFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.SortedDictionary<,>", "SortedDictionaryFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.OrderedDictionary<,>", "OrderedDictionaryFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Concurrent.ConcurrentDictionary<,>", "ConcurrentDictionaryFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.ObjectModel.ReadOnlyDictionary<,>", "ReadOnlyDictionaryFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Immutable.ImmutableDictionary<,>", "ImmutableDictionaryFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Immutable.ImmutableSortedDictionary<,>", "ImmutableSortedDictionaryFormatter<TYPEPARAMETER>" },

        { "global::System.Collections.Generic.IDictionary<,>", "IDictionaryFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Generic.IReadOnlyDictionary<,>", "IReadOnlyDictionaryFormatter<TYPEPARAMETER>" },
        { "global::System.Collections.Immutable.IImmutableDictionary<,>", "IImmutableDictionaryFormatter<TYPEPARAMETER>" },

        { "global::System.Collections.Frozen.FrozenDictionary<,>", "FrozenDictionaryFormatter<TYPEPARAMETER>" },
    };

    private static readonly Dictionary<string, string> builtInGenericFormatterTypes = new()
    {
        { "global::System.Collections.Generic.KeyValuePair<,>", "KeyValuePairFormatter<TYPEPARAMETER>" },
        { "global::System.Tuple<>", "TupleFormatter<TYPEPARAMETER>" },
        { "global::System.Tuple<,>", "TupleFormatter<TYPEPARAMETER>" },
        { "global::System.Tuple<,,>", "TupleFormatter<TYPEPARAMETER>" },
        { "global::System.Tuple<,,,>", "TupleFormatter<TYPEPARAMETER>" },
        { "global::System.Tuple<,,,,>", "TupleFormatter<TYPEPARAMETER>" },
        { "global::System.Tuple<,,,,,>", "TupleFormatter<TYPEPARAMETER>" },
        { "global::System.Tuple<,,,,,,>", "TupleFormatter<TYPEPARAMETER>" },
        { "global::System.Tuple<,,,,,,,>", "TupleFormatter<TYPEPARAMETER>" },
        { "global::System.ValueTuple<>", "ValueTupleFormatter<TYPEPARAMETER>" },
        { "global::System.ValueTuple<,>", "ValueTupleFormatter<TYPEPARAMETER>" },
        { "global::System.ValueTuple<,,>", "ValueTupleFormatter<TYPEPARAMETER>" },
        { "global::System.ValueTuple<,,,>", "ValueTupleFormatter<TYPEPARAMETER>" },
        { "global::System.ValueTuple<,,,,>", "ValueTupleFormatter<TYPEPARAMETER>" },
        { "global::System.ValueTuple<,,,,,>", "ValueTupleFormatter<TYPEPARAMETER>" },
        { "global::System.ValueTuple<,,,,,,>", "ValueTupleFormatter<TYPEPARAMETER>" },
        { "global::System.ValueTuple<,,,,,,,>", "ValueTupleFormatter<TYPEPARAMETER>" },

        { "global::System.Nullable<,>", "NullableFormatter<TYPEPARAMETER>" },
        { "global::System.Memory<>", "MemoryFormatter<TYPEPARAMETER>" },
        { "global::System.ReadOnlyMemory<>", "ReadOnlyMemoryFormatter<TYPEPARAMETER>" },
    };

    private static readonly Dictionary<string, string> otherGenericFormatterTypes = new()
    {
        { "global::System.Lazy<>", "LazyFormatter<TYPEPARAMETER>" },
    };

    private static readonly HashSet<string> builtInFormatterTypeMap = new()
    {
        "bool",
        "byte",
        "sbyte",
        "char",
        "float",
        "double",
        "short",
        "ushort",
        "int",
        "uint",
        "long",
        "ulong",
        "decimal",
        "string",
        "global::System.Boolean",
        "global::System.Byte",
        "global::System.SByte",
        "global::System.Char",
        "global::System.Single",
        "global::System.Double",
        "global::System.Int16",
        "global::System.UInt16",
        "global::System.Int32",
        "global::System.UInt32",
        "global::System.Int64",
        "global::System.UInt64",
        "global::System.Decimal",
        "global::System.String",
        "global::System.DateTime",
        "global::System.DateTimeOffset",
        "global::System.DateOnly",
        "global::System.TimeOnly",
        "global::System.Half",
        "global::System.Int128",
        "global::System.UInt128" ,
        "global::System.Numerics.BigInteger",
        "global::System.TimeSpan",
        "global::System.Uri",
        "global::System.Guid",
        "global::System.Version",
        "global::System.Text.StringBuilder",
        "global::System.Collections.BitArray",
        "global::System.Type",
        "global::System.Numerics.Complex"
    };

    public static bool ContainsCollectionType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol nameType)
            return false;
        if (!nameType.IsGenericType)
            return false;

        var collectionType = nameType.ConstructUnboundGenericType();
        var fullTypeString = collectionType.ToFullFormatString();
        return builtInCollectionFormatterTypes.TryGetValue(fullTypeString, out _) ||
               builtInInterfaceCollectionFormatterTypes.TryGetValue(fullTypeString, out _);
    }

    public static bool ContainsCollectionInterfaceType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol nameType)
            return false;
        if (!nameType.IsGenericType)
            return false;

        var collectionType = nameType.ConstructUnboundGenericType();
        var fullTypeString = collectionType.ToFullFormatString();
        return builtInInterfaceCollectionFormatterTypes.TryGetValue(fullTypeString, out _);
    }

    public static bool ContainsCollectionAny(ITypeSymbol type)
    {
        if (ContainsCollectionType(type))
            return true;

        return type.AllInterfaces.OfType<INamedTypeSymbol>().Any(t => ContainsCollectionType(t));
    }

    public static bool ContainsDictionary(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol nameType)
            return false;
        if (!nameType.IsGenericType)
            return false;

        var dictType = nameType.ConstructUnboundGenericType();
        return builtInDictionaryFormatterTypes.TryGetValue(dictType.ToFullFormatString(), out _);
    }

    public static bool ContainsDictionaryAny(ITypeSymbol type)
    {
        if (ContainsDictionary(type))
            return true;

        return type.AllInterfaces.OfType<INamedTypeSymbol>().Any(t => ContainsDictionary(t));
    }

    public static GenericFormatterType TryGetGenericFormatterType(ITypeSymbol formatterTypeSymbol, out string? formatter)
    {
        if (formatterTypeSymbol is not INamedTypeSymbol nameformatterTypeSymbol)
        {
            formatter = null;
            return GenericFormatterType.None;
        }
        if (!nameformatterTypeSymbol.IsGenericType)
        {
            formatter = null;
            return GenericFormatterType.None;
        }

        var formatterType = nameformatterTypeSymbol.ConstructUnboundGenericType();
        var formatterTypeString = formatterType.ToFullFormatString();

        return TryGetGenericFormatterType(formatterTypeString, out formatter);
    }

    public static GenericFormatterType TryGetGenericFormatterType(string formatterType, out string? formatter)
    {
        if (builtInCollectionFormatterTypes.TryGetValue(formatterType, out formatter))
        {
            return GenericFormatterType.Collection;
        }

        if (builtInInterfaceCollectionFormatterTypes.TryGetValue(formatterType, out formatter))
        {
            return GenericFormatterType.Collection;
        }

        if (builtInDictionaryFormatterTypes.TryGetValue(formatterType, out formatter))
        {
            return GenericFormatterType.Dictionary;
        }

        if(builtInGenericFormatterTypes.TryGetValue(formatterType, out formatter))
        {
            return GenericFormatterType.Collection;
        }

        if (otherGenericFormatterTypes.TryGetValue(formatterType, out formatter))
        {
            return GenericFormatterType.Other;
        }

        return GenericFormatterType.None;
    }

    public static bool ContainsBuiltInFormatterType(ITypeSymbol typeSymbol)
    {
        // If typeSymbol is SymbolKind.TypeParameter, it always return false.
        if (typeSymbol.Kind == SymbolKind.TypeParameter)
        {
            return false;
        }

        if (TryGetNullableParameterType(typeSymbol, out var nullableTypeSymbol))
        {
            return builtInFormatterTypeMap.Contains(nullableTypeSymbol!.ToFullFormatString());
        }

        return builtInFormatterTypeMap.Contains(typeSymbol.ToFullFormatString());
    }

    public static TomlSerializationKind GetTomlSerializationKind(ITypeSymbol type)
    {
        if (ContainsBuiltInFormatterType(type))
        {
            return TomlSerializationKind.Primitive;
        }

        if (type.Kind == SymbolKind.TypeParameter)
        {
            return TomlSerializationKind.TypeParameter;
        }

        // TypeParameter with nullable struct is a special case.
        if (type is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeArguments.Length > 0)
        {
            var isTypeParameter = namedTypeSymbol.TypeArguments[0].Kind;
            if (namedTypeSymbol.IsGenericType &&
                namedTypeSymbol.IsValueType &&
                namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T &&
                namedTypeSymbol.TypeArguments[0].Kind == SymbolKind.TypeParameter)
            {
                return TomlSerializationKind.NullableStructWithTypeParameter;
            }
        }

        if (type.SpecialType == SpecialType.System_Object)
        {
            return TomlSerializationKind.Object;
        }

        switch (type.TypeKind)
        {
            case TypeKind.Array:
                if (IsElementPrimitiveType(type, GenericFormatterType.Array, TomlSerializationKind.Primitive))
                {
                    return TomlSerializationKind.PrimitiveArray;
                }
                return TomlSerializationKind.TomlSerializedObjectArray;
            case TypeKind.Enum:
                return TomlSerializationKind.Enum;
            case TypeKind.Class:
                if (type.IsTomlSerializedObject())
                {
                    return TomlSerializationKind.TomlSerializedObject;
                }
                if (ContainsCollectionType(type))
                {
                    if (IsElementPrimitiveType(type, GenericFormatterType.Collection, TomlSerializationKind.Primitive))
                    {
                        return TomlSerializationKind.PrimitiveCollection;
                    }
                    return TomlSerializationKind.TomlSerializedObjectCollection;
                }
                if (ContainsDictionary(type))
                {
                    return TomlSerializationKind.Dictionary;
                }
                return TomlSerializationKind.Class;
            case TypeKind.Interface:
                if (ContainsDictionaryAny(type))
                {
                    return TomlSerializationKind.Dictionary;
                }
                if (ContainsCollectionAny(type))
                {
                    if (IsElementPrimitiveType(type, GenericFormatterType.Collection, TomlSerializationKind.Primitive))
                    {
                        return TomlSerializationKind.PrimitiveCollection;
                    }
                    return TomlSerializationKind.TomlSerializedObjectCollection;
                }
                return TomlSerializationKind.Interface;
            case TypeKind.Struct:
                if (type.IsTomlSerializedObject())
                {
                    return TomlSerializationKind.TomlSerializedObject;
                }

                // For a Nullable<T> Collection/Dictionary like ImmutableArray<T>?
                if (TryGetNullableParameterType(type, out var nullableType) &&
                    (ContainsCollectionType(nullableType!) || ContainsDictionary(nullableType!)))
                {
                    type = nullableType!;
                }

                var genericFormatterType = TryGetGenericFormatterType(type, out var _);
                if (genericFormatterType != GenericFormatterType.None)
                {
                    if (IsElementPrimitiveType(type, genericFormatterType, TomlSerializationKind.Primitive))
                    {
                        return TomlSerializationKind.PrimitiveCollection;
                    }
                    return TomlSerializationKind.TomlSerializedObjectCollection;
                }
                return TomlSerializationKind.Struct;
        }

        return TomlSerializationKind.Error;
    }

    public static bool IsElementPrimitiveType(ITypeSymbol typeSymbol, GenericFormatterType genericFormatterType, TomlSerializationKind kind)
    {
        if (genericFormatterType == GenericFormatterType.None)
            return false;

        if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            ITypeSymbol elementType = arrayTypeSymbol.ElementType;
            while(elementType is IArrayTypeSymbol t)
            {
                elementType = t.ElementType;
            }

            var symbolKind = GetTomlSerializationKind(elementType);
            return symbolKind == kind;
        }

        if (typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType)
        {
            var typeArguments = namedTypeSymbol.TypeArguments;
            if (typeArguments.Length == 1)
            {
                var symbolKind = GetTomlSerializationKind(typeArguments[0]);
                return symbolKind == kind;
            }

            // KeyValuePair, ValueTuple are applicable.
            if (typeArguments.Length >= 2 && genericFormatterType == GenericFormatterType.Collection)
            {
                return typeArguments.All(t => GetTomlSerializationKind(t) == kind);
            }
            return false;
        }

        return false;
    }

    public static bool TryGetNullableParameterType(ITypeSymbol typeSymbol, out ITypeSymbol? nullableType)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType)
        {
            // Nullable<T> is a special case.
            if (namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            {
                nullableType = namedTypeSymbol.TypeArguments[0];
                return true;
            }
        }

        nullableType = null;
        return false;
    }
}