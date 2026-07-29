using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Unicode;

namespace CsToml.Values;

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlUnquotedDottedKey : TomlDottedKey, ITomlStringParser<TomlUnquotedDottedKey>
{
    public static readonly TomlUnquotedDottedKey EmptyString = new([]);

    public TomlUnquotedDottedKey(ReadOnlySpan<byte> value) : base(value)
    { }

    public TomlUnquotedDottedKey(byte[] value) : base(value)
    { }

    static TomlUnquotedDottedKey ITomlStringParser<TomlUnquotedDottedKey>.Parse(ReadOnlySpan<byte> value)
    {
        if (value.Length == 0)
        {
            return EmptyString;
        }
        return new TomlUnquotedDottedKey(value);
    }

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        if (Value.Length > 0)
        {
            writer.WriteBytes(Value);
        }
    }
}

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlBasicDottedKey : TomlDottedKey, ITomlStringParser<TomlBasicDottedKey>
{
    public static readonly TomlBasicDottedKey EmptyString = new([]);

    public TomlBasicDottedKey(ReadOnlySpan<byte> value) : base(value)
    { }

    public TomlBasicDottedKey(byte[] value) : base(value)
    { }

    static TomlBasicDottedKey ITomlStringParser<TomlBasicDottedKey>.Parse(ReadOnlySpan<byte> value)
    {
        if (value.Length == 0)
        {
            return EmptyString;
        }
        return new TomlBasicDottedKey(value);
    }

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        TomlBasicString.ToTomlBasicString(ref writer, Value);
    }
}

[DebuggerDisplay("{Utf16String}")]
internal sealed class TomlLiteralDottedKey : TomlDottedKey, ITomlStringParser<TomlLiteralDottedKey>
{
    public static readonly TomlLiteralDottedKey EmptyString = new([]);

    public TomlLiteralDottedKey(ReadOnlySpan<byte> value) : base(value)
    { }

    public TomlLiteralDottedKey(byte[] value) : base(value)
    { }

    static TomlLiteralDottedKey ITomlStringParser<TomlLiteralDottedKey>.Parse(ReadOnlySpan<byte> value)
    {
        if (value.Length == 0)
        {
            return EmptyString;
        }
        return new TomlLiteralDottedKey(value);
    }

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        TomlLiteralString.ToTomlLiteralString(ref writer, Value);
    }
}

[DebuggerDisplay("{Utf16String}")]
internal abstract partial class TomlDottedKey : TomlValue, IEquatable<TomlDottedKey?>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private byte[] bytes;

    public override bool HasValue => true;

    public override TomlValueType Type => TomlValueType.Key;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public ReadOnlySpan<byte> Value => bytes.AsSpan();

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public string Utf16String => Utf8Helper.ToUtf16(Value);

    protected TomlDottedKey(ReadOnlySpan<byte> value) : this(value.ToArray())
    {}

    protected TomlDottedKey(byte[] value) : base()
    {
        bytes = value;
    }

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        var status = Utf8.ToUtf16(Value, destination, out var bytesRead, out charsWritten, replaceInvalidSequences: false);
        return status == OperationStatus.Done;
    }

    public override string ToString(string? format, IFormatProvider? formatProvider) => Utf16String;

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (utf8Destination.Length < Value.Length)
        {
            bytesWritten = 0;
            return false;
        }

        Value.TryCopyTo(utf8Destination);
        bytesWritten = Value.Length;
        return true;
    }

    public override string ToString() => Utf16String;

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;

        return Equals((TomlDottedKey)obj);
    }

    public bool Equals(TomlDottedKey? other)
    {
        if (other == null) return false;

        return Equals(other.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ReadOnlySpan<byte> other)
        => Value.SequenceEqual(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetHashCodeFast()
        => ByteArrayHash.ToInt32(Value);

    public override int GetHashCode()
        => GetHashCodeFast();
}

internal static class TomlDottedKeyExtensions
{
    public static string GetJoinName(this ReadOnlySpan<TomlDottedKey> key)
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            InlineArray4<TomlDottedKey> initialKeys = default;
            ref InlineArray4<TomlDottedKey> initialKeysRef = ref Unsafe.AsRef(in initialKeys);
            TempList<TomlDottedKey> keyList = new(initialKeysRef);

            InlineArray4<(TomlValueState state, int dottedKeyIndex)> initialStates = default;
            ref InlineArray4<(TomlValueState state, int dottedKeyIndex)> initialStatesRef = ref Unsafe.AsRef(in initialStates);
            TempList<(TomlValueState state, int dottedKeyIndex)> stateList = new(initialStatesRef);

            var writer = new Utf8TomlDocumentWriter<ArrayPoolBufferWriter<byte>>(ref bufferWriter, ref keyList, ref stateList, false, CsTomlSerializerOptions.Default);

            for (int i = 0; i < key.Length; i++)
            {
                key[i].ToTomlString(ref writer);
                if (i < key.Length - 1)
                    writer.Write(TomlCodes.Symbol.DOT);
            }

            return Utf8Helper.ToUtf16(bufferWriter.WrittenSpan); ;
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }
}