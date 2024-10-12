
using CsToml.Error;
using CsToml.Formatter.Resolver;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Unicode;

namespace CsToml;

[DebuggerDisplay("DocumentNode = {NodeCount}")]
public struct TomlDocumentNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly TomlValue value;
    private readonly TomlTableNode node;

    internal readonly int NodeCount => node?.NodeCount ?? 0;

    internal readonly TomlValue Value => value;

    internal readonly TomlTableNode Node => node;

    public readonly bool HasValue => Value.HasValue || NodeCount > 0;

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
        get
        {
            if (node?.TryGetChildNode(key, out var value) ?? false)
            {
                return new TomlDocumentNode(value!);
            }
            return new TomlDocumentNode(TomlTableNode.Empty);
        }
    }

    public TomlDocumentNode this[int index]
    {
        get
        {
            if (Value.TryGetArrayValue(index, out var value))
            {
                if (value is TomlTable table)
                {
                    return new TomlDocumentNode(table!.RootNode!);
                }
                else if (value is TomlInlineTable inlineTable)
                {
                    return new TomlDocumentNode(inlineTable!.RootNode);
                }
                return new TomlDocumentNode(value);
            }
            return new TomlDocumentNode(TomlTableNode.Empty);
        }
    }

    internal TomlDocumentNode(TomlTableNode node)
    {
        this.node = node;
        this.value = node.Value!;
    }

    internal TomlDocumentNode(TomlValue value)
    {
        if (value is TomlTable table)
        {
            this.value = TomlValue.Empty;
            this.node = table!.RootNode!;
        }
        else if (value is TomlInlineTable inlineTable)
        {
            this.value = TomlValue.Empty;
            this.node = inlineTable!.RootNode!;
        }
        else
        {
            this.value = value;
            this.node = TomlTableNode.Empty;
        }
    }

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
    public bool TryGetObject(out object value)
        => Value.TryGetObject(out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetNumber<T>(out T value) where T : struct, INumberBase<T>
        => Value.TryGetNumber<T>(out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    internal IDictionary<string, object?> GetDictionary()
    {
        return node?.GetDictionary() ?? new Dictionary<string,object?>();
    }

    internal bool TryGetDictionary(out IDictionary<string, object?> value)
    {
        try
        {
            value = GetDictionary();
            return true;
        }
        catch(CsTomlException)
        {
            value = default!;
            return false;
        }
    }
}
