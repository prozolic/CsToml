using CsToml.Formatter;
using CsToml.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsToml.Values;

internal partial class CsTomlArray //: ISpanFormattable
{
    public override string GetString()
    {
        using var writer = new ArrayPoolBufferWriter<byte>(128);
        var utf8Writer = new Utf8Writer(writer);
        ToTomlString(ref utf8Writer);

        var tempReader = new Utf8Reader(writer.WrittenSpan);
        return StringFormatter.Deserialize(ref tempReader, tempReader.Length);
    }


    //public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    //{
    //}

    //public string ToString(string? format, IFormatProvider? formatProvider)
    //{
    //}
}

