
using CsToml.Utility;
using CsToml.Values;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml;

internal ref struct CsTomlWriter
{
    private Utf8Writer writer;
    private readonly ReadOnlySpan<byte> newLineCh;

    [DebuggerStepThrough]
    public CsTomlWriter(ref Utf8Writer bufferWriter)
    {
        writer = bufferWriter;
        newLineCh = OperatingSystem.IsWindows() ? CsTomlSyntax.Symbol.WindowsNewLine : CsTomlSyntax.Symbol.UnixNewLine;
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
        if (isGroupingProperty)
        {
            writer.Write(key.Value);
            writer.Write(CsTomlSyntax.Symbol.PERIOD);
        }
        else
        {
            writer.Write(key.Value);
        }
    }

    public void WriteEquals()
    {
        var span = writer.GetWriteSpan(3);
        span[2] = CsTomlSyntax.Symbol.SPACE;
        span[1] = CsTomlSyntax.Symbol.EQUAL;
        span[0] = CsTomlSyntax.Symbol.SPACE;
    }

    public void WriteTableHeader(Span<CsTomlString> keysSpan)
    {
        writer.Write(CsTomlSyntax.Symbol.LEFTSQUAREBRACKET);
        if (keysSpan.Length > 0)
        {
            for (var i = 0; i < keysSpan.Length; i++)
            {
                WriterKey(in keysSpan[i], i < keysSpan.Length - 1);
            }
        }
        writer.Write(CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET);
        WriteNewLine();
    }
}
