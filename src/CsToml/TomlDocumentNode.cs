
using CsToml.Error;
using CsToml.Formatter.Resolver;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
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

    public readonly bool HasValue => Value.HasValue || NodeCount > 0 || isRoot;

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
            if (Value.TryGetArrayValue(index, out var value))
            {
                return new TomlDocumentNode(value);
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
    public readonly bool CanGetValue(TomlValueFeature feature)
        => Value.CanGetValue(feature);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlyCollection<TomlValue> GetArray()
        => Value.GetArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly TomlValue GetArrayValue(int index)
        => Value.GetArrayValue(index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ImmutableArray<TomlValue> GetImmutableArray()
        => Value.GetImmutableArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string GetString()
        => Value.GetString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly long GetInt64()
        => Value.GetInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly double GetDouble()
        => Value.GetDouble();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool GetBool()
        => Value.GetBool();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly DateTime GetDateTime()
        => Value.GetDateTime();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly DateTimeOffset GetDateTimeOffset()
        => Value.GetDateTimeOffset();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly DateOnly GetDateOnly()
        => Value.GetDateOnly();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly TimeOnly GetTimeOnly()
        => Value.GetTimeOnly();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly object GetObject()
        => Value.GetObject();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T GetNumber<T>() where T : struct, INumberBase<T>
        => Value.GetNumber<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValue<T>()
    {
        return TomlValueFormatterResolver.Instance.GetFormatter<T>()!.
            Deserialize(ref this, CsTomlSerializerOptions.Default);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetArray(out ReadOnlyCollection<TomlValue> value)
        => Value.TryGetArray(out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetArrayValue(int index, out TomlValue value)
        => Value.TryGetArrayValue(index, out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetImmutableArray(out ImmutableArray<TomlValue> value)
        => Value.TryGetImmutableArray(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetString(out string value)
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
    public readonly bool TryGetObject(out object value)
        => Value.TryGetObject(out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetNumber<T>(out T value) where T : struct, INumberBase<T>
        => Value.TryGetNumber<T>(out value);

    public bool TryGetValue<T>(out T value)
    {
        try
        {
            value = GetValue<T>();
            return true;
        }
        catch (CsTomlException)
        {
            value = default!;
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
                // An empty table is enumerated as an empty node that does not refer to the table.
                current = new KeyValuePair<TomlValue, TomlDocumentNode>(pair.Key, childTable.Count > 0 ? new TomlDocumentNode(childTable) : new TomlDocumentNode(TomlValue.Empty));
            }
            else
            {
                current = new KeyValuePair<TomlValue, TomlDocumentNode>(pair.Key, new TomlDocumentNode(pair.Value));
            }
            return true;
        }
    }
}
