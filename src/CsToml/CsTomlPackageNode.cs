
using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Unicode;

namespace CsToml;

[DebuggerDisplay("{Value}")]
public struct CsTomlPackageNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private CsTomlTableNode node;

    public readonly CsTomlValue Value => node.Value!;

    public CsTomlPackageNode this[ReadOnlySpan<char> key]
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
                var writer = new ArrayPoolBufferWriter<byte>(128);
                using var _ = writer;
                var keyrWriter = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref writer);

                ValueFormatter.Serialize(ref keyrWriter, key);
                return this[writer.WrittenSpan[..keyrWriter.WrittingCount]];
            }
        }
    }

    public CsTomlPackageNode this[ReadOnlySpan<byte> key]
    {
        get
        {
            if (node.TryGetChildNode(key, out var value))
            {
                node = value!;
                return this;
            }
            node = CsTomlTableNode.Empty;
            return this;
        }
    }

    public CsTomlPackageNode this[int index]
    {
        get
        {
            if (Value is CsTomlArray arrayValue)
            {
                if (arrayValue[index] is CsTomlTable table)
                {
                    node = table!.RootNode;
                }
                else if (arrayValue[index] is CsTomlInlineTable inlineTable)
                {
                    node = inlineTable!.RootNode;
                }
                else
                {
                    node = CsTomlTableNode.Empty;
                }
                return this;
            }
            node = CsTomlTableNode.Empty;
            return this;
        }
    }

    public readonly bool HasValue => Value.HasValue;

    internal CsTomlPackageNode(CsTomlTableNode node)
    {
        this.node = node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool CanGetValue(CsTomlValueFeature feature)
        => Value.CanGetValue(feature);

    public readonly ReadOnlyCollection<CsTomlValue> GetArray()
        => Value.GetArray();

    public readonly CsTomlValue GetArrayValue(int index)
        => Value.GetArrayValue(index);

    public readonly string GetString()
        => Value.GetString();

    public readonly long GetInt64()
        => Value.GetInt64();

    public readonly double GetDouble()
        => Value.GetDouble();

    public readonly bool GetBool()
        => Value.GetBool();

    public readonly DateTime GetDateTime()
        => Value.GetDateTime();

    public readonly DateTimeOffset GetDateTimeOffset()
        => Value.GetDateTimeOffset();

    public readonly DateOnly GetDateOnly()
        => Value.GetDateOnly();

    public readonly TimeOnly GetTimeOnly()
        => Value.GetTimeOnly();

    public readonly T GetNumber<T>() where T : struct, INumberBase<T>
        => Value.GetNumber<T>();

    public readonly bool TryGetArray(out ReadOnlyCollection<CsTomlValue> value)
        => Value.TryGetArray(out value);

    public readonly bool TryGetArrayValue(int index, out CsTomlValue value)
        => Value.TryGetArrayValue(index, out value);

    public readonly bool TryGetString(out string value)
        => Value.TryGetString(out value);

    public readonly bool TryGetInt64(out long value)
        => Value.TryGetInt64(out value);

    public readonly bool TryGetDouble(out double value)
        => Value.TryGetDouble(out value);

    public readonly bool TryGetBool(out bool value)
        => Value.TryGetBool(out value);

    public readonly bool TryGetDateTime(out DateTime value)
        => Value.TryGetDateTime(out value);

    public readonly bool TryGetDateTimeOffset(out DateTimeOffset value)
        => Value.TryGetDateTimeOffset(out value);

    public readonly bool TryGetDateOnly(out DateOnly value)
        => Value.TryGetDateOnly(out value);

    public readonly bool TryGetTimeOnly(out TimeOnly value)
        => Value.TryGetTimeOnly(out value);

    public readonly bool TryGetNumber<T>(out T value) where T : struct, INumberBase<T>
        => Value.TryGetNumber<T>(out value);

}
