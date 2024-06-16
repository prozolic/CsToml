using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsToml.Extension;

internal static class ByteSpanExtensions
{
    public static ReadOnlySpan<byte> TrimWhiteSpace(this ReadOnlySpan<byte> bytes)
        => bytes.Trim(CsTomlSyntax.Symbol.SPACE);
}

