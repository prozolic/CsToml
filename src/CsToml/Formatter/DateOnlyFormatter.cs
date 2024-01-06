using CsToml.Error;
using CsToml.Utility;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsToml.Formatter;

internal class DateOnlyFormatter : ICsTomlFormatter<DateOnly>
{
    public static void Serialize(ref Utf8Writer writer, DateOnly value)
    {
        var format = "yyyy-MM-dd";
        value.TryFormat(writer.GetWriteSpan(CsTomlSyntax.DateTime.LocalDateFormat.Length), out int bytesWritten, format);
    }

    public static DateOnly Deserialize(ref Utf8Reader reader, int length)
    {
        var bytes = reader.ReadBytes(length);

        if (bytes.Length != CsTomlSyntax.DateTime.LocalDateFormat.Length)
            ExceptionHelper.ThrowIncorrectTomlFormat();

        // local date
        if (!(CsTomlSyntax.IsHyphen(bytes[4]) && CsTomlSyntax.IsHyphen(bytes[7])))
        {
            ExceptionHelper.ThrowIncorrectTomlFormat();
        }

        return DeserializeDateOnly(bytes);
    }

    internal static DateOnly DeserializeDateOnly(ReadOnlySpan<byte> bytes)
    {
        var year = CsTomlSyntax.Number.ParseDecimal(bytes[0]) * 1000;
        year += CsTomlSyntax.Number.ParseDecimal(bytes[1]) * 100;
        year += CsTomlSyntax.Number.ParseDecimal(bytes[2]) * 10;
        year += CsTomlSyntax.Number.ParseDecimal(bytes[3]);

        var month = CsTomlSyntax.Number.ParseDecimal(bytes[5]) * 10;
        month += CsTomlSyntax.Number.ParseDecimal(bytes[6]);

        var day = CsTomlSyntax.Number.ParseDecimal(bytes[8]) * 10;
        day += CsTomlSyntax.Number.ParseDecimal(bytes[9]);

        return new DateOnly(year, month, day);
    }

}

