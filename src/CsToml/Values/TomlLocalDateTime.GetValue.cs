
namespace CsToml.Values;

internal partial class TomlLocalDateTime
{
    public override bool TryGetDateTime(out DateTime value)
    {
        value = Value;
        return true;
    }

    public override object GetObject() => Value;
}
