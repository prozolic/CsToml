using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
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
}

public ref struct Utf8TomlDocumentWriter<TBufferWriter>
    where TBufferWriter : IBufferWriter<byte>
{
    private Utf8Writer<TBufferWriter> writer;
    private List<TomlDottedKey> dottedKeys;
    private List<(TomlValueState state, int dottedKeyIndex)> valueStates;
    private readonly bool valueOnly;

    internal readonly int WrittenSize => writer.WrittenSize;

    internal readonly (TomlValueState state, int dottedKeyIndex) CurrentPreviousState => valueStates[^2];

    internal readonly (TomlValueState state, int dottedKeyIndex) CurrentState => valueStates[^1];

    public readonly TomlValueState State => CurrentState.state;

    internal readonly bool IsRoot => valueStates.Count == 1 && !valueOnly;

    public Utf8TomlDocumentWriter(ref TBufferWriter bufferWriter, bool valueOnly = false)
    {
        writer = new Utf8Writer<TBufferWriter>(ref bufferWriter);
        dottedKeys = new List<TomlDottedKey>();
        valueStates = [(TomlValueState.Default, -1)];
        this.valueOnly = valueOnly;
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
        dottedKeys.Add(TomlDottedKey.ParseKey(key));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void PushKeyForPrimitive<T>(T value)
    {
        dottedKeys.Add(TomlDottedKey.ParseKeyForPrimitive(value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PopKey()
    {
        dottedKeys.RemoveAt(dottedKeys.Count - 1);
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
                    if (valueStates.Count > 1)
                    {
                        if (valueStates.Any(v => v.state == TomlValueState.ArrayOfTable))
                            return;
                    }
                    WriteNewLine();
                    return;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BeginCurrentState(TomlValueState state)
    {
        valueStates.Add((state, dottedKeys.Count));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndCurrentState()
    {
        valueStates.RemoveAt(valueStates.Count - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsArrayOrListElement()
    {
        return valueStates.Any(v => v.state == TomlValueState.ArrayOfTable);
    }

    public void WriteBoolean(bool value)
    {
        if (value)
        {
            writer.Write(TomlCodes.Alphabet.t);
            writer.Write(TomlCodes.Alphabet.r);
            writer.Write(TomlCodes.Alphabet.u);
            writer.Write(TomlCodes.Alphabet.e);
        }
        else
        {
            writer.Write(TomlCodes.Alphabet.f);
            writer.Write(TomlCodes.Alphabet.a);
            writer.Write(TomlCodes.Alphabet.l);
            writer.Write(TomlCodes.Alphabet.s);
            writer.Write(TomlCodes.Alphabet.e);
        }
    }

    public void WriteInt64(long value)
    {
        var length = TomlCodes.Number.DigitsDecimalUnroll4(value);
        if (value < 0) length++;

        value.TryFormat(writer.GetWrittenSpan(length), out int bytesWritten, null, CultureInfo.InvariantCulture);
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
        var writtenSpan = writer.GetWrittenSpan(digits);
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
        var writtenSpan = writer.GetWrittenSpan(digits);
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
        var writtenSpan = writer.GetWrittenSpan(digits);
        var index = digits - 1;

        while (value > 0)
        {
            var v = value & 15;
            switch (v)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    writtenSpan[index--] = (byte)(TomlCodes.Number.Zero + (byte)v);
                    break;
                case 10: // A
                case 11: // B
                case 12: // C
                case 13: // D
                case 14: // E
                case 15: // F
                    writtenSpan[index--] = (byte)(TomlCodes.Alphabet.A + (byte)v - 10);
                    break;
            }
            value >>= 4;
        }
    }

    public void WriteDouble(double value)
    {
        switch(value)
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

                var writtenSpan = writer.GetSpan(length);
                while (!value.TryFormat(writtenSpan, out bytesWritten, "G", CultureInfo.InvariantCulture))
                {
                    length *= 2;
                    writtenSpan = writer.GetSpan(length);
                }
                writer.Advance(bytesWritten);

                // integer check
                if (!writtenSpan.Slice(0, bytesWritten).ContainsAny(".eE"u8))
                {
                    var writtenSpanEx = writer.GetWrittenSpan(2);
                    writtenSpanEx[0] = TomlCodes.Symbol.DOT;
                    writtenSpanEx[1] = TomlCodes.Number.Zero;
                }
                return;
        }
    }

    public void WriteString(string? value)
    {
        TomlStringHelper.Parse(value).ToTomlString(ref this);
    }

    public void WriteString(scoped ReadOnlySpan<byte> value)
    {
        TomlStringHelper.Parse(value).ToTomlString(ref this);
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
                    var length = TomlCodes.Number.DigitsDecimalUnroll4(value.Millisecond);
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
                    var length = TomlCodes.Number.DigitsDecimalUnroll4(value.Microsecond);
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
                    var length = TomlCodes.Number.DigitsDecimalUnroll4(value.Millisecond);
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
                    var length = TomlCodes.Number.DigitsDecimalUnroll4(value.Microsecond);
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
        var totalMicrosecond = value.Millisecond * 1000 + value.Microsecond;
        if (totalMicrosecond == 0)
        {
            WriteDateTimeCore(value, "s");
        }
        else
        {
            if (value.Microsecond == 0)
            {
                var length = TomlCodes.Number.DigitsDecimalUnroll4(value.Millisecond);
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
                var length = TomlCodes.Number.DigitsDecimalUnroll4(value.Microsecond);
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
        value.TryFormat(writer.GetWrittenSpan(TomlCodes.DateTime.LocalDateFormatLength), out int bytesWritten, "yyyy-MM-dd");
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
                var length = TomlCodes.Number.DigitsDecimalUnroll4(value.Millisecond);
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
                var length = TomlCodes.Number.DigitsDecimalUnroll4(value.Microsecond);
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
        Span<byte> buffer = writer.GetSpan(length);
        while (!value.TryFormat(buffer, out bytesWritten, format))
        {
            length *= 2;
            buffer = writer.GetSpan(length);
        }

        writer.Advance(bytesWritten);
    }

    private void WriteDateTimeCore(DateTime value, ReadOnlySpan<char> format)
    {
        var length = 32;
        int bytesWritten;
        while (!value.TryFormat(writer.GetSpan(length), out bytesWritten, format))
        {
            length *= 2;
        }

        writer.Advance(bytesWritten);
    }

    private void WriteTimeOnlyCore(TimeOnly value, ReadOnlySpan<char> format)
    {
        value.TryFormat(writer.GetWrittenSpan(format.Length), out var bytesWritten, format);
    }

    public void WriteKey(ReadOnlySpan<byte> key)
    {
        var currentState = CurrentState;
        var index = 0;
        if (valueStates.Count > 0 && currentState.state == TomlValueState.ArrayOfTable)
        {
            index = currentState.dottedKeyIndex;
        }

        if (currentState.state != TomlValueState.Table)
        {
            if (valueStates.Count > 1)
            {
                index = currentState.dottedKeyIndex;
                var keySpan = CollectionsMarshal.AsSpan(dottedKeys);
                for (int i = index; i < keySpan.Length; i++)
                {
                    keySpan[i].ToTomlString(ref this);
                    writer.Write(TomlCodes.Symbol.DOT);
                }

            }
            else
            {
                var keySpan = CollectionsMarshal.AsSpan(dottedKeys);
                for (int i = index; i < keySpan.Length; i++)
                {
                    keySpan[i].ToTomlString(ref this);
                    writer.Write(TomlCodes.Symbol.DOT);
                }
            }
        }

        TomlDottedKey.ParseKey(key).ToTomlString(ref this);
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
                        WriteInt64(checked((long)(object)refValue));
                    }
                    else
                    {
                        WriteInt64(UnsafeHelper.BitCast<T, long>(refValue));
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
                    TomlDottedKey.ParseKey(strKey.AsSpan()).ToTomlString(ref this);
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
        var keySpan = CollectionsMarshal.AsSpan(dottedKeys);
        for (int i = 0; i < keySpan.Length; i++)
        {
            keySpan[i].ToTomlString(ref this);
            writer.Write(TomlCodes.Symbol.DOT);
        }
        TomlDottedKey.ParseKey(key).ToTomlString(ref this);
        EndTableHeader();
    }

    internal void WriteTableHeaderForPrimitive<T>(T value)
    {
        BeginTableHeader();
        var keySpan = CollectionsMarshal.AsSpan(dottedKeys);
        for (int i = 0; i < keySpan.Length; i++)
        {
            keySpan[i].ToTomlString(ref this);
            writer.Write(TomlCodes.Symbol.DOT);
        }
        WriteKeyForPrimitive(value);
        EndTableHeader();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBytes(ReadOnlySpan<byte> bytes)
        => writer.Write(bytes);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNewLine()
        => WriteBytes(TomlCodes.Environment.NewLine);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSpace()
        => writer.Write(TomlCodes.Symbol.SPACE);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteEqual()
        => WriteBytes(" = "u8);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteComma()
        => writer.Write(", "u8);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Write(byte @byte)
        => writer.Write(@byte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BeginArray()
    {
        if (this.CurrentState.state != TomlValueState.ArrayOfTable)
        {
            valueStates.Add((TomlValueState.Array, dottedKeys.Count));
        }
        writer.Write(TomlCodes.Symbol.LEFTSQUAREBRACKET);
        WriteSpace();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndArray()
    {
        if (this.CurrentState.state == TomlValueState.Array)
        {
            valueStates.RemoveAt(valueStates.Count - 1);
        }
        writer.Write(TomlCodes.Symbol.RIGHTSQUAREBRACKET);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BeginTableHeader()
        => writer.Write(TomlCodes.Symbol.LEFTSQUAREBRACKET);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndTableHeader()
        => writer.Write(TomlCodes.Symbol.RIGHTSQUAREBRACKET);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BeginArrayOfTablesHeader()
    {
        writer.Write(TomlCodes.Symbol.LEFTSQUAREBRACKET);
        writer.Write(TomlCodes.Symbol.LEFTSQUAREBRACKET);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndArrayOfTablesHeader()
    {
        writer.Write(TomlCodes.Symbol.RIGHTSQUAREBRACKET);
        writer.Write(TomlCodes.Symbol.RIGHTSQUAREBRACKET);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BeginInlineTable()
        => writer.Write(TomlCodes.Symbol.LEFTBRACES);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndInlineTable()
        => writer.Write(TomlCodes.Symbol.RIGHTBRACES);
}

