namespace CsToml.Values;

internal partial class TomlFloat
{
    public override bool TryGetDouble(out double value)
    {
        value = Value;
        return true;
    }

    public override object GetObject() => Value;
}
