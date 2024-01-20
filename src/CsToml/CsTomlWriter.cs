using CsToml.Formatter;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CsToml;

internal ref struct CsTomlWriter
{
    private Utf8Writer writer;
    private readonly ReadOnlySpan<byte> newLineCh;

    private ReadOnlySpan<byte> currentKeySpan = [];

    [DebuggerStepThrough]
    public CsTomlWriter(ref Utf8Writer bufferWriter)
    {
        writer = bufferWriter;
        newLineCh = OperatingSystem.IsWindows() ? CsTomlSyntax.Symbol.WindowsNewLine : CsTomlSyntax.Symbol.UnixNewLine;
    }

    public void WriteNewLine()
    {
        writer.Write(newLineCh);
    }

    public void WriteKeyValue(in Utf8FixString key, CsTomlValue value)
    {
        WriterKey(in key, false);
        WriteEquals();
        WriterValue(in key, value);
    }

    public void WriterKey(in Utf8FixString key, bool isGroupingProperty)
    {
        if (isGroupingProperty)
        {
            writer.Write(key.BytesSpan);
            writer.Write(CsTomlSyntax.Symbol.PERIOD);
        }
        else
        {
            writer.Write(key.BytesSpan);
        }
    }

    public void WriteEquals()
    {
        writer.Write(" = "u8);
    }

    public void WriterValue(in Utf8FixString key, CsTomlValue value)
    {
        value.ToTomlString(ref writer);
        writer.Write(newLineCh);
    }

    public void WriteTableHeader(Span<Utf8FixString> keysSpan)
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
