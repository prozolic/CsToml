
namespace CsToml.Extensions.Configuration.Tests;

internal sealed class Options
{
    public static readonly CsTomlSerializerOptions TomlSpecVersion110 = CsTomlSerializerOptions.Default with
    {
        Spec = TomlSpec.Version110
    };
}