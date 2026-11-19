
namespace CsToml.Values;

internal partial class TomlBoolean
{
    public override bool TryGetBool(out bool value)
    {
        value = Value;
        return true;
    }

    public override object GetObject() => Value;
}
