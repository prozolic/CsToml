
using CsToml.Utility;

namespace CsToml.Formatter;

internal interface ICsTomlFormatter<T>
{
    public static abstract void Serialize(ref Utf8Writer writer, T value);

    public static abstract T Deserialize(ref Utf8Reader reader, int length);
}

