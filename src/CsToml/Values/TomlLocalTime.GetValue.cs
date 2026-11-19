
namespace CsToml.Values;

internal partial class TomlLocalTime
{
    public override bool TryGetTimeOnly(out TimeOnly value)
    {
        value = Value;
        return true;
    }

    public override object GetObject() => Value;
}
