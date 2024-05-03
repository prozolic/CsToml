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

    public readonly int WrittingCount => writer.WrittingCount;

    [DebuggerStepThrough]
    public CsTomlWriter(ref Utf8Writer<TBufferWriter> bufferWriter)
    {
        writer = bufferWriter;
        newLineCh = CsTomlSyntax.Environment.NewLine;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNewLine()
        => writer.Write(newLineCh);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteComma()
        => writer.Write(CsTomlSyntax.Symbol.COMMA);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSpace()
        => writer.Write(CsTomlSyntax.Symbol.SPACE);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteTable()
        => writer.Write(CsTomlSyntax.Symbol.LEFTSQUAREBRACKET);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteTableEnd()
        => writer.Write(CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET);

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
    public void WriteEquals()
    {
        var span = writer.GetWriteSpan(3);
        span[2] = CsTomlSyntax.Symbol.SPACE;
        span[1] = CsTomlSyntax.Symbol.EQUAL;
        span[0] = CsTomlSyntax.Symbol.SPACE;
    }

    public void WriteKeyValue(in CsTomlString key, CsTomlValue value)
    {
        WriterKey(in key, false);
        WriteEquals();
        value.ToTomlString(ref writer);
    }

    public void WriteKeyValueAndNewLine(in CsTomlString key, CsTomlValue value)
    {
        WriterKey(in key, false);
        WriteEquals();
        value.ToTomlString(ref writer);
        WriteNewLine();
    }

    public void WriterKey(in CsTomlString key, bool isGroupingProperty)
    {
        key.ToTomlString(ref writer);
        if (isGroupingProperty)
        {
            writer.Write(CsTomlSyntax.Symbol.PERIOD);
        }
    }

    public void WriteTableHeader(ReadOnlySpan<CsTomlString> keysSpan)
    {
        WriteTable();
        if (keysSpan.Length > 0)
        {
            for (var i = 0; i < keysSpan.Length; i++)
            {
                WriterKey(in keysSpan[i], i < keysSpan.Length - 1);
            }
        }
        WriteTableEnd();
        WriteNewLine();
    }

    public void WriteArrayOfTablesHeader(ReadOnlySpan<CsTomlString> keysSpan)
    {
        WriteArrayOfTables();
        if (keysSpan.Length > 0)
        {
            for (var i = 0; i < keysSpan.Length; i++)
            {
                WriterKey(in keysSpan[i], i < keysSpan.Length - 1);
            }
        }
        WriteArrayOfTablesEnd();
        WriteNewLine();
    }

    public void WriteComments(ReadOnlySpan<CsTomlString> comments)
    {
        if (comments.Length == 0) return;

        for (var i = 0; i < comments.Length; i++)
        {
            writer.Write(CsTomlSyntax.Symbol.NUMBERSIGN);
            comments[i].ToTomlString(ref writer);
            WriteNewLine();
        }
    }
}
