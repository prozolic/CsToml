using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml;

internal ref struct CsTomlWriter<TBufferWriter>
    where TBufferWriter : IBufferWriter<byte>
{
    private Utf8Writer<TBufferWriter> writer;
    private readonly ReadOnlySpan<byte> newLineCh;

    public readonly int WrittingCount => writer.WrittenSize;

    [DebuggerStepThrough]
    public CsTomlWriter(ref Utf8Writer<TBufferWriter> bufferWriter)
    {
        writer = bufferWriter;
        newLineCh = TomlCodes.Environment.NewLine;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNewLine()
        => writer.Write(newLineCh);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteComma()
        => writer.Write(TomlCodes.Symbol.COMMA);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSpace()
        => writer.Write(TomlCodes.Symbol.SPACE);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteTable()
        => writer.Write(TomlCodes.Symbol.LEFTSQUAREBRACKET);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteTableEnd()
        => writer.Write(TomlCodes.Symbol.RIGHTSQUAREBRACKET);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteArrayOfTables()
    {
        WriteTable();
        WriteTable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteArrayOfTablesEnd()
    {
        WriteTableEnd();
        WriteTableEnd();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInlineTable()
        => writer.Write(TomlCodes.Symbol.LEFTBRACES);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInlineTableEnd()
        => writer.Write(TomlCodes.Symbol.RIGHTBRACES);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteEquals()
    {
        var span = writer.GetWrittenSpan(3);
        span[2] = TomlCodes.Symbol.SPACE;
        span[1] = TomlCodes.Symbol.EQUAL;
        span[0] = TomlCodes.Symbol.SPACE;
    }

    public void WriteKeyValue(TomlDotKey key, TomlValue value)
    {
        WriterKey(key, false);
        WriteEquals();
        value.ToTomlString(ref writer);
    }

    public void WriteKeyValueAndNewLine(TomlDotKey key, TomlValue value)
    {
        WriterKey(key, false);
        WriteEquals();
        value.ToTomlString(ref writer);
        WriteNewLine();
    }

    public void WriterKey(TomlDotKey key, bool isGroupingProperty)
    {
        key.ToTomlString(ref writer);
        if (isGroupingProperty)
        {
            writer.Write(TomlCodes.Symbol.DOT);
        }
    }

    public void WriteTableHeader(ReadOnlySpan<TomlDotKey> keysSpan)
    {
        WriteTable();
        if (keysSpan.Length > 0)
        {
            for (var i = 0; i < keysSpan.Length; i++)
            {
                WriterKey(keysSpan[i], i < keysSpan.Length - 1);
            }
        }
        WriteTableEnd();
        WriteNewLine();
    }

    public void WriteArrayOfTablesHeader(ReadOnlySpan<TomlDotKey> keysSpan)
    {
        WriteArrayOfTables();
        if (keysSpan.Length > 0)
        {
            for (var i = 0; i < keysSpan.Length; i++)
            {
                WriterKey(keysSpan[i], i < keysSpan.Length - 1);
            }
        }
        WriteArrayOfTablesEnd();
        WriteNewLine();
    }

    public void WriteComments(ReadOnlySpan<TomlString> comments)
    {
        if (comments.Length == 0) return;

        for (var i = 0; i < comments.Length; i++)
        {
            writer.Write(TomlCodes.Symbol.NUMBERSIGN);
            comments[i].ToTomlString(ref writer);
            WriteNewLine();
        }
    }
}
