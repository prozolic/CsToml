
namespace CsToml.Values;

internal partial class TomlInteger
{
    public override bool TryGetInt64(out long value)
    {
        value = Value;
        return true;
    }

    public override bool TryGetDouble(out double value)
    {
        value = Value;
        return true;
    }

    public override object GetObject() => Value;
}
