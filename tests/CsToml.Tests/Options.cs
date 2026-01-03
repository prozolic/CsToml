
namespace CsToml.Tests;

internal sealed class Options
{
    public static readonly CsTomlSerializerOptions TomlSpecVersion110 = CsTomlSerializerOptions.Default with
    {
        Spec = TomlSpec.Version110
    };

    public static readonly CsTomlSerializerOptions AllowUnicodeInBareKeys = CsTomlSerializerOptions.Default with
    {
        Spec = new TomlSpec
        {
            AllowUnicodeInBareKeys = true,
        }
    };
}