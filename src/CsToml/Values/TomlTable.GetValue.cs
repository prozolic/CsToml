
namespace CsToml.Values;

internal partial class TomlTable
{
    public override object GetObject()
        => GetDictionary();
}
