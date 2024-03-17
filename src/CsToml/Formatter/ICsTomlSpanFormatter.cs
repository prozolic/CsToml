using CsToml.Utility;

namespace CsToml.Formatter;

internal interface ICsTomlSpanFormatter<T>
    where T : struct
{
    static abstract void Serialize(ref Utf8Writer writer, ReadOnlySpan<T> value);
}

