using System.Diagnostics.CodeAnalysis;

namespace CsToml.Values;

internal partial class TomlString
{
    public override bool TryGetString([NotNullWhen(true)] out string? value)
    {
        value = Utf16String;
        return true;
    }

    public override object GetObject() => value;

}
