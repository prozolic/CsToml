
using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;
using CsToml.Values;

namespace CsToml;

public partial class CsTomlPackage
{
    public bool TryGetTomlValue(ReadOnlySpan<byte> key, out CsTomlValue? value)
        => table.TryGetValue(key, out value);

    public bool TryGetTomlValue(string key, out CsTomlValue? value)
    {
        using var writer = new ArrayPoolBufferWriter<byte>(128);
        var utf8Writer = new Utf8Writer(writer);
        try
        {
            StringFormatter.Serialize(ref utf8Writer, key);
        }
        catch (CsTomlException)
        {
            // TODO;
            value = null;
            return false;
        }
        return TryGetTomlValue(writer.WrittenSpan, out value);
    }



}

