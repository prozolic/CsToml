using CsToml.Error;
using CsToml.Extension;
using CsToml.Formatter;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml;

public enum TomlValueState
{
    Default,
    Array,
    ArrayOfTable,
    Table,
    ArrayOfTableForMulitiLine,
}

[StructLayout(LayoutKind.Auto)]
public ref struct Utf8TomlDocumentWriter
{
    private Span<byte> buffer;
    private int position;
    private ArrayPoolBufferSource arrayPoolSource;
    private WriterBufferSource writerSource;
    private TempList<TomlDottedKey> dottedKeys;
    private TempList<(TomlValueState state, int dottedKeyIndex)> valueStates;
    private int arrayOfTableDepth;
    private readonly bool valueOnly;
    private readonly CsTomlSerializerOptions options;

    internal readonly long WrittenSize => writerSource.CommittedLength + position;

    internal readonly ReadOnlySpan<byte> WrittenSpan => buffer.Slice(0, position);

    internal readonly (TomlValueState state, int dottedKeyIndex) CurrentPreviousState => valueStates[^2];

    internal readonly (TomlValueState state, int dottedKeyIndex) CurrentState => valueStates[^1];

    public readonly TomlValueState State => CurrentState.state;

    public readonly bool IsRoot => valueStates.Count == 1 && !valueOnly;

    internal Utf8TomlDocumentWriter(
        ArrayPoolBufferSource arrayPoolSource,
        scoped in TempList<TomlDottedKey> dottedKeys,
        scoped in TempList<(TomlValueState state, int dottedKeyIndex)> valueStates,
        bool valueOnly,
        CsTomlSerializerOptions? options)
    {
        this.arrayPoolSource = arrayPoolSource;
        buffer = arrayPoolSource.Buffer;
        this.dottedKeys = dottedKeys;
        this.valueStates = valueStates;
        this.valueStates.Add((TomlValueState.Default, -1));
        this.valueOnly = valueOnly;
        this.options = options ?? CsTomlSerializerOptions.Default;
    }

    internal Utf8TomlDocumentWriter(
        WriterBufferSource writerSource,
        scoped in TempList<TomlDottedKey> dottedKeys,
        scoped in TempList<(TomlValueState state, int dottedKeyIndex)> valueStates,
        bool valueOnly,
        CsTomlSerializerOptions? options)
    {
        this.writerSource = writerSource;
        this.dottedKeys = dottedKeys;
        this.valueStates = valueStates;
        this.valueStates.Add((TomlValueState.Default, -1));
        this.valueOnly = valueOnly;
        this.options = options ?? CsTomlSerializerOptions.Default;
    }

    internal Utf8TomlDocumentWriter(
        Span<byte> destination,
        scoped in TempList<TomlDottedKey> dottedKeys,
        scoped in TempList<(TomlValueState state, int dottedKeyIndex)> valueStates,
        bool valueOnly,
        CsTomlSerializerOptions? options)
    {
        buffer = destination;
        this.dottedKeys = dottedKeys;
        this.valueStates = valueStates;
        this.valueStates.Add((TomlValueState.Default, -1));
        this.valueOnly = valueOnly;
        this.options = options ?? CsTomlSerializerOptions.Default;
    }

    internal void Flush()
    {
        if (!writerSource.IsActive) return;

        var pos = position;
        buffer = default;
        position = 0;
        writerSource.Commit(pos);
    }

    internal void Dispose()
    {
        buffer = default;
        position = 0;
        arrayPoolSource.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Span<byte> GetWrittenSpan(int length)
    {
        var pos = position;
        if ((uint)length > (uint)(buffer.Length - pos))
        {
            Grow(length);
            pos = position;
        }

        position = pos + length;
        return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), (nint)(uint)pos), length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Span<byte> GetSpan(int sizeHint)
    {
        if (sizeHint == 0)
        {
            sizeHint = 1;
        }

        var pos = position;
        if ((uint)sizeHint > (uint)(buffer.Length - pos))
        {
            Grow(sizeHint);
            pos = position;
        }

        return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), (nint)(uint)pos), buffer.Length - pos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Advance(int count)
    {
        var pos = position;
        if ((uint)count > (uint)(buffer.Length - pos))
        {
            ExceptionHelper.ThrowInvalidAdvance();
        }

        position = pos + count;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int sizeHint)
    {
        if (writerSource.IsActive)
        {
            var pos = position;
            buffer = default;
            position = 0;
            buffer = writerSource.Next(pos, sizeHint);
            return;
        }

        buffer = arrayPoolSource.Grow(position, sizeHint);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BeginScope()
    {
        if (valueStates.Count == 0) return;

        if (CurrentState.state == TomlValueState.ArrayOfTable)
        {
            BeginInlineTable();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndScope()
    {
        if (valueStates.Count == 0) return;

        if (CurrentState.state == TomlValueState.ArrayOfTable)
        {
            EndInlineTable();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushKey(ReadOnlySpan<byte> key)
    {
        dottedKeys.Add(TomlDottedKeyHelper.ParseKey(key, options.Spec.SupportsEscapeSequenceE, options.Spec.SupportsEscapeSequenceX));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushBareKey(ReadOnlySpan<byte> key, bool skipValidation = false)
    {
        dottedKeys.Add(TomlDottedKeyHelper.ParseBareKey(key, skipValidation));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushBareKey(ImmutableArray<byte> immutableKey, bool skipValidation = false)
    {
        var key = ImmutableCollectionsMarshal.AsArray(immutableKey);
        if (key is null)
        {
            ExceptionHelper.ThrowUninitializedKey();
        }

        dottedKeys.Add(TomlDottedKeyHelper.ParseBareKey(key, skipValidation));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void PushKeyForPrimitive<T>(T value)
    {
        dottedKeys.Add(TomlDottedKeyHelper.ParseKeyForPrimitive(value, options));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PopKey()
    {
        dottedKeys.RemoveLast();
    }

    public void EndKeyValue(bool lastValue = false)
    {
        if (valueStates.Count > 0)
        {
            var state = CurrentState.state;
            switch (state)
            {
                case TomlValueState.ArrayOfTable:
                    if (lastValue) return;
                    WriteComma();
                    return;
                default:
                    if (arrayOfTableDepth == 0)
                    {
                        WriteNewLine();
                    }
                    return;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BeginCurrentState(TomlValueState state)
    {
        if (state == TomlValueState.ArrayOfTable) arrayOfTableDepth++;
        valueStates.Add((state, dottedKeys.Count));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndCurrentState()
    {
        if (CurrentState.state == TomlValueState.ArrayOfTable) arrayOfTableDepth--;
        valueStates.RemoveLast();
    }

    public void WriteBoolean(bool value)
    {
        if (value)
        {
            WriteBytes("true"u8);
        }
        else
        {
            WriteBytes("false"u8);
        }
    }

    public void WriteInt64(long value)
    {
        var length = TomlCodes.Number.CountDigitsForInt64(value);
        if (value < 0) length++;

        value.TryFormat(GetWrittenSpan(length), out int bytesWritten, null, CultureInfo.InvariantCulture);
    }

    internal void WriteInt64InBinaryFormat(long value)
    {
        if (value == 0)
        {
            WriteBytes("0b0"u8);
            return;
        }

        WriteBytes("0b"u8);

        int digits = 64 - BitOperations.LeadingZeroCount((ulong)value);
        var writtenSpan = GetWrittenSpan(digits);
        var index = digits - 1;

        while (value > 0)
        {
            var v = value & 1;
            writtenSpan[index--] = (byte)(TomlCodes.Number.Zero + (byte)v);
            value >>= 1;
        }
    }

    internal void WriteInt64InOctalFormat(long value)
    {
        if (value == 0)
        {
            WriteBytes("0o0"u8);
            return;
        }

        WriteBytes("0o"u8);

        int bitLength = 64 - BitOperations.LeadingZeroCount((ulong)value);
        var digits = (bitLength + 2) / 3;
        var writtenSpan = GetWrittenSpan(digits);
        var index = digits - 1;

        while (value > 0)
        {
            var v = value & 7;
            writtenSpan[index--] = (byte)(TomlCodes.Number.Zero + (byte)v);
            value >>= 3;
        }
    }

    internal void WriteInt64InHexFormat(long value)
    {
        if (value == 0)
        {
            WriteBytes("0x0"u8);
            return;
        }

        WriteBytes("0x"u8);

        int bitLength = 64 - BitOperations.LeadingZeroCount((ulong)value);
        var digits = (bitLength + 3) / 4;
        var writtenSpan = GetWrittenSpan(digits);
        var index = digits - 1;

        while (value > 0)
        {
            var v = (int)(value & 15);
            writtenSpan[index--] = (byte)(v < 10 ? (TomlCodes.Number.Zero + v) : (TomlCodes.Alphabet.A + v - 10));
            value >>= 4;
        }
    }

    private static readonly SearchValues<byte> ExponentialBytes = SearchValues.Create(".eE"u8);

    public void WriteDouble(double value)
    {
        switch (value)
        {
            case double.NegativeInfinity:
                WriteBytes("-inf"u8);
                return;
            case double.PositiveInfinity:
                WriteBytes("inf"u8);
                return;
            case double.NaN:
                WriteBytes("nan"u8);
                return;
            default:
                var length = 32;
                int bytesWritten;

                var writtenSpan = GetSpan(0);
                while (!value.TryFormat(writtenSpan, out bytesWritten, "G", CultureInfo.InvariantCulture))
                {
                    writtenSpan = GetSpan(length);
                    length *= 2;
                }

                var needsDotZero = !writtenSpan.Slice(0, bytesWritten).ContainsAny(ExponentialBytes);
                Advance(bytesWritten);

                if (needsDotZero)
                {
                    var writtenSpanEx = GetWrittenSpan(2);
                    writtenSpanEx[0] = TomlCodes.Symbol.DOT;
                    writtenSpanEx[1] = TomlCodes.Number.Zero;
                }
                return;
        }
    }

    public void WriteDecimal(decimal value)
    {
        var length = 64;
        int bytesWritten;
        var writtenSpan = GetSpan(0);
        while (!value.TryFormat(writtenSpan, out bytesWritten, default, CultureInfo.InvariantCulture))
        {
            writtenSpan = GetSpan(length);
            length *= 2;
        }
        Advance(bytesWritten);
    }

#if NET11_0_OR_GREATER
    internal void WriteDecimalFloatingPoint<T>(T value)
        where T : IDecimalFloatingPointIeee754<T>
    {
        if (!T.IsFinite(value))
        {
            if (T.IsNaN(value))
            {
                WriteBytes("nan"u8);
            }
            else
            {
                WriteBytes(T.IsNegative(value) ? "-inf"u8 : "inf"u8);
            }
            return;
        }

        var length = 64;
        int bytesWritten;
        var writtenSpan = GetSpan(0);
        while (!value.TryFormat(writtenSpan, out bytesWritten, default, CultureInfo.InvariantCulture))
        {
            writtenSpan = GetSpan(length);
            length *= 2;
        }

        // Digits only is a TOML integer, which must fit in Int64. Decimal128 can exceed it (up to 34 digits),
        // so such a value is written as a float.
        var written = writtenSpan.Slice(0, bytesWritten);
        var needsDotZero = bytesWritten > 18 &&
            !written.ContainsAny(ExponentialBytes) &&
            !long.TryParse(written, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _);
        Advance(bytesWritten);

        if (needsDotZero)
        {
            var writtenSpanEx = GetWrittenSpan(2);
            writtenSpanEx[0] = TomlCodes.Symbol.DOT;
            writtenSpanEx[1] = TomlCodes.Number.Zero;
        }
    }
#endif

    public void WriteString(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            WriteBytes("\"\""u8);
            return;
        }

        var valueSpan = value.AsSpan();

        // buffer size to 3 times worst-case (UTF16 -> UTF8)
        var maxBufferSize = (valueSpan.Length + 1) * 3;
        if (maxBufferSize <= 1024)
        {
            Span<byte> dest = stackalloc byte[maxBufferSize];
            Utf8Helper.FromUtf16(valueSpan, dest, out var _, out var bytesWritten);

            if (bytesWritten > 0)
            {
                ref byte destReference = ref MemoryMarshal.GetReference(dest);
                unsafe
                {
                    fixed (byte* ptr = &destReference)
                    {
                        var writtenSpan = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(ptr), bytesWritten);
                        WriteStringInternal(writtenSpan, TomlStringHelper.GetTomlStringType(writtenSpan, options.Spec.SupportsEscapeSequenceE, options.Spec.SupportsEscapeSequenceX));
                    }
                }
            }
        }
        else
        {
            var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
            try
            {
                Utf8Helper.FromUtf16(bufferWriter, value.AsSpan());
                WriteStringInternal(bufferWriter.WrittenSpan, TomlStringHelper.GetTomlStringType(bufferWriter.WrittenSpan, options.Spec.SupportsEscapeSequenceE, options.Spec.SupportsEscapeSequenceX));
            }
            finally
            {
                RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
            }
        }
    }

    private void WriteStringInternal(ReadOnlySpan<byte> value, TomlStringType tomlStringType)
    {
        switch (tomlStringType)
        {
            case TomlStringType.Unquoted:
                WriteBytes(value);
                break;
            case TomlStringType.Basic:
                TomlBasicString.ToTomlBasicString(ref this, value);
                break;
            case TomlStringType.Literal:
                TomlLiteralString.ToTomlLiteralString(ref this, value);
                break;
            case TomlStringType.MultiLineBasic:
                TomlMultiLineBasicString.ToTomlMultiLineBasicString(ref this, value);
                break;
            case TomlStringType.MultiLineLiteral:
                TomlMultiLineLiteralString.ToTomlMultiLineLiteralString(ref this, value);
                break;
            default:
                TomlLiteralString.ToTomlLiteralString(ref this, value);
                break;
        }
    }

    public void WriteDateTimeOffset(DateTimeOffset value)
    {
        var totalMicrosecond = value.Millisecond * 1000 + value.Microsecond;
        var offsetTotalMinutes = value.Offset.TotalMinutes; // check TimeZone

        if (offsetTotalMinutes == 0)
        {
            if (totalMicrosecond == 0)
            {
                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ssZ");
            }
            else
            {
                if (value.Microsecond == 0)
                {
                    var length = TomlCodes.Number.CountDigitsForInt64(value.Millisecond);
                    switch (length)
                    {
                        case 1:
                            WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffZ");
                            break;
                        case 2:
                            if (value.Millisecond % 10 == 0)
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffZ");
                            else
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffZ");
                            break;
                        case 3:
                            if (value.Millisecond % 100 == 0)
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fZ");
                            else if (value.Millisecond % 10 == 0)
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffZ");
                            else
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffZ");
                            break;
                        default:
                            WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffZ");
                            break;
                    }
                }
                else
                {
                    var length = TomlCodes.Number.CountDigitsForInt64(value.Microsecond);
                    switch (length)
                    {
                        case 1:
                            WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffffZ");
                            break;
                        case 2:
                            if (value.Microsecond % 10 == 0)
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffffZ");
                            else
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffffZ");
                            break;
                        case 3:
                            if (value.Microsecond % 100 == 0)
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffZ");
                            else if (value.Microsecond % 10 == 0)
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffffZ");
                            else
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffffZ");
                            break;
                        default:
                            WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffffZ");
                            break;
                    }
                }
            }
        }
        else
        {
            if (totalMicrosecond == 0)
            {
                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:sszzz");
            }
            else
            {
                if (value.Microsecond == 0)
                {
                    var length = TomlCodes.Number.CountDigitsForInt64(value.Millisecond);
                    switch (length)
                    {
                        case 1:
                            WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffzzz");
                            break;
                        case 2:
                            if (value.Millisecond % 10 == 0)
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffzzz");
                            else
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffzzz");
                            break;
                        case 3:
                            if (value.Millisecond % 100 == 0)
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fzzz");
                            else if (value.Millisecond % 10 == 0)
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffzzz");
                            else
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffzzz");
                            break;
                        default:
                            WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffzzz");
                            break;
                    }
                }
                else
                {
                    var length = TomlCodes.Number.CountDigitsForInt64(value.Microsecond);
                    switch (length)
                    {
                        case 1:
                            WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffffzzz");
                            break;
                        case 2:
                            if (value.Microsecond % 10 == 0)
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffffzzz");
                            else
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffffzzz");
                            break;
                        case 3:
                            if (value.Microsecond % 100 == 0)
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffzzz");
                            else if (value.Microsecond % 10 == 0)
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffffzzz");
                            else
                                WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffffzzz");
                            break;
                        default:
                            WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffffzzz");
                            break;
                    }
                }
            }
        }
    }

    public void WriteDateTime(DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc)
        {
            WriteDateTimeOffset(new DateTimeOffset(value));
            return;
        }

        var totalMicrosecond = value.Millisecond * 1000 + value.Microsecond;
        if (totalMicrosecond == 0)
        {
            WriteDateTimeCore(value, "s");
        }
        else
        {
            if (value.Microsecond == 0)
            {
                var length = TomlCodes.Number.CountDigitsForInt64(value.Millisecond);
                switch (length)
                {
                    case 1:
                        WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fff");
                        break;
                    case 2:
                        if (value.Millisecond % 10 == 0)
                            WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ff");
                        else
                            WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fff");
                        break;
                    case 3:
                        if (value.Millisecond % 100 == 0)
                            WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.f");
                        else if (value.Millisecond % 10 == 0)
                            WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ff");
                        else
                            WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fff");
                        break;
                    default:
                        WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fff");
                        break;
                }
            }
            else
            {
                var length = TomlCodes.Number.CountDigitsForInt64(value.Microsecond);
                switch (length)
                {
                    case 1:
                        WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffff");
                        break;
                    case 2:
                        if (value.Microsecond % 10 == 0)
                            WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffff");
                        else
                            WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffff");
                        break;
                    case 3:
                        if (value.Microsecond % 100 == 0)
                            WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffff");
                        else if (value.Microsecond % 10 == 0)
                            WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffff");
                        else
                            WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffff");
                        break;
                    default:
                        WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffff");
                        break;
                }
            }
        }
    }

    public void WriteDateOnly(DateOnly value)
    {
        value.TryFormat(GetWrittenSpan(TomlCodes.DateTime.LocalDateFormatLength), out int bytesWritten, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public void WriteTimeOnly(TimeOnly value)
    {
        var totalMicrosecond = value.Millisecond * 1000 + value.Microsecond;
        if (totalMicrosecond == 0)
        {
            WriteTimeOnlyCore(value, "HH:mm:ss");
        }
        else
        {
            if (value.Microsecond == 0)
            {
                var length = TomlCodes.Number.CountDigitsForInt64(value.Millisecond);
                switch (length)
                {
                    case 1:
                        WriteTimeOnlyCore(value, "HH:mm:ss.fff");
                        break;
                    case 2:
                        if (value.Millisecond % 10 == 0)
                            WriteTimeOnlyCore(value, "HH:mm:ss.ff");
                        else
                            WriteTimeOnlyCore(value, "HH:mm:ss.fff");
                        break;
                    case 3:
                        if (value.Millisecond % 100 == 0)
                            WriteTimeOnlyCore(value, "HH:mm:ss.f");
                        else if (value.Millisecond % 10 == 0)
                            WriteTimeOnlyCore(value, "HH:mm:ss.ff");
                        else
                            WriteTimeOnlyCore(value, "HH:mm:ss.fff");
                        break;
                    default:
                        WriteTimeOnlyCore(value, "HH:mm:ss.fff");
                        break;
                }
            }
            else
            {
                var length = TomlCodes.Number.CountDigitsForInt64(value.Microsecond);
                switch (length)
                {
                    case 1:
                        WriteTimeOnlyCore(value, "HH:mm:ss.ffffff");
                        break;
                    case 2:
                        if (value.Microsecond % 10 == 0)
                            WriteTimeOnlyCore(value, "HH:mm:ss.fffff");
                        else
                            WriteTimeOnlyCore(value, "HH:mm:ss.ffffff");
                        break;
                    case 3:
                        if (value.Microsecond % 100 == 0)
                            WriteTimeOnlyCore(value, "HH:mm:ss.ffff");
                        else if (value.Microsecond % 10 == 0)
                            WriteTimeOnlyCore(value, "HH:mm:ss.fffff");
                        else
                            WriteTimeOnlyCore(value, "HH:mm:ss.ffffff");
                        break;
                    default:
                        WriteTimeOnlyCore(value, "HH:mm:ss.ffffff");
                        break;
                }
            }
        }
    }

    private void WriteOffsetDateTimeCore(DateTimeOffset value, ReadOnlySpan<char> format)
    {
        var length = 32;
        int bytesWritten;
        var span = GetSpan(0);
        while (!value.TryFormat(span, out bytesWritten, format, CultureInfo.InvariantCulture))
        {
            span = GetSpan(length);
            length *= 2;
        }

        Advance(bytesWritten);
    }

    private void WriteDateTimeCore(DateTime value, ReadOnlySpan<char> format)
    {
        var length = 32;
        int bytesWritten;
        var span = GetSpan(0);
        while (!value.TryFormat(span, out bytesWritten, format, CultureInfo.InvariantCulture))
        {
            span = GetSpan(length);
            length *= 2;
        }

        Advance(bytesWritten);
    }

    private void WriteTimeOnlyCore(TimeOnly value, ReadOnlySpan<char> format)
    {
        value.TryFormat(GetWrittenSpan(format.Length), out var bytesWritten, format, CultureInfo.InvariantCulture);
    }

    private void WriteDottedKeyPrefix()
    {
        var currentState = CurrentState;
        if (currentState.state == TomlValueState.Table) return;

        var index = (currentState.state == TomlValueState.ArrayOfTable || valueStates.Count > 1)
            ? currentState.dottedKeyIndex
            : 0;

        var keySpan = dottedKeys.Items;
        for (int i = index; i < keySpan.Length; i++)
        {
            keySpan[i].ToTomlString(ref this);
            Write(TomlCodes.Symbol.DOT);
        }
    }

    public void WriteKey(ReadOnlySpan<byte> key)
    {
        WriteDottedKeyPrefix();
        WriteStringInternal(key, TomlDottedKeyHelper.GetTomlKeyType(key, options.Spec.SupportsEscapeSequenceE, options.Spec.SupportsEscapeSequenceX));
    }

    public void WriteBareKey(ReadOnlySpan<byte> key)
    {
        WriteBareKey(key, false);
    }

    public void WriteBareKey(ReadOnlySpan<byte> key, bool skipValidation)
    {
        if (!skipValidation && Utf8Helper.ContainInvalidSequences(key))
            ExceptionHelper.ThrowInvalidCodePoints();

        WriteDottedKeyPrefix();
        WriteBytes(key);
    }

    internal void WriteKeyInternal(string key)
    {
        var keySpan = key.AsSpan();

        // buffer size to 3 times worst-case (UTF16 -> UTF8)
        var maxBufferSize = (keySpan.Length + 1) * 3;
        if (maxBufferSize <= 1024)
        {
            Span<byte> dest = stackalloc byte[maxBufferSize];
            Utf8Helper.FromUtf16(keySpan, dest, out var _, out var bytesWritten);

            if (bytesWritten > 0)
            {
                ref byte destReference = ref MemoryMarshal.GetReference(dest);
                unsafe
                {
                    fixed (byte* ptr = &destReference)
                    {
                        var writtenSpan = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(ptr), bytesWritten);
                        WriteStringInternal(writtenSpan, TomlDottedKeyHelper.GetTomlKeyType(writtenSpan, options.Spec.SupportsEscapeSequenceE, options.Spec.SupportsEscapeSequenceX));
                    }
                }
            }
        }
        else
        {
            var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
            try
            {
                Utf8Helper.FromUtf16(bufferWriter, keySpan);
                WriteStringInternal(bufferWriter.WrittenSpan, TomlDottedKeyHelper.GetTomlKeyType(bufferWriter.WrittenSpan, options.Spec.SupportsEscapeSequenceE, options.Spec.SupportsEscapeSequenceX));
            }
            finally
            {
                RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
            }
        }
    }


    internal void WriteKeyForPrimitive<T>(T value)
    {
#pragma warning disable CS8600
#pragma warning disable CS8605
        var valueType = value?.GetType() ?? typeof(T);
        var isObjectType = typeof(T) == typeof(object);
        if (PrimitiveObjectFormatter.TryGetJumpCode(valueType, out var jumpCode))
        {
            var refValue = value;
            switch (jumpCode)
            {
                case 0:
                    if (isObjectType)
                    {
                        WriteBoolean((bool)(object)refValue);
                    }
                    else
                    {
                        WriteBoolean(UnsafeHelper.BitCast<T, bool>(refValue));
                    }
                    break;
                case 1:
                    if (isObjectType)
                    {
                        WriteInt64((byte)(object)refValue);
                    }
                    else
                    {
                        WriteInt64(UnsafeHelper.BitCast<T, byte>(refValue));
                    }
                    break;
                case 2:
                    if (isObjectType)
                    {
                        WriteInt64((sbyte)(object)refValue);
                    }
                    else
                    {
                        WriteInt64(UnsafeHelper.BitCast<T, sbyte>(refValue));
                    }
                    break;
                case 3:
                    if (isObjectType)
                    {
                        WriteInt64((char)(object)refValue);
                    }
                    else
                    {
                        WriteInt64(UnsafeHelper.BitCast<T, char>(refValue));
                    }
                    break;
                case 4:
                    if (isObjectType)
                    {
                        WriteInt64((short)(object)refValue);
                    }
                    else
                    {
                        WriteInt64(UnsafeHelper.BitCast<T, short>(refValue));
                    }
                    break;
                case 5:
                    if (isObjectType)
                    {
                        WriteInt64((int)(object)refValue);
                    }
                    else
                    {
                        WriteInt64(UnsafeHelper.BitCast<T, int>(refValue));
                    }
                    break;
                case 6:
                    if (isObjectType)
                    {
                        WriteInt64((long)(object)refValue);
                    }
                    else
                    {
                        WriteInt64(UnsafeHelper.BitCast<T, long>(refValue));
                    }
                    break;
                case 7:
                    if (isObjectType)
                    {
                        WriteInt64((ushort)(object)refValue);
                    }
                    else
                    {
                        WriteInt64(UnsafeHelper.BitCast<T, ushort>(refValue));
                    }
                    break;
                case 8:
                    if (isObjectType)
                    {
                        WriteInt64((uint)(object)refValue);
                    }
                    else
                    {
                        WriteInt64(UnsafeHelper.BitCast<T, uint>(refValue));
                    }
                    break;
                case 9:
                    if (isObjectType)
                    {
                        WriteInt64(IntegerRangeHelper.ConvertToInt64((ulong)(object)refValue!));
                    }
                    else
                    {
                        WriteInt64(IntegerRangeHelper.ConvertToInt64(UnsafeHelper.BitCast<T, ulong>(refValue)));
                    }
                    break;
                case 10:
                    Write(TomlCodes.Symbol.DOUBLEQUOTED);
                    if (isObjectType)
                    {
                        WriteDouble((float)(object)refValue);
                    }
                    else
                    {
                        WriteDouble(UnsafeHelper.BitCast<T, float>(refValue));
                    }
                    Write(TomlCodes.Symbol.DOUBLEQUOTED);
                    break;
                case 11:
                    Write(TomlCodes.Symbol.DOUBLEQUOTED);
                    if (isObjectType)
                    {
                        WriteDouble((double)(object)refValue);
                    }
                    else
                    {
                        WriteDouble(UnsafeHelper.BitCast<T, double>(refValue));
                    }
                    Write(TomlCodes.Symbol.DOUBLEQUOTED);
                    break;
                case 12:
                    var strKey = refValue as string;

                    var writer = RecycleArrayPoolBufferWriter<byte>.Rent();
                    try
                    {
                        Utf8Helper.FromUtf16(writer, strKey);
                        var keyType = TomlDottedKeyHelper.GetTomlKeyType(writer.WrittenSpan, options.Spec.SupportsEscapeSequenceE, options.Spec.SupportsEscapeSequenceX);
                        WriteStringInternal(writer.WrittenSpan, keyType);
                    }
                    finally
                    {
                        RecycleArrayPoolBufferWriter<byte>.Return(writer);
                    }
                    break;
                case 13:
                    Write(TomlCodes.Symbol.DOUBLEQUOTED);
                    if (isObjectType)
                    {
                        WriteDateTime((DateTime)(object)refValue);
                    }
                    else
                    {
                        WriteDateTime(UnsafeHelper.BitCast<T, DateTime>(refValue));
                    }
                    Write(TomlCodes.Symbol.DOUBLEQUOTED);
                    break;
                case 14:
                    Write(TomlCodes.Symbol.DOUBLEQUOTED);
                    if (isObjectType)
                    {
                        WriteDateTimeOffset((DateTimeOffset)(object)refValue);
                    }
                    else
                    {
                        WriteDateTimeOffset(UnsafeHelper.BitCast<T, DateTimeOffset>(refValue));
                    }
                    Write(TomlCodes.Symbol.DOUBLEQUOTED);
                    break;
                case 15:
                    Write(TomlCodes.Symbol.DOUBLEQUOTED);
                    if (isObjectType)
                    {
                        WriteDateOnly((DateOnly)(object)refValue);
                    }
                    else
                    {
                        WriteDateOnly(UnsafeHelper.BitCast<T, DateOnly>(refValue));
                    }
                    Write(TomlCodes.Symbol.DOUBLEQUOTED);
                    break;
                case 16:
                    Write(TomlCodes.Symbol.DOUBLEQUOTED);
                    if (isObjectType)
                    {
                        WriteTimeOnly((TimeOnly)(object)refValue);
                    }
                    else
                    {
                        WriteTimeOnly(UnsafeHelper.BitCast<T, TimeOnly>(refValue));
                    }
                    Write(TomlCodes.Symbol.DOUBLEQUOTED);
                    break;
            }
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailedAsKey(valueType);
        }
#pragma warning restore CS8600
#pragma warning restore CS8605
    }

    public void WriteTableHeader(ReadOnlySpan<byte> key)
    {
        BeginTableHeader();
        var keySpan = dottedKeys.Items;
        for (int i = 0; i < keySpan.Length; i++)
        {
            keySpan[i].ToTomlString(ref this);
            Write(TomlCodes.Symbol.DOT);
        }

        WriteStringInternal(key, TomlDottedKeyHelper.GetTomlKeyType(key, options.Spec.SupportsEscapeSequenceE, options.Spec.SupportsEscapeSequenceX));

        EndTableHeader();
    }

    public void WriteBareTableHeader(ReadOnlySpan<byte> key, bool skipValidation = false)
    {
        if (!skipValidation && Utf8Helper.ContainInvalidSequences(key))
            ExceptionHelper.ThrowInvalidCodePoints();

        BeginTableHeader();
        var keySpan = dottedKeys.Items;
        for (int i = 0; i < keySpan.Length; i++)
        {
            keySpan[i].ToTomlString(ref this);
            Write(TomlCodes.Symbol.DOT);
        }

        WriteStringInternal(key, TomlStringType.Unquoted);

        EndTableHeader();
    }

    internal void WriteTableHeaderForPrimitive<T>(T value)
    {
        BeginTableHeader();
        var keySpan = dottedKeys.Items;
        for (int i = 0; i < keySpan.Length; i++)
        {
            keySpan[i].ToTomlString(ref this);
            Write(TomlCodes.Symbol.DOT);
        }
        WriteKeyForPrimitive(value);
        EndTableHeader();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0) return;

        var pos = position;
        if ((uint)bytes.Length > (uint)(buffer.Length - pos))
        {
            Grow(bytes.Length);
            pos = position;
        }

        bytes.CopyTo(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), (nint)(uint)pos), bytes.Length));
        position = pos + bytes.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNewLine()
        => WriteBytes(TomlCodes.Environment.NewLine);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSpace()
        => Write(TomlCodes.Symbol.SPACE);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteEqual()
        => WriteBytes(" = "u8);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteComma()
        => WriteBytes(", "u8);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Write(byte value)
    {
        var pos = position;
        if ((uint)pos >= (uint)buffer.Length)
        {
            Grow(1);
            pos = position;
        }

        Unsafe.Add(ref MemoryMarshal.GetReference(buffer), (nint)(uint)pos) = value;
        position = pos + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BeginArray()
    {
        if (this.CurrentState.state != TomlValueState.ArrayOfTable)
        {
            valueStates.Add((TomlValueState.Array, dottedKeys.Count));
        }
        Write(TomlCodes.Symbol.LEFTSQUAREBRACKET);
        WriteSpace();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndArray()
    {
        if (this.CurrentState.state == TomlValueState.Array)
        {
            valueStates.RemoveLast();
        }
        Write(TomlCodes.Symbol.RIGHTSQUAREBRACKET);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BeginTableHeader()
        => Write(TomlCodes.Symbol.LEFTSQUAREBRACKET);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndTableHeader()
        => Write(TomlCodes.Symbol.RIGHTSQUAREBRACKET);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BeginArrayOfTablesHeader()
        => WriteBytes("[["u8);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndArrayOfTablesHeader()
        => WriteBytes("]]"u8);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BeginInlineTable()
        => Write(TomlCodes.Symbol.LEFTBRACES);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndInlineTable()
        => Write(TomlCodes.Symbol.RIGHTBRACES);
}

