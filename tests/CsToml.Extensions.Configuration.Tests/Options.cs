
namespace CsToml.Extensions.Configuration.Tests;

internal sealed class Options
{
    public static readonly CsTomlSerializerOptions TomlSpecVersion110 = CsTomlSerializerOptions.Default with
    {
        Spec = new TomlSpec()
        {
            AllowUnicodeInBareKeys = true,
            AllowNewlinesInInlineTables = true,
            AllowTrailingCommaInInlineTables = true,
            SupportsEscapeSequenceE = true,
            AllowSecondsOmissionInTime = true,
            SupportsEscapeSequenceX = true,
        }
    };
}