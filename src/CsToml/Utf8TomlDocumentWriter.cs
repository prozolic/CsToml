using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml;

public enum TomlValueState
{
    Default,
    Array,
    ArrayOfTable
}

public ref struct Utf8TomlDocumentWriter<TBufferWriter>
    where TBufferWriter : IBufferWriter<byte>
{
    private Utf8Writer<TBufferWriter> writer;
    private List<TomlDottedKey> dottedKeys;
    private List<(TomlValueState state, int dottedKeyIndex)> valueStates;

    internal int WrittenSize => writer.WrittenSize;

    internal (TomlValueState state, int dottedKeyIndex) CurrentState => valueStates[^1];

    public Utf8TomlDocumentWriter(ref TBufferWriter bufferWriter)
    {
        writer = new Utf8Writer<TBufferWriter>(ref bufferWriter);
        dottedKeys = new List<TomlDottedKey>();
        valueStates = [(TomlValueState.Default, 0)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushKey(ReadOnlySpan<byte> key)
    {
        dottedKeys.Add(TomlDottedKey.ParseKey(key));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PopKey()
    {
        dottedKeys.RemoveAt(dottedKeys.Count - 1);
    }

    public void EndKeyValue()
    {
        if (valueStates.Count > 0)
        {
            var state = CurrentState.state;
            switch (state)
            {
                case TomlValueState.Array:
                case TomlValueState.ArrayOfTable:
                    break;
                default:
                    WriteNewLine();
                    return;
            }

            if (state == TomlValueState.ArrayOfTable)
            {
                EndInlineTable();
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

    public void WriteDouble(double value)
    {
        var length = 32;
        int bytesWritten;
        while (!value.TryFormat(writer.GetSpan(length), out bytesWritten, "G", CultureInfo.InvariantCulture))
        {
            length *= 2;
        }
        writer.Advance(bytesWritten);
    }

    public void WriteString(string? value)
    {
        TomlString.Parse(value).ToTomlString(ref this);
    }

    public void WriteString(scoped ReadOnlySpan<byte> value)
    {
        TomlString.Parse(value).ToTomlString(ref this);
    }

    public void WriteDateTimeOffset(DateTimeOffset value)
    {
        var totalMicrosecond = value.Millisecond * 1000 + value.Microsecond;
        var totalMinutes = value.Offset.TotalMinutes; // check timezone

        if (totalMicrosecond == 0 && totalMinutes == 0)
        {
            WriteOffsetDateTimeCore(value, "u");
        }
        else if (totalMicrosecond > 0 && totalMinutes != 0)
        {
            var length = TomlCodes.Number.DigitsDecimalUnroll4(totalMicrosecond);

            switch (length)
            {
                case 1:
                    WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fzzz");
                    break;
                case 2:
                    WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffzzz");
                    break;
                case 3:
                    WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffzzz");
                    break;
                case 4:
                    WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffzzz");
                    break;
                case 5:
                    WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffffzzz");
                    break;
                case 6:
                    WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffffzzz");
                    break;
                default:
                    WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffffzzz");
                    break;
            }
        }
        else if (totalMicrosecond > 0)
        {
            var length = TomlCodes.Number.DigitsDecimalUnroll4(totalMicrosecond);

            switch (length)
            {
                case 1:
                    WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.f");
                    break;
                case 2:
                    WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ff");
                    break;
                case 3:
                    WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fff");
                    break;
                case 4:
                    WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffff");
                    break;
                case 5:
                    WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffff");
                    break;
                case 6:
                    WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffff");
                    break;
                default:
                    WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffff");
                    break;
            }
        }
        else if (totalMinutes != 0)
        {
            WriteOffsetDateTimeCore(value, "yyyy-MM-ddTHH:mm:sszzz");
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
            var length = TomlCodes.Number.DigitsDecimalUnroll4(totalMicrosecond);

            switch (length)
            {
                case 1:
                    WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.f");
                    break;
                case 2:
                    WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ff");
                    break;
                case 3:
                    WriteDateTimeCore( value, "yyyy-MM-ddTHH:mm:ss.fff");
                    break;
                case 4:
                    WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffff");
                    break;
                case 5:
                    WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.fffff");
                    break;
                case 6:
                    WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffff");
                    break;
                default:
                    WriteDateTimeCore(value, "yyyy-MM-ddTHH:mm:ss.ffffff");
                    break;

            }
        }
    }

    public void WriteDateOnly(DateOnly value)
    {
        value.TryFormat(writer.GetWrittenSpan(TomlCodes.DateTime.LocalDateFormatLength), out int bytesWritten, "yyyy-MM-dd");
    }

    public void WriteTimeOnly(TimeOnly value)
    {
        var totalMicrosecond = value.Millisecond * 100 + value.Microsecond;
        if (totalMicrosecond == 0)
        {
            WriteTimeOnlyCore(value, "HH:mm:ss");
        }
        else
        {
            var length = TomlCodes.Number.DigitsDecimalUnroll4(totalMicrosecond);

            switch (length)
            {
                case 1:
                    WriteTimeOnlyCore(value, "HH:mm:ss.f");
                    break;
                case 2:
                    WriteTimeOnlyCore(value, "HH:mm:ss.ff");
                    break;
                case 3:
                    WriteTimeOnlyCore(value, "HH:mm:ss.fff");
                    break;
                case 4:
                    WriteTimeOnlyCore(value, "HH:mm:ss.ffff");
                    break;
                case 5:
                    WriteTimeOnlyCore(value, "HH:mm:ss.fffff");
                    break;
                case 6:
                    WriteTimeOnlyCore(value, "HH:mm:ss.ffffff");
                    break;
                default:
                    WriteTimeOnlyCore(value, "HH:mm:ss.ffffff");
                    break;

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

        // ex 1979-05-27 07:32:00Z -> 1979-05-27T07:32:00Z
        if (value.Offset == TimeSpan.Zero)
        {
            buffer[10] = TomlCodes.Alphabet.T;
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
            writer.Write(TomlCodes.Symbol.LEFTBRACES);
            index = currentState.dottedKeyIndex;
        }

        var keySpan = CollectionsMarshal.AsSpan(dottedKeys);
        for (int i = index; i < keySpan.Length; i++)
        {
            keySpan[i].ToTomlString(ref this);
            writer.Write(TomlCodes.Symbol.DOT);
        }
        TomlDottedKey.ParseKey(key).ToTomlString(ref this);
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

    public void WriteEqual()
        => WriteBytes(" = "u8);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Write(byte @byte)
        => writer.Write(@byte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BeginArray()
    {
        writer.Write(TomlCodes.Symbol.LEFTSQUAREBRACKET);
        WriteSpace();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndArray()
        => writer.Write(TomlCodes.Symbol.RIGHTSQUAREBRACKET);

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

