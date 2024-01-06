using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Debugger;

internal sealed class Utf8FixStringDebugView(Utf8FixString @string)
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Utf8FixString fixString = @string;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public Span<byte> Bytes => fixString.BytesSpan;
}

