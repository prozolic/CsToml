
namespace CsToml.Values;

internal partial class TomlLocalDate
{
    public override bool TryGetDateOnly(out DateOnly value)
    {
        value = Value;
        return true;
    }

    public override object GetObject() => Value;
}
