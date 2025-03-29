
namespace CsToml.Tests;

internal sealed class Options
{
    public static readonly CsTomlSerializerOptions TomlSpecVersion110 = CsTomlSerializerOptions.Default with
    {
        Spec = new TomlSpec()
        {
            AllowNewlinesInInlineTables = true,
            AllowTrailingCommaInInlineTables = true,
            SupportsEscapeSequenceE = true,
            AllowSecondsOmissionInTime = true,
        }
    };
}