
namespace CsToml;

public record CsTomlPackageOptions
{
    public static readonly CsTomlPackageOptions Default = new() { IsDottedKeys = false };
    public static readonly CsTomlPackageOptions DottedKeys = new() { IsDottedKeys = true };

    public bool IsDottedKeys { get; init; }
}
