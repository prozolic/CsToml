
namespace CsToml.Values;

internal partial class TomlOffsetDateTime
{
    public override bool TryGetDateTimeOffset(out DateTimeOffset value)
    {
        value = Value;
        return true;
    }

    public override object GetObject() => Value;
}
