
using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;

namespace CsToml;

[DebuggerDisplay("DocumentNode = {NodeCount}")]
[StructLayout(LayoutKind.Auto)]
public struct TomlDocumentNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly TomlValue value;
    private readonly TomlTable? table;
    private readonly bool isRoot;

    internal readonly int NodeCount => table?.Count ?? 0;

    internal readonly TomlValue Value => value;

    internal readonly TomlTable? Table => table;

    public readonly bool HasValue => Value.HasValue || table != null || isRoot;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly bool HasNodeOnly => !Value.HasValue && NodeCount > 0;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly bool HasValueOnly => Value.HasValue && NodeCount == 0;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly bool IsTableHeader => table?.IsTableHeader ?? false;

    public readonly TomlValueType ValueType => Value.Type;

    public TomlDocumentNode this[ReadOnlySpan<char> key]
    {
        get
        {
            // buffer size to 3 times worst-case (UTF16 -> UTF8)
            var maxBufferSize = (key.Length + 1) * 3;
            if (maxBufferSize < 1024)
            {
                Span<byte> utf8Span = stackalloc byte[maxBufferSize];
                var status = Utf8.FromUtf16(key, utf8Span,  out int charsRead, out int bytesWritten, replaceInvalidSequences: false);
                if (status != System.Buffers.OperationStatus.Done)
                {
                    if (status == OperationStatus.InvalidData)
                        ExceptionHelper.ThrowInvalidByteIncluded();
                    ExceptionHelper.ThrowBufferTooSmallFailed();
                }
                return this[utf8Span[..bytesWritten]];
            }
            else
            {
                var writer = RecycleArrayPoolBufferWriter<byte>.Rent();
                try
                {
                    Utf8Helper.FromUtf16(writer, key);
                    return this[writer.WrittenSpan];
                }
                finally
                {
                    RecycleArrayPoolBufferWriter<byte>.Return(writer);
                }
            }
        }
    }

    public TomlDocumentNode this[ReadOnlySpan<byte> key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var target = table ?? (value as TomlInlineTable)?.Table;
            if (target != null && target.TryGetChildValue(key, out var child))
            {
                return FromChild(child!);
            }
            return new TomlDocumentNode(TomlValue.Empty);
        }
    }

    public TomlDocumentNode this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (Value is TomlArray array)
            {
                var values = array.AsSpan();
                if ((uint)index < (uint)values.Length)
                {
                    return new TomlDocumentNode(values[index]);
                }
            }
            return new TomlDocumentNode(TomlValue.Empty);
        }
    }

    private TomlDocumentNode(TomlValue value, TomlTable? table, bool isRoot)
    {
        this.value = value;
        this.table = table;
        this.isRoot = isRoot;
    }

    internal TomlDocumentNode(TomlTable table, bool isRoot = false)
        : this(TomlValue.Empty, table, isRoot)
    {
    }

    public TomlDocumentNode(TomlValue value)
    {
        if (value is TomlTable table)
        {
            this.value = TomlValue.Empty;
            this.table = table;
        }
        else if (value is TomlInlineTable inlineTable)
        {
            this.value = TomlValue.Empty;
            this.table = inlineTable.Table;
        }
        else
        {
            this.value = value;
            this.table = null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TomlDocumentNode FromChild(TomlValue value)
    {
        if (value is TomlTable table)
        {
            return new TomlDocumentNode(TomlValue.Empty, table, false);
        }
        return new TomlDocumentNode(value, null, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly TomlValue GetTomlValue()
        => Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ImmutableArray<TomlValue> GetArray()
    {
        if (table != null)
        {
            ExceptionHelper.ThrowTypeMismatch(TomlValueType.Table, "array");
        }
        return Value.GetArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<TomlValue> GetArraySpan()
    {
        if (table != null)
        {
            ExceptionHelper.ThrowTypeMismatch(TomlValueType.Table, "array");
        }
        return Value.GetArraySpan();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string GetString()
    {
        if (table != null)
        {
            ExceptionHelper.ThrowTypeMismatch(TomlValueType.Table, "string");
        }
        return Value.GetString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly long GetInt64()
    {
        if (table != null)
        {
            ExceptionHelper.ThrowTypeMismatch(TomlValueType.Table, "long");
        }
        return Value.GetInt64();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly double GetDouble()
    {
        if (table != null)
        {
            ExceptionHelper.ThrowTypeMismatch(TomlValueType.Table, "double");
        }
        return Value.GetDouble();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool GetBool()
    {
        if (table != null)
        {
            ExceptionHelper.ThrowTypeMismatch(TomlValueType.Table, "bool");
        }
        return Value.GetBool();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly DateTime GetDateTime()
    {
        if (table != null)
        {
            ExceptionHelper.ThrowTypeMismatch(TomlValueType.Table, "DateTime");
        }
        return Value.GetDateTime();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly DateTimeOffset GetDateTimeOffset()
    {
        if (table != null)
        {
            ExceptionHelper.ThrowTypeMismatch(TomlValueType.Table, "DateTimeOffset");
        }
        return Value.GetDateTimeOffset();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly DateOnly GetDateOnly()
    {
        if (table != null)
        {
            ExceptionHelper.ThrowTypeMismatch(TomlValueType.Table, "DateOnly");
        }
        return Value.GetDateOnly();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly TimeOnly GetTimeOnly()
    {
        if (table != null)
        {
            ExceptionHelper.ThrowTypeMismatch(TomlValueType.Table, "TimeOnly");
        }
        return Value.GetTimeOnly();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly object GetObject()
        => table != null ? table.GetObject() : Value.GetObject();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetArray(out ImmutableArray<TomlValue> value)
        => Value.TryGetArray(out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetArraySpan(out ReadOnlySpan<TomlValue> value)
        => Value.TryGetArraySpan(out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetString([NotNullWhen(true)] out string? value)
        => Value.TryGetString(out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetInt64(out long value)
        => Value.TryGetInt64(out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetDouble(out double value)
        => Value.TryGetDouble(out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetBool(out bool value)
        => Value.TryGetBool(out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetDateTime(out DateTime value)
        => Value.TryGetDateTime(out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetDateTimeOffset(out DateTimeOffset value)
        => Value.TryGetDateTimeOffset(out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetDateOnly(out DateOnly value)
        => Value.TryGetDateOnly(out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetTimeOnly(out TimeOnly value)
        => Value.TryGetTimeOnly(out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetObject([NotNullWhen(true)] out object? value)
    {
        if (table != null)
        {
            value = table.GetObject();
            return true;
        }
        return Value.TryGetObject(out value);
    }

    public readonly T GetValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(CsTomlSerializerOptions? options = null)
    {
        options ??= CsTomlSerializer.DefaultOptions;
        var node = this;

        try
        {
            return options.Resolver.GetFormatter<T>()!.Deserialize(ref node, options);
        }
        catch (CsTomlException cte)
        {
            throw new CsTomlSerializeException("An exception was thrown while deserializing a TOML node. See the 'InnerException' property for details.", cte);
        }
    }

    public readonly bool TryGetValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>([MaybeNullWhen(false)] out T value, CsTomlSerializerOptions? options = null)
    {
        if (!HasValue)
        {
            value = default;
            return false;
        }

        options ??= CsTomlSerializer.DefaultOptions;
        var node = this;

        try
        {
            value = options.Resolver.GetFormatter<T>()!.Deserialize(ref node, options);
            return true;
        }
        catch (CsTomlException)
        {
            value = default;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NodeEnumerator GetEnumerator()
        => new (table, HasValue);

    [StructLayout(LayoutKind.Auto)]
    public ref struct NodeEnumerator
    {
        private TomlTable.KeyValuePairEnumerator enumerator;
        private bool hasValue;
        private KeyValuePair<TomlValue, TomlDocumentNode> current;

        public readonly KeyValuePair<TomlValue, TomlDocumentNode> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => current;
        }

        internal NodeEnumerator(TomlTable? table, bool hasValue)
        {
            enumerator = new TomlTable.KeyValuePairEnumerator(table);
            this.hasValue = hasValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (!hasValue) return false;

            if (!enumerator.MoveNext())
            {
                return false;
            }

            var pair = enumerator.Current;
            if (pair.Value is TomlTable childTable)
            {
                current = new KeyValuePair<TomlValue, TomlDocumentNode>(pair.Key, new TomlDocumentNode(childTable));
            }
            else
            {
                current = new KeyValuePair<TomlValue, TomlDocumentNode>(pair.Key, new TomlDocumentNode(pair.Value));
            }
            return true;
        }
    }
}
