using CsToml.Formatter;
using CsToml.Utility;
using CsToml.Values;
using System.Diagnostics;

namespace CsToml;

internal ref struct CsTomlWriter
{
    private Utf8Writer writer;

    [DebuggerStepThrough]
    public CsTomlWriter(ref Utf8Writer bufferWriter)
    {
        writer = bufferWriter;
    }

    public void WriteKeyValue(Utf8FixString key, CsTomlValue value)
    {
        writer.Write(key.BytesSpan);
        writer.Write(" = "u8);

        value.ToTomlString(ref writer);
        writer.Write("\n"u8);
    }

}
